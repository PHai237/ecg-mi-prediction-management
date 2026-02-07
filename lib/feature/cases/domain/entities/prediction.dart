class Prediction {
  final int? id;
  final int caseId;
  final int patientId;
  final String label;
  final String confidence;
  final DateTime? createdat;
  final DateTime? updatedat;

  Prediction({
    this.id,
    required this.caseId,
    required this.patientId,
    required this.label,
    required this.confidence,
    this.createdat,
    this.updatedat,
  });
}
