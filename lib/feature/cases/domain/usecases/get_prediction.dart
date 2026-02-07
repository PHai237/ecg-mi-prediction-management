import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';
import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';

class GetPatient {
  final CaseRepository repo;
  GetPatient(this.repo);

  Future<Patient> call(int id) => repo.getPatientById(id);
}