import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../app.dart';
import 'core/config/api_config.dart';
import 'feature/auth/data/datasource/auth_remote_datasource.dart';
import 'feature/auth/domain/repositories/auth_repository.dart';
import 'feature/auth/data/repository/auth_repository_impl.dart';
import 'feature/auth/domain/usecases/login.dart';
void main() {
  runApp(
    MultiProvider(
      providers: [
        Provider<AuthRemoteDataSource>(
          create: (_) => AuthRemoteDataSource(baseUrl: ApiConfig.baseUrl),
        ),
        Provider<AuthRepository>(
          create: (context) => AuthRepositoryImpl(
            remoteDataSource: context.read<AuthRemoteDataSource>(),
          ),
        ),
        Provider<Login>(
          create: (context) => Login(context.read<AuthRepository>()),
        ),
      ],
      child: const MyApp(),
    )
  );
}