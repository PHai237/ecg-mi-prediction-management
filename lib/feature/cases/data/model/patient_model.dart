import 'package:appsuckhoe/feature/cases/domain/entities/patient.dart';
class PatientModel extends Patient{
  PatientModel({
    super.id,
    required super.code,
    required super.name,
    required super.birthyear,
    required super.gender,
    required super.note,
    super.createdat,
    super.updatedat,
  });

  factory PatientModel.fromJson(Map<String, dynamic> json) {
    return PatientModel(
      id: json['id'] ?? 0,
      code: json['code'] ?? '',
      name: json['name'] ?? '',
      birthyear: json['birth_year'] ?? 0,
      gender: json['gender'] ?? '',
      note: json['note'] ?? '',
      createdat: json['created_at'] != null
          ? DateTime.parse(json['created_at'])
          : null,
      updatedat: json['updated_at'] != null
          ? DateTime.parse(json['updated_at'])
          : null,
    );
  }
  Map<String, dynamic> toJson() {
    return {
      'code': code,
      'name': name,
      'birth_year': birthyear,
      'gender': gender,
      'note': note, 
    };
  }
  // ================= FROM ENTITY =================
  factory PatientModel.fromEntity(Patient p) => PatientModel(
        code: p.code,
        name: p.name,
        birthyear: p.birthyear,
        gender: p.gender,
        note: p.note,
      );
}