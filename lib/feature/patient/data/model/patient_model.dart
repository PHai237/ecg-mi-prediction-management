import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';
class PatientModel extends Patient{
  PatientModel({
    super.id,
    required super.code,
    required super.name,
    required super.dateOfBirth,
    required super.gender,
    required super.note,
    super.createdat,
  });

  factory PatientModel.fromJson(Map<String, dynamic> json) {
    return PatientModel(
      id: json['id'] ?? 0,
      code: json['code'] ?? '',
      name: json['name'] ?? '',
      dateOfBirth: json['dateOfBirth'] ?? '',
      gender: json['gender'] ?? '',
      note: json['note'] ?? '',
      createdat: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : null,
    );
  }
  Map<String, dynamic> toJson() {
    return {
      'code': code,
      'name': name,
      'dateOfBirth': dateOfBirth,
      'gender': gender,
      'note': note, 
      'createdAt': createdat?.toIso8601String(),
    };
  }
  // ================= FROM ENTITY =================
  factory PatientModel.fromEntity(Patient p) => PatientModel(
        id:p.id,
        code: p.code,
        name: p.name,
        dateOfBirth: p.dateOfBirth,
        gender: p.gender,
        note: p.note,
      );
}