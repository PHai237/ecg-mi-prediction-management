class CaseInfo {
  final int? id;

  /// ID của case
  final int caseId;

  /// ID bệnh nhân
  final int patientId;

  /// Trạng thái ca đo (done / pending / error...)
  final String status;

  /// Tên file ECG đại diện
  final String fileName;

  /// Nhãn dự đoán AI
  final String label;

  /// Độ tin cậy (vd: "0.93" hoặc "93%")
  final String confidence;

  /// Thời điểm đo ECG
  final DateTime measuredAt;

  /// Giới tính bệnh nhân
  final String gender;

  final DateTime? createdAt;
  final DateTime? updatedAt;

  const CaseInfo({
    this.id,
    required this.caseId,
    required this.patientId,
    required this.status,
    required this.fileName,
    required this.label,
    required this.confidence,
    required this.measuredAt,
    required this.gender,
    this.createdAt,
    this.updatedAt,
  });
}
