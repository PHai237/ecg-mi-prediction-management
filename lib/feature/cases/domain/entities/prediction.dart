class Prediction {
  final int caseId;
  final String label;
  final double confidence;
  final DateTime predictedAt;
  final String algorithm;
  final String note;

  Prediction({
    required this.caseId,
    required this.label,
    required this.confidence,
    required this.predictedAt,
    required this.algorithm,
    required this.note,
  });
}
