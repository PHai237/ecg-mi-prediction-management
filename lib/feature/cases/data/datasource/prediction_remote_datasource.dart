import 'package:flutter/foundation.dart';
import 'package:appsuckhoe/core/data/remote_datasource.dart';
import 'package:appsuckhoe/feature/cases/data/model/prediction_model.dart';
import 'package:shared_preferences/shared_preferences.dart';

abstract class PredictionRemoteDatasource {
  Future<List<PredictionModel>> getAllPredictions();
  Future<PredictionModel> getPredictionById(int id);
}

class PredictionRemoteDatasourceImpl implements PredictionRemoteDatasource {
  final RemoteDatasource<PredictionModel> _remote;

  PredictionRemoteDatasourceImpl()
      : _remote = RemoteDatasource<PredictionModel>(
          baseUrl: 'http://127.0.0.1:8000/api',
          fromJson: (json) => PredictionModel.fromJson(json),
        );

  void _log(String message) {
    if (kDebugMode) {
      debugPrint('🤖 [PREDICTION-DS] $message');
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
  Future<List<PredictionModel>> getAllPredictions() async {
    _log('GET /predictions');
    return await _remote.getList(
      '/predictions',
      headers: await _authHeader(),
    );
  }

  @override
  Future<PredictionModel> getPredictionById(int id) async {
    _log('GET /predictions/$id');
    return await _remote.get(
      '/predictions/$id',
      headers: await _authHeader(),
    );
  }
}
