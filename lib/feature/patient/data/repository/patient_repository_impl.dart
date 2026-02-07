import 'package:appsuckhoe/feature/patient/data/datasource/patient_remote_datasource.dart';
import 'package:appsuckhoe/feature/patient/data/model/patient_model.dart';
import 'package:appsuckhoe/feature/patient/domain/entities/patient.dart';
import 'package:appsuckhoe/feature/patient/domain/repositories/patient_repository.dart';

class PatientRepositoryImpl implements PatientRepository {
  final PatientRemoteDatasource remoteDatasource;
  PatientRepositoryImpl(this.remoteDatasource);
  @override
  Future<void> createPatient(Patient patient) async{
    PatientModel patientModel = PatientModel.fromEntity(patient);
    return await remoteDatasource.createPatient(patientModel);
  }
  @override
  Future<void> deletePatient(String id) async{
    return await remoteDatasource.deletePatient(id);
  }
  @override
  Future<List<Patient>> getAllPatients() async{
    List<PatientModel> patientModels = await remoteDatasource.getAllPatients();
    return patientModels.map((model) => model).toList();
    //return patientModels;
  }
  @override
  Future<Patient> getPatientById(String id) async{
    PatientModel? patientModel = await remoteDatasource.getPatientById(id);
    return patientModel!;
  }
    

  @override
  Future<void> updatePatient(Patient patient) async{
    PatientModel patientModel = PatientModel.fromEntity(patient);
    await remoteDatasource.updatePatient(patientModel);
  }
}