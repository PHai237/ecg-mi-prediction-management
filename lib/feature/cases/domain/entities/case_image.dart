class CaseImage {
  final int id;
  final String url;
  final String contentType;

  CaseImage({
    required this.id,
    required this.url,
    required this.contentType,
  });

  factory CaseImage.fromJson(Map<String, dynamic> json) {
    return CaseImage(
      id: json['id'],
      url: json['url'],
      contentType: json['contentType'],
    );
  }
}
