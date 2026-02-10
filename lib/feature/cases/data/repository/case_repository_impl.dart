import 'dart:io';

import 'package:appsuckhoe/feature/cases/data/datasource/case_remote_datasource.dart';
import 'package:appsuckhoe/feature/cases/data/model/case_model.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/case_image.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/prediction.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';
import 'package:appsuckhoe/feature/cases/data/model/prediction_model.dart';

class CaseRepositoryImpl implements CaseRepository {
  final CaseRemoteDatasource remoteDatasource;

  CaseRepositoryImpl(this.remoteDatasource);

  @override
  Future<void> createCase(Case c) async {
    final model = CaseModel.fromEntity(c);
    await remoteDatasource.createCase(model);
  }

  @override
  Future<void> deleteCase(String id) async {
    await remoteDatasource.deleteCase(id);
  }

  @override
  Future<List<Case>> getAllCases() async {
    final models = await remoteDatasource.getAllCases();
    return models.map((e) => e).toList();
  }

  @override
  Future<Case> getCaseById(String id) async {
    final model = await remoteDatasource.getCaseById(id);
    return model;
  }

  @override
  Future<void> updateCase(Case c) async {
    final model = CaseModel.fromEntity(c);
    await remoteDatasource.updateCase(model);
  }

  @override
  Future<PredictionModel> predictCase(String id) async {
    return await remoteDatasource.predictCase(id);
  }

  // ✅ CHUẨN: chỉ delegate + trả Entity
  @override
  Future<List<CaseImage>> uploadCaseImages({
    required int caseId,
    required List<File> files,
  }) async {
    return await remoteDatasource.uploadCaseImages(
      caseId: caseId,
      files: files,
    );
  }
  @override
  Future<List<CaseImage>> getCaseImages(int caseId) async {
    final models = await remoteDatasource.getCaseImages(caseId);
    return models.map((m) => m).toList(); // model extends entity
  }

}
