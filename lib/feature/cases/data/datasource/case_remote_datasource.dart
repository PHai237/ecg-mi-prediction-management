import 'package:flutter/foundation.dart';
import 'package:appsuckhoe/core/data/remote_datasource.dart';
import 'package:appsuckhoe/feature/cases/data/model/case_model.dart';
import 'package:shared_preferences/shared_preferences.dart';

abstract class CaseRemoteDatasource {
  Future<List<CaseModel>> getAllCases();
  Future<CaseModel> getCaseById(int id);
}

class CaseRemoteDatasourceImpl implements CaseRemoteDatasource {
  final RemoteDatasource<CaseModel> _remote;

  CaseRemoteDatasourceImpl()
      : _remote = RemoteDatasource<CaseModel>(
          baseUrl: 'http://127.0.0.1:8000/api',
          fromJson: (json) => CaseModel.fromJson(json),
        );

  void _log(String message) {
    if (kDebugMode) {
      debugPrint('📁 [CASE-DS] $message');
    }
  }

  Future<Map<String, String>> _authHeader() async {
    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('access_token') ?? '';
    return {
      'Authorization': 'Bearer $token',
      'Accept': 'application/json',
    };
  }

  @override
  Future<List<CaseModel>> getAllCases() async {
    _log('GET /cases');
    return await _remote.getList(
      '/cases',
      headers: await _authHeader(),
    );
  }

  @override
  Future<CaseModel> getCaseById(int id) async {
    _log('GET /cases/$id');
    return await _remote.get(
      '/cases/$id',
      headers: await _authHeader(),
    );
  }
}
