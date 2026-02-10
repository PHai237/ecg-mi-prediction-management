import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';

class CaseModel extends Case {
  CaseModel({
    super.id,
    required super.patientId,
    super.patientCode,
    super.patientName,
    required super.measuredAt,
    super.status,
    required super.note,
    super.imageCount = 0,
    super.createdAt,
    super.createdByUserId,
    super.createdByUsername,
    super.createdByFullName,
    super.createdByTitle,
    super.createdByDepartment,
    super.predictedLabel,
    super.predictedConfidence,
    super.predictedAt,
    super.predictedByUserId,
    super.predictedByUsername,
    super.predictedByFullName,
    super.predictedByTitle,
    super.predictedByDepartment,
  });

  // ================= FROM JSON =================
  factory CaseModel.fromJson(Map<String, dynamic> json) {
    return CaseModel(
      id: json['id'],
      patientId: json['patientId'],
      patientCode: json['patientCode'],
      patientName: json['patientName'],
      measuredAt: DateTime.parse(json['measuredAt']),
      status: json['status'],
      note: json['note'] ?? '',
      imageCount: json['imageCount'] ?? 0,
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : null,
      createdByUserId: json['createdByUserId'],
      createdByUsername: json['createdByUsername'],
      createdByFullName: json['createdByFullName'],
      createdByTitle: json['createdByTitle'],
      createdByDepartment: json['createdByDepartment'],
      predictedLabel: json['predictedLabel'],
      predictedConfidence:
          (json['predictedConfidence'] as num?)?.toDouble(),
      predictedAt: json['predictedAt'] != null
          ? DateTime.parse(json['predictedAt'])
          : null,
      predictedByUserId: json['predictedByUserId'],
      predictedByUsername: json['predictedByUsername'],
      predictedByFullName: json['predictedByFullName'],
      predictedByTitle: json['predictedByTitle'],
      predictedByDepartment: json['predictedByDepartment'],
    );
  }

  // ================= TO JSON (CREATE CASE) =================
  Map<String, dynamic> toJson() {
    return {
      'patientId': patientId,
      'measuredAt': measuredAt.toUtc().toIso8601String(),
      'note': note,
    };
  }

  // ================= FROM ENTITY =================
  factory CaseModel.fromEntity(Case e) => CaseModel(
        id: e.id,
        patientId: e.patientId,
        patientCode: e.patientCode,
        patientName: e.patientName,
        measuredAt: e.measuredAt,
        status: e.status,
        note: e.note,
        imageCount: e.imageCount,
        createdAt: e.createdAt,
        createdByUserId: e.createdByUserId,
        createdByUsername: e.createdByUsername,
        createdByFullName: e.createdByFullName,
        createdByTitle: e.createdByTitle,
        createdByDepartment: e.createdByDepartment,
        predictedLabel: e.predictedLabel,
        predictedConfidence: e.predictedConfidence,
        predictedAt: e.predictedAt,
        predictedByUserId: e.predictedByUserId,
        predictedByUsername: e.predictedByUsername,
        predictedByFullName: e.predictedByFullName,
        predictedByTitle: e.predictedByTitle,
        predictedByDepartment: e.predictedByDepartment,
      );
}
