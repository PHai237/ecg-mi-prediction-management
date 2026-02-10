import 'dart:io';
import '../repositories/case_repository.dart';
import '../entities/case_image.dart';

class UploadCaseImages {
  final CaseRepository repository;

  UploadCaseImages(this.repository);

  Future<List<CaseImage>> call({
    required int caseId,
    required List<File> files,
  }) {
    return repository.uploadCaseImages(
      caseId: caseId,
      files: files,
    );
  }
}
