import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class GetAllCases {
  final CaseRepository repo;

  GetAllCases(this.repo);

  Future<List<Case>>  call() => repo.getCases();
}