import 'package:appsuckhoe/feature/patient/domain/repositories/patient_repository.dart';
class GetPatient {
  final PatientRepository repository;

  GetPatient(this.repository);

  Future<void> call(String id) => repository.getPatientById(id);
}