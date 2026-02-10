import 'dart:io';
import 'package:flutter/foundation.dart';

class ApiConfig {
  static String get baseUrl {
    // Flutter Web (chrome)
    if (kIsWeb) {
      return 'http://localhost:5089';
    }

    // Mobile (Android / iOS)
    if (Platform.isAndroid || Platform.isIOS) {
      return 'http://10.202.17.147:5089';
    }

    // fallback
    return 'http://localhost:5089';
  }
}
