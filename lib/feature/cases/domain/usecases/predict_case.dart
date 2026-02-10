import 'package:appsuckhoe/feature/cases/domain/entities/prediction.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class PredictCase {
  final CaseRepository repository;

  PredictCase(this.repository);

  Future<Prediction> call(String caseId) {
    return repository.predictCase(caseId);
  }
}
