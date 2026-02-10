import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class DeleteCase {
  final CaseRepository repository;

  DeleteCase(this.repository);

  Future<void> call(String id) => repository.deleteCase(id);
}