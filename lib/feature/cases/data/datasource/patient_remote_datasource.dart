import 'package:flutter/foundation.dart';
import 'package:appsuckhoe/core/data/remote_datasource.dart';
import 'package:appsuckhoe/feature/patient/data/model/patient_model.dart';
import 'package:shared_preferences/shared_preferences.dart';

abstract class PatientRemoteDatasource {
  Future<List<PatientModel>> getAllPatients();
  Future<PatientModel> getPatientById(String id);
  Future<void> createPatient(PatientModel patient);
  Future<void> updatePatient(PatientModel patient);
  Future<void> deletePatient(String id);
}

class PatientRemoteDatasourceImpl implements PatientRemoteDatasource {
  final RemoteDatasource<PatientModel> _remote;

  PatientRemoteDatasourceImpl()
      : _remote = RemoteDatasource<PatientModel>(
          baseUrl: 'http://127.0.0.1:8000/api',
          fromJson: (json) => PatientModel.fromJson(json),
        );

  void _log(String message) {
    if (kDebugMode) {
      debugPrint('🧑‍⚕️ [PATIENT-DS] $message');
    }
  }
  Future <Map<String, String>> _authHeader() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('access_token') ?? '';
    return {
      'Authorization': 'Bearer $token',
      'Accept': 'application/json',
    };
  }
  @override
  Future<List<PatientModel>> getAllPatients() async {
    print('🧑‍⚕️ [PATIENT-DS] GET /patients');
    final result = await _remote.getList('/patients', headers: await _authHeader());
    return result;
  }

  @override
  Future<PatientModel> getPatientById(String id) async {
    _log('GET /patients/$id');
    return await _remote.get('/patients/$id', headers: await _authHeader());
  }

  @override
  Future<void> createPatient(PatientModel patient) async {
    print('🧑‍⚕️ [PATIENT-DS] CREATE PATIENT');
    print('📤 Request body: ${patient.toJson()}');

    await _remote.post(
      '/patients',
      headers: await _authHeader(),
      body: patient.toJson(),
    );

    _log('→ Patient created successfully');
  }

  @override
  Future<void> updatePatient(PatientModel patient) async {
    _log('PUT /patients/${patient.id}');
    _log('Body: ${patient.toJson()}');

    await _remote.put(
      '/patients/${patient.id}',
      headers: await _authHeader(),
      body: patient.toJson(),
    );

    _log('→ Patient updated successfully');
  }

  @override
  Future<void> deletePatient(String id) async {
    _log('DELETE /patients/$id');

    await _remote.delete('/patients/$id', headers: await _authHeader());

    _log('→ Patient deleted successfully');
  }
}
