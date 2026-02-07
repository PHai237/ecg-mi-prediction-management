// feature/auth/data/datasource/auth_remote_datasource.dart
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'package:flutter/foundation.dart';


import '../../domain/entities/user.dart';
import '../../domain/repositories/auth_repository.dart';

class AuthRemoteDataSource implements AuthRepository {
  final String baseUrl;

  AuthRemoteDataSource({required this.baseUrl});

  // ================= LOGIN =================
  @override
Future<User> login(String username, String password) async {
    final url = Uri.parse('$baseUrl/api/auth/login');

    final response = await http.post(
      url,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: jsonEncode({
        'username': username,
        'password': password,
      }),
    );

    debugPrint('📥 Status: ${response.statusCode}');
    debugPrint('📥 Body: ${response.body}');

    if (response.statusCode != 200) {
      throw Exception('Login failed');
    }

    final data = jsonDecode(response.body);

    final token = data['token'];          // ✅ ĐÚNG KEY
    final userJson = data['user'];

    // lưu token
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('access_token', token);
    await prefs.setString('role', userJson['role']);

    return User(
      id: userJson['id'],
      username: userJson['username'],
      role: userJson['role'],
    );
  }


  // ================= LOGOUT =================
  @override
  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('access_token');
    await prefs.remove('role');
  }

  // ================= ME =================
  @override
  Future<User?> me() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('access_token');

    if (token == null) return null;

    final url = Uri.parse('$baseUrl/me'); // hoặc /api/auth/me
    print('🌐 [ME] URL: $url');

    final response = await http.get(
      url,
      headers: {
        'Authorization': 'Bearer $token',
        'Accept': 'application/json',
      },
    );

    print('📥 Status: ${response.statusCode}');
    print('📥 Body: ${response.body}');

    if (response.statusCode != 200) return null;

    final user = jsonDecode(response.body);

    return User(
      id: int.parse(user['uid'].toString()),
      username: user['username'],
      role: user['role'],
    );
  }
}
