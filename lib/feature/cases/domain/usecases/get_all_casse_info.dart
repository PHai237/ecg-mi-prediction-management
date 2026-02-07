import 'package:appsuckhoe/feature/cases/domain/entities/case_info.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class GetAllCases {
  final CaseRepository repo;

  GetAllCases(this.repo);

  Future<List<CaseInfo>>  call() => repo.getCasesInfo();
}