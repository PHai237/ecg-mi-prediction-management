import 'package:appsuckhoe/feature/cases/domain/entities/case.dart';

class CaseModel extends Case {
  CaseModel({
    super.id,
    required super.patientId,
    required super.measuredAt,
    required super.status,
    super.createdAt,
    super.updatedAt,
  });

  // ================= FROM JSON =================
  factory CaseModel.fromJson(Map<String, dynamic> json) {
    return CaseModel(
      id: json['id'] ?? 0,
      patientId: json['patient_id'] ?? '',
      measuredAt: json['measure_at'] ?? '',
      status: json['status'] ?? 0,
      createdAt: json['created_at'] != null
          ? DateTime.parse(json['created_at'])
          : null,
      updatedAt: json['updated_at'] != null
          ? DateTime.parse(json['updated_at'])
          : null,
    );
  }

  // ================= TO JSON =================
  Map<String, dynamic> toJson() {
    return {
      'patient_id': patientId,
      'measure_at': measuredAt,
      'status': status,
    };
  }

  // ================= FROM ENTITY =================
  factory CaseModel.fromEntity(Case c) => CaseModel(
        id: c.id,
        patientId: c.patientId,
        measuredAt: c.measuredAt,
        status: c.status,
        createdAt: c.createdAt,
        updatedAt: c.updatedAt,
      );
}
