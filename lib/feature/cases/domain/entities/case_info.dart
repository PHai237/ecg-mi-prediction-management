class CaseInfo {
  final int? id;
  final int caseId;
  final int patientId;
  final String status;
  final String fileName;
  final String label;
  final String confidence;
  final DateTime mesuredAt;
  final String gender;
  final DateTime? createdAt;
  final DateTime? updatedAt;
  CaseInfo({
    this.id,
    required this.caseId,
    required this.patientId,
    required this.status,
    required this.fileName,
    required this.label,
    required this.confidence,
    required this.mesuredAt,
    required this.gender,
    this.createdAt,
    this.updatedAt
  });
}