import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/case_image.dart';

class GetCaseImages {
  final CaseRepository repository;

  GetCaseImages(this.repository);

  Future<List<CaseImage>> call(int caseId) {
    return repository.getCaseImages(caseId);
  }
}
