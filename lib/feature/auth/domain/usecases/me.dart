import 'package:appsuckhoe/feature/auth/domain/entities/user.dart';
import 'package:appsuckhoe/feature/auth/domain/repositories/auth_repository.dart';
class Me {
  final AuthRepository repository;

  Me(this.repository);

  Future<User?> call() {
    return repository.me();
  }
}