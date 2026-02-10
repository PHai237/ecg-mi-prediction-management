import 'package:flutter/material.dart';

import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
import 'package:appsuckhoe/feature/cases/data/datasource/case_remote_datasource.dart';
import 'package:appsuckhoe/feature/cases/data/repository/case_repository_impl.dart';
import 'package:appsuckhoe/feature/cases/domain/usecases/get_all_cases.dart';
import 'package:appsuckhoe/feature/cases/domain/usecases/create_case.dart';
import 'package:appsuckhoe/feature/cases/domain/usecases/delete_case.dart';
import 'package:appsuckhoe/feature/cases/domain/usecases/update_case.dart';
import 'package:appsuckhoe/feature/cases/domain/usecases/predict_case.dart';
import 'package:appsuckhoe/feature/cases/presentation/page/case_detail_page.dart';

class CaseListPage extends StatefulWidget {
  const CaseListPage({super.key});

  @override
  State<CaseListPage> createState() => _CaseListPageState();
}

class _CaseListPageState extends State<CaseListPage> {
  late final _remote = CaseRemoteDatasourceImpl();
  late final _repo = CaseRepositoryImpl(_remote);

  late final _getAllCases = GetAllCases(_repo);
  late final _createCase = CreateCase(_repo);
  late final _deleteCase = DeleteCase(_repo);
  late final _updateCase = UpdateCase(_repo);
  late final _predictCase = PredictCase(_repo);

  List<Case> _cases = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadCases();
  }

  Future<void> _loadCases() async {
    final data = await _getAllCases();
    if (!mounted) return;
    setState(() {
      _cases = data;
      _loading = false;
    });
  }

  Future<void> _delete(String id) async {
    await _deleteCase(id);
    _loadCases();
  }

  /// ================== DIALOG ==================
  Widget caseDialog(BuildContext context, {Case? c}) {
    final patientIdController =
        TextEditingController(text: c?.patientId.toString() ?? '');
    final measuredAtController =
        TextEditingController(text: c?.measuredAt.toIso8601String() ?? '');
    final noteController =
        TextEditingController(text: c?.note ?? '');

    final isEdit = c != null;

    return AlertDialog(
      title: Text(isEdit ? 'Sửa ca ECG' : 'Thêm ca ECG'),
      content: SingleChildScrollView(
        child: Column(
          children: [
            TextField(
              controller: patientIdController,
              decoration: const InputDecoration(
                labelText: 'Patient ID',
              ),
              keyboardType: TextInputType.number,
            ),
            TextField(
              controller: measuredAtController,
              decoration: const InputDecoration(
                labelText: 'Thời điểm đo (ISO)',
                hintText: '2026-02-09T11:15:00Z',
              ),
            ),
            TextField(
              controller: noteController,
              decoration: const InputDecoration(labelText: 'Ghi chú'),
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Hủy'),
        ),
        TextButton(
          onPressed: () async {
            if (patientIdController.text.isEmpty ||
                measuredAtController.text.isEmpty) {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Vui lòng nhập đủ thông tin')),
              );
              return;
            }

            final newCase = Case(
              id: c?.id,
              patientId: int.parse(patientIdController.text),
              measuredAt: DateTime.parse(measuredAtController.text),
              note: noteController.text.trim(),
            );

            if (isEdit) {
              await _updateCase(newCase);
            } else {
              await _createCase(newCase);
            }

            await _loadCases();
            Navigator.pop(context, true);
          },
          child: const Text('Lưu'),
        ),
      ],
    );
  }

  /// ================== UI ==================
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Case List'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadCases,
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _cases.isEmpty
              ? const Center(child: Text('No cases found'))
              : ListView.builder(
                  itemCount: _cases.length,
                  itemBuilder: (context, index) {
                    final c = _cases[index];

                    return Card(
                      margin: const EdgeInsets.symmetric(
                          horizontal: 12, vertical: 6),
                      child: ListTile(
                        leading: CircleAvatar(
                          child: Text(c.patientId.toString()),
                        ),
                        title: Text(
                          'Case #${c.id}',
                          style: const TextStyle(fontWeight: FontWeight.bold),
                        ),
                        subtitle: Text(
                          'Patient ID: ${c.patientId}\n'
                          'Measured at: ${c.measuredAt.toLocal()}\n'
                          'Note: ${c.note}',
                        ),
                        isThreeLine: true,
                        onTap: () {
                          Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (_) => CaseDetailPage(caseId: c.id!),
                            ),
                          );
                        },
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            IconButton(
                              icon: const Icon(Icons.edit, color: Colors.blue),
                              onPressed: () async {
                                final result = await showDialog<bool>(
                                  context: context,
                                  builder: (ctx) => caseDialog(ctx, c: c),
                                );
                                if (result == true) {
                                  _loadCases();
                                }
                              },
                            ),
                            IconButton(
                              icon: const Icon(Icons.delete, color: Colors.red),
                              onPressed: () => _delete(c.id.toString()),
                            ),
                          ],
                        ),

                      ),
                    );
                  },
                ),
      floatingActionButton: FloatingActionButton(
        onPressed: () async {
          final result = await showDialog<bool>(
            context: context,
            builder: (ctx) => caseDialog(ctx),
          );
          if (result == true) {
            _loadCases();
          }
        },
        child: const Icon(Icons.add),
      ),
      
    );
  }
}
