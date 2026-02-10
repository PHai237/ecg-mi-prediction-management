
import 'package:appsuckhoe/feature/patient/data/datasource/patient_remote_datasource.dart';
import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';
import 'package:appsuckhoe/feature/patient/domain/repositories/patient_repository.dart';
import 'package:appsuckhoe/feature/patient/domain/usecases/create_patient.dart';
import 'package:appsuckhoe/feature/patient/domain/usecases/get_all_patients.dart';
import 'package:appsuckhoe/feature/patient/domain/usecases/get_patient.dart';
import 'package:appsuckhoe/feature/patient/domain/usecases/delete_patient.dart';
import 'package:appsuckhoe/feature/patient/domain/usecases/update_patient.dart';
import 'package:appsuckhoe/feature/patient/data/repository/patient_repository_impl.dart';
import 'package:flutter/material.dart';

class PatientListPage extends StatefulWidget {
  const PatientListPage({super.key});

  @override
  State<PatientListPage> createState() => _PatientListPageState();
}

class _PatientListPageState extends State<PatientListPage> {
  late final _remote = PatientRemoteDatasourceImpl();
  late final _repo = PatientRepositoryImpl(_remote);

  late final _getallPatients = GetAllPatients(_repo);
  late final _deletePatient = DeletePatient(_repo);
  late final _createPatient = CreatePatient(_repo);
  late final _updatePatient = UpdatePatient(_repo);

  List<Patient> _patients = [];
  bool _loading = true;
  
  @override
  void initState() {
    super.initState();
    _loadPatients();
  }
  Future<void> _loadPatients() async {
    final patients = await _getallPatients();
     if (!mounted) return;
    setState(() {
      _patients = patients;
      _loading = false;
    });
  }
  Future<void> _delete(String id) async {
    await _deletePatient(id);
    _loadPatients();
  }
    Widget patientDialog(BuildContext context, {Patient? patient}) {
    final codeController =
        TextEditingController(text: patient?.code ?? '');
    final nameController =
        TextEditingController(text: patient?.name ?? '');
    final dobController =
        TextEditingController(text: patient?.dateOfBirth ?? '');
    final noteController =
        TextEditingController(text: patient?.note ?? '');

    bool gender = patient?.gender ?? true; // true = Nam
    bool isExamined = patient?.isExamined ?? false;

    final isEdit = patient != null;

    return AlertDialog(
      title: Text(isEdit ? 'Sửa bệnh nhân' : 'Thêm bệnh nhân'),
      content: SingleChildScrollView(
        child: Column(
          children: [
            TextField(
              controller: codeController,
              decoration: const InputDecoration(labelText: 'Mã bệnh nhân'),
              enabled: !isEdit,
            ),
            TextField(
              controller: nameController,
              decoration: const InputDecoration(labelText: 'Tên bệnh nhân'),
            ),
            TextField(
              controller: dobController,
              decoration:
                  const InputDecoration(labelText: 'Ngày sinh (yyyy-MM-dd)'),
            ),

            /// 👇 GENDER
            SwitchListTile(
              title: const Text('Giới tính'),
              subtitle: Text(gender ? 'Nam' : 'Nữ'),
              value: gender,
              onChanged: (v) {
                setState(() => gender = v);
              },
            ),

            /// 👇 IS EXAMINED
            SwitchListTile(
              title: const Text('Đã khám'),
              value: isExamined,
              onChanged: (v) {
                setState(() => isExamined = v);
              },
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
            if (codeController.text.isEmpty ||
                nameController.text.isEmpty ||
                dobController.text.isEmpty) {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Vui lòng nhập đủ thông tin')),
              );
              return;
            }

            final newPatient = Patient(
              id: patient?.id,
              code: codeController.text.trim(),
              name: nameController.text.trim(),
              dateOfBirth: dobController.text.trim(),
              gender: gender,
              note: noteController.text.trim(),
              isExamined: isExamined,
            );

            if (isEdit) {
              await _updatePatient(newPatient);
            } else {
              await _createPatient(newPatient);
            }

            await _loadPatients();
            Navigator.pop(context, true);
          },
          child: const Text('Lưu'),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Patient List'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadPatients,
          ),
        ],
      ),

      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _patients.isEmpty
              ? const Center(child: Text('No patients found'))
              : ListView.builder(
                  itemCount: _patients.length,
                  itemBuilder: (context, index) {
                    final p = _patients[index];

                    return Card(
                      margin: const EdgeInsets.symmetric(
                          horizontal: 12, vertical: 6),
                      child: ListTile(
                        leading: CircleAvatar(
                          child: Text(p.id.toString()),
                        ),
                        title: Text(
                          p.name,
                          style: const TextStyle(
                              fontWeight: FontWeight.bold),
                        ),
                        subtitle: Text(
                          'Mã BN: ${p.code}\n'
                          'Ngày sinh: ${p.dateOfBirth}\n'
                          'Giới tính: ${p.gender ? 'Nam' : 'Nữ'}\n'
                          'Đã khám: ${p.isExamined ? 'Rồi' : 'Chưa'}\n'
                          'Ghi chú: ${p.note}\n'
                          'Tạo lúc: ${p.createdAt != null ? p.createdAt!.toLocal() : '-'}',
                        ),
                        isThreeLine: true,
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            IconButton(
                              icon: const Icon(Icons.edit, color: Colors.blue),
                              onPressed: () async {
                                final result = await showDialog<bool>(
                                  context: context,
                                  builder: (ctx) => patientDialog(ctx, patient: p),
                                );
                                if (result == true) {
                                  await _loadPatients();
                                }
                              },
                            ),
                            IconButton(
                              icon: const Icon(Icons.delete, color: Colors.red),
                              onPressed: () => _delete(p.id.toString()),
                            ),
                          ],
                        ),

                      ),
                    );
                  },
                ),
      floatingActionButton: FloatingActionButton(
        onPressed: () async{
            final result = await showDialog<bool>(
              context: context, 
              builder: (ctx) => patientDialog(ctx),);
            if(result == true){
              debugPrint('Dialog dong, load lai danh sach benh nhan');
              await _loadPatients();
            }
        },
        child: const Icon(Icons.add),
      ),
    );
  }
}
