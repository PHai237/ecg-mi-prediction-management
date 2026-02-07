import 'package:flutter/foundation.dart';
import 'package:appsuckhoe/core/data/remote_datasource.dart';
import 'package:appsuckhoe/feature/cases/data/model/ecg_image_model.dart';
import 'package:shared_preferences/shared_preferences.dart';

abstract class EcgImageRemoteDatasource {
  Future<List<EcgImageModel>> getAllEcgImages();
  Future<EcgImageModel> getEcgImageById(int id);
}

class EcgImageRemoteDatasourceImpl implements EcgImageRemoteDatasource {
  final RemoteDatasource<EcgImageModel> _remote;

  EcgImageRemoteDatasourceImpl()
      : _remote = RemoteDatasource<EcgImageModel>(
          baseUrl: 'http://127.0.0.1:8000/api',
          fromJson: (json) => EcgImageModel.fromJson(json),
        );

  void _log(String message) {
    if (kDebugMode) {
      debugPrint('🫀 [ECG-DS] $message');
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
  Future<List<EcgImageModel>> getAllEcgImages() async {
    _log('GET /ecg-images');
    return await _remote.getList(
      '/ecg-images',
      headers: await _authHeader(),
    );
  }

  @override
  Future<EcgImageModel> getEcgImageById(int id) async {
    _log('GET /ecg-images/$id');
    return await _remote.get(
      '/ecg-images/$id',
      headers: await _authHeader(),
    );
  }
}
