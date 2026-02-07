import 'package:appsuckhoe/feature/patient/domain/repositories/patient_repository.dart';
import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';
class UpdatePatient {
  final PatientRepository repository;

  UpdatePatient(this.repository);
  Future<void> call(Patient patient) => repository.updatePatient(patient);
}