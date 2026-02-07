import 'package:appsuckhoe/feature/cases/domain/entities/prediction.dart';

class PredictionModel extends Prediction {
  PredictionModel({
    super.id,
    required super.caseId,
    required super.patientId,
    required super.label,
    required super.confidence,
    super.createdat,
    super.updatedat,
  });

  // ================= FROM JSON =================
  factory PredictionModel.fromJson(Map<String, dynamic> json) {
    return PredictionModel(
      id: json['id'] ?? 0,
      caseId: json['case_id'] ?? 0,
      patientId: json['patient_id'] ?? 0,
      label: json['label'] ?? '',
      confidence: json['confidence'] ?? '',
      createdat: json['created_at'] != null
          ? DateTime.parse(json['created_at'])
          : null,
      updatedat: json['updated_at'] != null
          ? DateTime.parse(json['updated_at'])
          : null,
    );
  }

  // ================= FROM ENTITY =================
  factory PredictionModel.fromEntity(Prediction p) => PredictionModel(
        id: p.id,
        caseId: p.caseId,
        patientId: p.patientId,
        label: p.label,
        confidence: p.confidence,
        createdat: p.createdat,
        updatedat: p.updatedat,
      );
}
