import '../../domain/entities/prediction.dart';

class PredictionModel extends Prediction {
  PredictionModel({
    required super.caseId,
    required super.label,
    required super.confidence,
    required super.predictedAt,
    required super.algorithm,
    required super.note,
  });

  factory PredictionModel.fromJson(Map<String, dynamic> json) {
    return PredictionModel(
      caseId: json['caseId'],
      label: json['label'],
      confidence: (json['confidence'] as num).toDouble(),
      predictedAt: DateTime.parse(json['predictedAt']),
      algorithm: json['algorithm'],
      note: json['note'],
    );
  }
  Map<String, dynamic> toJson() {
    return {
      'id': caseId,
    };
  }
}
