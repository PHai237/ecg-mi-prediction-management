import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/ecg_image.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/prediction.dart';
import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/case_info.dart';

abstract class CaseRepository {
  Future<List<CaseInfo>> getCasesInfo();
  Future<CaseInfo> getCaseInfoById(int id);
  Future<List<Case>> getCases();
  Future<Case> getCaseById(int id);
  Future<List<EcgImage>> getEcgImages();
  Future<EcgImage> getEcgImageById(int id);
  Future<List<Patient>> getPatients();
  Future<Patient> getPatientById(int id);
  Future<List<Prediction>> getPredictions();
  Future<Prediction> getPredictionById(int id);
}