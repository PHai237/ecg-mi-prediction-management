
import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class GetCase {
  final CaseRepository repo;

  GetCase(this.repo);
  Future<Case> call(int id) => repo.getCaseById(id);
}