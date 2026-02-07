class Case{
  final int? id;
  final String patientId;
  final DateTime measuredAt;
  final String status;
  final DateTime? createdAt;
  final DateTime? updatedAt;


  Case({
    this.id,
    required this.patientId,
    required this.measuredAt,
    required this.status,
    this.updatedAt,
    this.createdAt
    });
}
