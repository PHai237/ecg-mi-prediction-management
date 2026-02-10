import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class CreateCase {
  final CaseRepository repository;

  CreateCase(this.repository);

  Future<void> call(Case c) => repository.createCase(c);
}