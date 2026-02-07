import 'package:appsuckhoe/feature/cases/domain/entities/ecg_image.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class GetAllEcgImage {
  final CaseRepository repo;
  GetAllEcgImage(this.repo);

  Future<List<EcgImage>> call() => repo.getEcgImages();
}