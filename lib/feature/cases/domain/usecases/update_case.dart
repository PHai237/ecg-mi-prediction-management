import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
class UpdateCase {
  final CaseRepository repository;

  UpdateCase(this.repository);
  Future<void> call(Case c) => repository.updateCase(c);
}