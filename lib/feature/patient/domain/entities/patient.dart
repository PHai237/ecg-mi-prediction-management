class Patient {
  final int? id;
  final String code;
  final String name;
  final String dateOfBirth;
  final String gender;
  final String note;
  final DateTime? createdat;

  Patient({
    this.id,
    required this.code,
    required this.name,
    required this.dateOfBirth,
    required this.gender,
    required this.note,
    this.createdat,
  });
}
