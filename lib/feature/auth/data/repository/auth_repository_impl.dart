import 'package:appsuckhoe/feature/auth/data/datasource/auth_remote_datasource.dart';
import 'package:appsuckhoe/feature/auth/domain/entities/user.dart';
import 'package:appsuckhoe/feature/auth/domain/repositories/auth_repository.dart';

class AuthRepositoryImpl implements AuthRepository {
  final AuthRemoteDataSource remoteDataSource;
  AuthRepositoryImpl({required this.remoteDataSource});

  @override
  Future<User> login(String username, String password) {
    return remoteDataSource.login(username, password);
  }

  @override
  Future<void> logout() {
    return remoteDataSource.logout();
  }

  @override
  Future<User?> me() {
    return remoteDataSource.me();
  }
}