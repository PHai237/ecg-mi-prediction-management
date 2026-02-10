import 'dart:convert';
import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

import 'package:appsuckhoe/core/data/remote_datasource.dart';
import 'package:appsuckhoe/feature/cases/data/model/case_model.dart';
import 'package:appsuckhoe/core/config/api_config.dart';
import 'package:mime/mime.dart';
import 'package:http_parser/http_parser.dart';
import 'package:appsuckhoe/feature/cases/domain/entities/case_image.dart';
import 'package:appsuckhoe/feature/cases/data/model/case_image_model.dart';
import 'package:appsuckhoe/feature/cases/data/model/prediction_model.dart';

abstract class CaseRemoteDatasource {
  Future<List<CaseModel>> getAllCases();
  Future<CaseModel> getCaseById(String id);
  Future<void> createCase(CaseModel c);
  Future<void> updateCase(CaseModel c);
  Future<void> deleteCase(String id);
  Future<PredictionModel> predictCase(String id);
  Future<List<CaseImage>> uploadCaseImages({
    required int caseId,
    required List<File> files,
  });
  Future<List<CaseImageModel>> getCaseImages(int caseId);
}

class CaseRemoteDatasourceImpl implements CaseRemoteDatasource {
  final RemoteDatasource<CaseModel> _remote;

  CaseRemoteDatasourceImpl()
      : _remote = RemoteDatasource<CaseModel>(
          baseUrl: '${ApiConfig.baseUrl}/api',
          fromJson: (json) => CaseModel.fromJson(json),
        );

  // ================= LOG =================
  void _log(String message) {
    if (kDebugMode) {
      debugPrint('📊 [CASE-DS] $message');
    }
  }

  // ================= AUTH HEADER =================
  Future<Map<String, String>> _authHeader() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('access_token') ?? '';
    return {
      'Authorization': 'Bearer $token',
      'Accept': 'application/json',
    };
  }

  // ================= GET ALL =================
  @override
  Future<List<CaseModel>> getAllCases() async {
    _log('GET /cases');
    return await _remote.getList(
      '/cases',
      headers: await _authHeader(),
    );
  }

  // ================= GET BY ID =================
  @override
  Future<CaseModel> getCaseById(String id) async {
    _log('GET /cases/$id');
    return await _remote.get(
      '/cases/$id',
      headers: await _authHeader(),
    );
  }

  // ================= CREATE =================
  @override
  Future<void> createCase(CaseModel c) async {
    _log('POST /cases');
    _log('📤 Body: ${c.toJson()}');

    await _remote.post(
      '/cases',
      headers: await _authHeader(),
      body: c.toJson(),
    );
  }

  // ================= UPDATE =================
  @override
  Future<void> updateCase(CaseModel c) async {
    _log('PUT /cases/${c.id}');
    await _remote.put(
      '/cases/${c.id}',
      headers: await _authHeader(),
      body: c.toJson(),
    );
  }

  // ================= DELETE =================
  @override
  Future<void> deleteCase(String id) async {
    _log('DELETE /cases/$id');
    await _remote.delete(
      '/cases/$id',
      headers: await _authHeader(),
    );
  }

  // ================= PREDICT =================
  @override
  Future<PredictionModel> predictCase(String id) async {
    _log('POST /cases/$id/predict');

    final response = await _remote.postRaw(
      '/cases/$id/predict',
      headers: await _authHeader(),
    );

    return PredictionModel.fromJson(response);
  }



  // ================= UPLOAD IMAGES =================
  @override
  Future<List<CaseImage>> uploadCaseImages({
    required int caseId,
    required List<File> files,
  }) async {
    _log('POST /cases/$caseId/images');

    final uri =
        Uri.parse('${ApiConfig.baseUrl}/api/cases/$caseId/images');

    final request = http.MultipartRequest('POST', uri);
    request.headers.addAll(await _authHeader());

    for (final file in files) {
      final mimeType = lookupMimeType(file.path);

      if (mimeType == null || !mimeType.startsWith('image/')) {
        throw Exception('File không phải ảnh hợp lệ');
      }

      final parts = mimeType.split('/');

      request.files.add(
        await http.MultipartFile.fromPath(
          'files',
          file.path,
          contentType: MediaType(parts[0], parts[1]),
        ),
      );
    }

    final response = await request.send();
    final body = await response.stream.bytesToString();

    _log('⬅️ Status: ${response.statusCode}');
    _log('📥 Body: $body');

    if (response.statusCode == 200) {
      final List decoded = jsonDecode(body);
      return decoded
          .map((e) => CaseImage.fromJson(e))
          .toList();
    }

    throw Exception('Upload ảnh thất bại: $body');
  }
@override
  Future<List<CaseImageModel>> getCaseImages(int caseId) async {
    _log('GET /cases/$caseId/images');

    final response = await http.get(
      Uri.parse('${ApiConfig.baseUrl}/api/cases/$caseId/images'),
      headers: await _authHeader(),
    );

    _log('⬅️ Status: ${response.statusCode}');
    _log('📥 Body: ${response.body}');

    if (response.statusCode == 200) {
      final List decoded = jsonDecode(response.body);
      return decoded
          .map((e) => CaseImageModel.fromJson(e))
          .toList();
    }

    throw Exception('Lấy ảnh case thất bại');
  }

}
