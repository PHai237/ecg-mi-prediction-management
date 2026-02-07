import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';
import 'package:appsuckhoe/feature/patient/domain/repositories/patient_repository.dart';

class CreatePatient {
  final PatientRepository repository;

  CreatePatient(this.repository);

  Future<void> call(Patient p) => repository.createPatient(p);
}