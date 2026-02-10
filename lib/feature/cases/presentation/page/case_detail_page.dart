import 'dart:io';

import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';

import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
import 'package:appsuckhoe/feature/cases/data/datasource/case_remote_datasource.dart';
import 'package:appsuckhoe/feature/cases/data/repository/case_repository_impl.dart';
import 'package:appsuckhoe/feature/cases/domain/usecases/get_case.dart';
import 'package:appsuckhoe/feature/cases/domain/usecases/predict_case.dart';
import 'package:appsuckhoe/feature/cases/domain/usecases/upload_case_image.dart';

class CaseDetailPage extends StatefulWidget {
  final int caseId;

  const CaseDetailPage({super.key, required this.caseId});

  @override
  State<CaseDetailPage> createState() => _CaseDetailPageState();
}

class _CaseDetailPageState extends State<CaseDetailPage> {
  late final _repo =
      CaseRepositoryImpl(CaseRemoteDatasourceImpl());

  late final _getCase = GetCase(_repo);
  late final _predictCase = PredictCase(_repo);
  late final _uploadImages = UploadCaseImages(_repo);

  final ImagePicker _picker = ImagePicker();

  Case? _case;
  bool _loading = true;
  bool _predicting = false;
  bool _uploading = false;

  List<File> _images = [];

  @override
  void initState() {
    super.initState();
    _loadCase();
  }

  Future<void> _loadCase() async {
    final data = await _getCase(widget.caseId.toString());
    if (!mounted) return;

    setState(() {
      _case = data;
      _loading = false;
    });
  }

  // ================= HELPER UI =================
  Widget infoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 150,
            child: Text(
              label,
              style: const TextStyle(
                fontWeight: FontWeight.w600,
                color: Colors.grey,
              ),
            ),
          ),
          Expanded(child: Text(value.isEmpty ? '-' : value)),
        ],
      ),
    );
  }

  // ================= PICK IMAGES =================
  Future<void> _pickImages() async {
    final picked = await _picker.pickMultiImage();
    if (picked.isEmpty) return;

    setState(() {
      _images = picked.map((e) => File(e.path)).toList();
    });
  }

  // ================= UPLOAD IMAGES =================
  Future<void> _uploadImagesToCase() async {
    if (_images.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Vui lòng chọn ảnh ECG')),
      );
      return;
    }

    setState(() => _uploading = true);

    try {
      await _uploadImages(
        caseId: widget.caseId,
        files: _images,
      );

      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Upload ảnh ECG thành công')),
      );

      _images.clear();
      _loadCase();
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.toString())),
      );
    } finally {
      if (mounted) setState(() => _uploading = false);
    }
  }

  // ================= PREDICT =================
  Future<void> _predict() async {
    setState(() => _predicting = true);

    try {
      final prediction =
          await _predictCase(widget.caseId.toString());

      if (!mounted) return;

      showDialog(
        context: context,
        builder: (dialogContext) => AlertDialog(
          title: const Text('Kết quả dự đoán ECG'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Kết quả: ${prediction.label}'),
              Text(
                'Độ tin cậy: ${(prediction.confidence * 100).toStringAsFixed(1)}%',
              ),
              const SizedBox(height: 8),
              Text('Thuật toán: ${prediction.algorithm}'),
              if (prediction.note != null)
                Text('Ghi chú: ${prediction.note}'),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(dialogContext).pop();
                Navigator.of(context).pop();
              },
              child: const Text('OK'),
            ),
          ],
        ),
      );
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.toString())),
      );
    } finally {
      if (mounted) setState(() => _predicting = false);
    }
  }

  // ================= UI =================
  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    final c = _case!;

    return Scaffold(
      appBar: AppBar(
        title: Text('Case #${c.id}'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // ================= CASE INFO =================
            Text('Thông tin Case',
                style: Theme.of(context).textTheme.titleMedium),
            const Divider(),

            infoRow('Case ID', c.id.toString()),
            infoRow('Status', c.status ?? '-'),
            infoRow('Measured At', c.measuredAt.toLocal().toString()),
            infoRow('Image Count', c.imageCount.toString()),

            const SizedBox(height: 16),

            // ================= PATIENT =================
            Text('Thông tin bệnh nhân',
                style: Theme.of(context).textTheme.titleMedium),
            const Divider(),

            infoRow('Patient ID', c.patientId.toString()),
            infoRow('Patient Code', c.patientCode ?? '-'),
            infoRow('Patient Name', c.patientName ?? '-'),

            const SizedBox(height: 16),

            // ================= PREDICTION =================
            Text('Kết quả dự đoán',
                style: Theme.of(context).textTheme.titleMedium),
            const Divider(),

            infoRow('Label', c.predictedLabel ?? '-'),
            infoRow(
              'Confidence',
              c.predictedConfidence != null
                  ? '${(c.predictedConfidence! * 100).toStringAsFixed(2)}%'
                  : '-',
            ),
            infoRow(
              'Predicted At',
              c.predictedAt?.toLocal().toString() ?? '-',
            ),

            const SizedBox(height: 16),

            // ================= CREATED BY =================
            Text('Người tạo',
                style: Theme.of(context).textTheme.titleMedium),
            const Divider(),

            infoRow(
              'Created At',
              c.createdAt?.toLocal().toString() ?? '-',
            ),
            infoRow('Username', c.createdByUsername ?? '-'),
            infoRow('Full Name', c.createdByFullName ?? '-'),
            infoRow('Title', c.createdByTitle ?? '-'),
            infoRow('Department', c.createdByDepartment ?? '-'),

            const SizedBox(height: 24),

            // ================= UPLOAD ECG =================
            Text('Ảnh ECG',
                style: Theme.of(context).textTheme.titleMedium),
            const SizedBox(height: 8),

            Row(
              children: [
                ElevatedButton.icon(
                  icon: const Icon(Icons.image),
                  label: const Text('Chọn ảnh'),
                  onPressed: _pickImages,
                ),
                const SizedBox(width: 12),
                ElevatedButton.icon(
                  icon: const Icon(Icons.cloud_upload),
                  label: _uploading
                      ? const SizedBox(
                          width: 16,
                          height: 16,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Upload'),
                  onPressed:
                      _uploading ? null : _uploadImagesToCase,
                ),
              ],
            ),

            const SizedBox(height: 12),

            if (_images.isNotEmpty)
              SizedBox(
                height: 100,
                child: ListView(
                  scrollDirection: Axis.horizontal,
                  children: _images.map((file) {
                    return Padding(
                      padding: const EdgeInsets.only(right: 8),
                      child: Image.file(
                        file,
                        width: 100,
                        fit: BoxFit.cover,
                      ),
                    );
                  }).toList(),
                ),
              ),

            const SizedBox(height: 32),

            // ================= PREDICT =================
            SizedBox(
              width: double.infinity,
              height: 48,
              child: ElevatedButton.icon(
                icon: const Icon(Icons.psychology),
                label: _predicting
                    ? const CircularProgressIndicator(color: Colors.white)
                    : const Text('Predict ECG'),
                onPressed:
                    (_uploading || _predicting) ? null : _predict,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
