

import 'package:appsuckhoe/feature/cases/domain/entities/prediction.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class GetAllPrediction {
  final CaseRepository repo;
  GetAllPrediction(this.repo);

  Future<List<Prediction>> call() => repo.getPredictions();
}