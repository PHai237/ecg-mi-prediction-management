import 'dart:io';

import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/case_image.dart';
import 'package:go_router/go_router.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/prediction.dart';
abstract class CaseRepository {
  Future<List<Case>> getAllCases();
  Future<void> createCase(Case c);
  Future<void> deleteCase(String id);
  Future<Case> getCaseById(String id);
  Future<void> updateCase(Case c);
  Future<Prediction> predictCase(String id);
  // upload trả về entity CaseImage
  Future<List<CaseImage>> uploadCaseImages({
    required int caseId,
    required List<File> files,
  });
  Future<List<CaseImage>> getCaseImages(int caseId);
}
