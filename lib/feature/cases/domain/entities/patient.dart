class Patient {
  final int? id;
  final String code;
  final String name;
  final int birthyear;
  final String gender;
  final String note;
  final DateTime? createdat;
  final DateTime? updatedat;

  Patient({
    this.id,
    required this.code,
    required this.name,
    required this.birthyear,
    required this.gender,
    required this.note,
    this.createdat,
    this.updatedat,
  });
}
