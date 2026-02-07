import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

typedef FromJson<T> = T Function(Map<String, dynamic> json);

class RemoteDatasource<T> {
  final String baseUrl;
  final FromJson<T> fromJson;

  RemoteDatasource({
    required this.baseUrl,
    required this.fromJson,
  });

  // ================= LOG HELPER =================
  void _logRequest(String method, Uri url, {dynamic body}) {
    debugPrint('➡️ [$method] $url');
    if (body != null) {
      debugPrint('📦 Body: ${jsonEncode(body)}');
    }
  }
  Future<Map<String, String>> _authHeader() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('access_token');

    if (token == null) return {};

    return {
      'Authorization': 'Bearer $token',
    };
  }


  void _logResponse(http.Response response) {
    debugPrint('⬅️ Status: ${response.statusCode}');
    debugPrint('📥 Response: ${response.body}');
  }

  // ================== READ (GET ONE) ==================
  Future<T> get(
    String endpoint, {
    Map<String, String>? headers,
  }) async {
    final url = Uri.parse('$baseUrl$endpoint');
    _logRequest('GET', url);

    final response = await http.get(url, headers: headers);
    _logResponse(response);

    if (response.statusCode != 200) {
      throw Exception('GET failed: ${response.body}');
    }

    final data = jsonDecode(response.body);
    return fromJson(data);
  }

  // ================== READ (GET LIST) ==================
  Future<List<T>> getList(
    String endpoint, {
    Map<String, String>? headers,
  }) async {
    final url = Uri.parse('$baseUrl$endpoint');
    _logRequest('GET LIST', url);

    final response = await http.get(url, headers: headers);
    _logResponse(response);

    if (response.statusCode != 200) {
      throw Exception('GET LIST failed: ${response.body}');
    }

    final List data = jsonDecode(response.body);
    return data.map((e) => fromJson(e)).toList();
  }

  // ================== CREATE (POST) ==================
  Future<T> post(
    String endpoint, {
    Map<String, String>? headers,
    Map<String, dynamic>? body,
  }) async {
    final url = Uri.parse('$baseUrl$endpoint');
    _logRequest('POST', url, body: body);

    final response = await http.post(
      url,
      headers: {
        'Content-Type': 'application/json',
        ...?headers,
      },
      body: jsonEncode(body),
    );

    _logResponse(response);

    if (response.statusCode != 200 && response.statusCode != 201) {
      throw Exception('POST failed: ${response.body}');
    }

    final data = jsonDecode(response.body);
    return fromJson(data);
  }

  // ================== UPDATE (PUT) ==================
  Future<T?> put(
    String endpoint, {
    Map<String, String>? headers,
    Map<String, dynamic>? body,
  }) async {
    final url = Uri.parse('$baseUrl$endpoint');
    _logRequest('PUT', url, body: body);

    final response = await http.put(
      url,
      headers: {
        'Content-Type': 'application/json',
        ...?headers,
      },
      body: jsonEncode(body),
    );

    _logResponse(response);

    if (response.statusCode == 204) {
      return null; // không có body
    }

    if (response.statusCode != 200) {
      throw Exception('PUT failed: ${response.body}');
    }

    final data = jsonDecode(response.body);
    return fromJson(data);
  }


  // ================== UPDATE (PATCH) ==================
  Future<T> patch(
    String endpoint, {
    Map<String, String>? headers,
    Map<String, dynamic>? body,
  }) async {
    final url = Uri.parse('$baseUrl$endpoint');
    _logRequest('PATCH', url, body: body);

    final response = await http.patch(
      url,
      headers: {
        'Content-Type': 'application/json',
        ...?headers,
      },
      body: jsonEncode(body),
    );

    _logResponse(response);

    if (response.statusCode != 200) {
      throw Exception('PATCH failed: ${response.body}');
    }

    final data = jsonDecode(response.body);
    return fromJson(data);
  }

  // ================== DELETE ==================
  Future<void> delete(
    String endpoint, {
    Map<String, String>? headers,
  }) async {
    final url = Uri.parse('$baseUrl$endpoint');
    _logRequest('DELETE', url);

    final response = await http.delete(url, headers: headers);
    _logResponse(response);

    if (response.statusCode != 200 && response.statusCode != 204) {
      throw Exception('DELETE failed: ${response.body}');
    }
  }
}
