import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';

class PatientModel extends Patient {
  PatientModel({
    super.id,
    required super.code,
    required super.name,
    required super.dateOfBirth,
    required super.gender,
    required super.isExamined,
    required super.note,
    super.isActive,
    super.deactivatedAt,
    super.deactivatedByUserId,
    super.createdAt,
  });

  // ================= FROM JSON =================
  factory PatientModel.fromJson(Map<String, dynamic> json) {
    return PatientModel(
      id: json['id'],
      code: json['code'] ?? '',
      name: json['name'] ?? '',
      dateOfBirth: json['dateOfBirth'] ?? '',
      gender: json['gender'] ?? false,
      isExamined: json['isExamined'] ?? false,
      note: json['note'] ?? '',
      isActive: json['isActive'] ?? true,
      deactivatedAt: json['deactivatedAt'] != null
          ? DateTime.parse(json['deactivatedAt'])
          : null,
      deactivatedByUserId: json['deactivatedByUserId'],
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : null,
    );
  }

  // ================= TO JSON (POST / PATCH) =================
  Map<String, dynamic> toJson() {
    return {
      'code': code,
      'name': name,
      'dateOfBirth': dateOfBirth,
      'gender': gender,
      'note': note,
      'isExamined': isExamined,
    };
  }

  // ================= FROM ENTITY =================
  factory PatientModel.fromEntity(Patient p) => PatientModel(
        id: p.id,
        code: p.code,
        name: p.name,
        dateOfBirth: p.dateOfBirth,
        gender: p.gender,
        isExamined: p.isExamined,
        note: p.note,
        isActive: p.isActive,
        deactivatedAt: p.deactivatedAt,
        deactivatedByUserId: p.deactivatedByUserId,
        createdAt: p.createdAt,
      );
}
