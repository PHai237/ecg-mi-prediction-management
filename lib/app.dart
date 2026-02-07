import 'package:flutter/material.dart';
import 'core/routing/app_go_router.dart';
import 'core/config/api_config.dart';
import 'feature/auth/data/datasource/auth_remote_datasource.dart';

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      routerConfig: AppGoRouter.router,
      title: 'Flutter Demo',
      theme: ThemeData(
        primarySwatch: Colors.blue,
      ),
    );
  }
}