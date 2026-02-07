
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';
import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';

class GetAllPatient {
  final CaseRepository repo;
  GetAllPatient(this.repo);

  Future<List<Patient>> call() => repo.getPatients();
}