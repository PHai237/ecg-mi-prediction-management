
import 'package:appsuckhoe/feature/cases/domain/entities/case_info.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class GetCaseInfo {
  final CaseRepository repo;

  GetCaseInfo(this.repo);
  Future<CaseInfo> call(int id) => repo.getCaseInfoById(id);
}