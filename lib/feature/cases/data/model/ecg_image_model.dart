import 'package:appsuckhoe/feature/cases/domain/entities/ecg_image.dart';

class EcgImageModel extends EcgImage {
  EcgImageModel({
    super.id,
    required super.caseId,
    required super.filePath,
    required super.fileName,
    super.createdAt,
    super.updatedAt,
  });

  // ================= FROM JSON =================
  factory EcgImageModel.fromJson(Map<String, dynamic> json) {
    return EcgImageModel(
      id: json['id'] ?? 0,
      caseId: json['case_id'] ?? '',
      filePath: json['file_path'] ?? '',
      fileName: json['file_name'] ?? '',
      createdAt: json['created_at'] != null
          ? DateTime.parse(json['created_at'])
          : null,
      updatedAt: json['updated_at'] != null
          ? DateTime.parse(json['updated_at'])
          : null,
    );
  }

  // ================= FROM ENTITY =================
  factory EcgImageModel.fromEntity(EcgImage e) => EcgImageModel(
        id: e.id,
        caseId: e.caseId,
        filePath: e.filePath,
        fileName: e.fileName,
        createdAt: e.createdAt,
        updatedAt: e.updatedAt,
  );
}