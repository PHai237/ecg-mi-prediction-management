import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';
import 'package:appsuckhoe/feature/patient/domain/repositories/patient_repository.dart';

class GetAllPatients {
  final PatientRepository repository;

  GetAllPatients(this.repository);

  Future<List<Patient>> call() => repository.getAllPatients();
}