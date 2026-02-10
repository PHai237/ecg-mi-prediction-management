class Case {
  final int? id;

  // Patient
  final int patientId;
  final String? patientCode;
  final String? patientName;

  // Case info
  final DateTime measuredAt;
  final String? status;
  final String note;
  final int imageCount;

  // Created info
  final DateTime? createdAt;
  final int? createdByUserId;
  final String? createdByUsername;
  final String? createdByFullName;
  final String? createdByTitle;
  final String? createdByDepartment;

  // Prediction
  final String? predictedLabel;
  final double? predictedConfidence;
  final DateTime? predictedAt;
  final int? predictedByUserId;
  final String? predictedByUsername;
  final String? predictedByFullName;
  final String? predictedByTitle;
  final String? predictedByDepartment;

  Case({
    this.id,
    required this.patientId,
    this.patientCode,
    this.patientName,
    required this.measuredAt,
    this.status,
    required this.note,
    this.imageCount = 0,
    this.createdAt,
    this.createdByUserId,
    this.createdByUsername,
    this.createdByFullName,
    this.createdByTitle,
    this.createdByDepartment,
    this.predictedLabel,
    this.predictedConfidence,
    this.predictedAt,
    this.predictedByUserId,
    this.predictedByUsername,
    this.predictedByFullName,
    this.predictedByTitle,
    this.predictedByDepartment,
  });
}
