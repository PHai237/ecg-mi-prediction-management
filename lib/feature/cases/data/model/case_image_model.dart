
import 'package:appsuckhoe/feature/cases/domain/entities/case_image.dart';
class CaseImageModel extends CaseImage {
  CaseImageModel({
    required super.id,
    required super.url,
    required super.contentType,
  });

  factory CaseImageModel.fromJson(Map<String, dynamic> json) {
    return CaseImageModel(
      id: json['id'],
      url: json['url'],
      contentType: json['contentType'],
    );
  }
}
