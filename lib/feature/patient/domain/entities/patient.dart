class Patient {
  final int? id;
  final String code;
  final String name;
  final String dateOfBirth; // giữ String cho dễ bind UI
  final bool gender;        // true / false
  final bool isExamined;
  final String note;
  final bool? isActive;
  final DateTime? deactivatedAt;
  final int? deactivatedByUserId;
  final DateTime? createdAt;

  Patient({
    this.id,
    required this.code,
    required this.name,
    required this.dateOfBirth,
    required this.gender,
    required this.isExamined,
    required this.note,
    this.isActive,
    this.deactivatedAt,
    this.deactivatedByUserId,
    this.createdAt,
  });
}