class EcgImage{
  final int? id;
  final String caseId;
  final String filePath;
  final String fileName;
  final DateTime? createdAt;
  final DateTime? updatedAt;


  EcgImage({
    this.id,
    required this.caseId,
    required this.filePath,
    required this.fileName,
    this.updatedAt,
    this.createdAt
    });
}
