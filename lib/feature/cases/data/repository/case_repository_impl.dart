// import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';
// import 'package:appsuckhoe/feature/cases/domain/entities/ecg_image.dart';
// import 'package:appsuckhoe/feature/cases/domain/entities/prediction.dart';
// import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';

// import 'package:appsuckhoe/feature/cases/data/datasource/case_remote_datasource.dart';
// import 'package:appsuckhoe/feature/cases/data/datasource/ecg_image_remote_datasource.dart';
// import 'package:appsuckhoe/feature/cases/data/datasource/prediction_remote_datasource.dart';
// import 'package:appsuckhoe/feature/patient/data/datasource/patient_remote_datasource.dart';
// import 'package:appsuckhoe/feature/cases/domain/entities/case_info.dart';

// import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

// class CaseRepositoryImpl implements CaseRepository {
//   final CaseRemoteDatasource caseRemoteDatasource;
//   final EcgImageRemoteDatasource ecgImageRemoteDatasource;
//   final PredictionRemoteDatasource predictionRemoteDatasource;
//   final PatientRemoteDatasource patientRemoteDatasource;

//   CaseRepositoryImpl({
//     required this.caseRemoteDatasource,
//     required this.ecgImageRemoteDatasource,
//     required this.predictionRemoteDatasource,
//     required this.patientRemoteDatasource,
//   });
//   // ================= CASE INFO =================
//   @override
//   Future<List<CaseInfo>> getCasesInfo() async {
//     final cases = await caseRemoteDatasource.getAllCases();

//     final List<CaseInfo> results = [];

//     for (final c in cases) {
//       final patient =
//           await patientRemoteDatasource.getPatientById(c.patientId.toString());

//       final prediction =
//           await predictionRemoteDatasource.getPredictionByCaseId(c.id!);

//       final images =
//           await ecgImageRemoteDatasource.getImagesByCaseId(c.id!);

//       results.add(
//         CaseInfo(
//           id: c.id,
//           caseId: c.id.toString(),
//           patientId: c.patientId,
//           status: c.status,
//           fileName: images.isNotEmpty ? images.first.fileName : '',
//           label: prediction.label,
//           confidence: prediction.confidence,
//           mesuredAt: c.measuredAt,
//           gender: patient.gender,
//           createdAt: c.createdAt,
//           updatedAt: c.updatedAt,
//         ),
//       );
//     }
//     return results;
//   }
//   @override
//   Future<CaseInfo> getCaseInfoById(int id) async {
//     final c = await caseRemoteDatasource.getCaseById(id);

//     final patient =
//         await patientRemoteDatasource.getPatientById(c.patientId.toString());

//     final prediction =
//         await predictionRemoteDatasource.getPredictionByCaseId(c.id!);

//     final images =
//         await ecgImageRemoteDatasource.getImagesByCaseId(c.id!);

//     return CaseInfo(
//       id: c.id,
//       caseId: c.id.toString(),
//       patientId: c.patientId,
//       status: c.status,
//       fileName: images.isNotEmpty ? images.first.fileName : '',
//       label: prediction.label,
//       confidence: prediction.confidence,
//       mesuredAt: c.measuredAt,
//       gender: patient.gender,
//       createdAt: c.createdAt,
//       updatedAt: c.updatedAt,
//     );
//   }

//   // ================= CASE =================
//   @override
//   Future<List<Case>> getCases() async {
//     final models = await caseRemoteDatasource.getAllCases();
//     return models;
//   }

//   @override
//   Future<Case> getCaseById(int id) async {
//     final model = await caseRemoteDatasource.getCaseById(id);
//     return model;
//   }

//   // ================= ECG IMAGE =================
//   @override
//   Future<List<EcgImage>> getEcgImages() async {
//     final models = await ecgImageRemoteDatasource.getAllEcgImages();
//     return models;
//   }

//   @override
//   Future<EcgImage> getEcgImageById(int id) async {
//     final model = await ecgImageRemoteDatasource.getEcgImageById(id);
//     return model;
//   }

//   // ================= PATIENT =================
//   @override
//   Future<List<Patient>> getPatients() async {
//     final models = await patientRemoteDatasource.getAllPatients();
//     return models;
//   }

//   @override
//   Future<Patient> getPatientById(int id) async {
//     final model = await patientRemoteDatasource.getPatientById(id.toString());
//     return model;
//   }

//   // ================= PREDICTION =================
//   @override
//   Future<List<Prediction>> getPredictions() async {
//     final models = await predictionRemoteDatasource.getAllPredictions();
//     return models;
//   }

//   @override
//   Future<Prediction> getPredictionById(int id) async {
//     final model = await predictionRemoteDatasource.getPredictionById(id);
//     return model;
//   }
// }
