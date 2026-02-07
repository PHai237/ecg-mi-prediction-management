import 'package:appsuckhoe/feature/auth/domain/entities/user.dart';
class UserModel extends User {
  UserModel({
    required super.id,
    required super.username,
    required super.role,
  });

  factory UserModel.fromJson(Map<String, dynamic> json) {
    return UserModel(
      id: json['uid'],
      username: json['username'],
      role: json['role'],
    );
  }
}
