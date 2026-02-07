import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';
abstract class PatientRepository {
  Future<List<Patient>> getAllPatients();
  Future<Patient> getPatientById(String id);
  Future<void> createPatient(Patient patient);
  Future<void> updatePatient(Patient patient);
  Future<void> deletePatient(String id);
}
