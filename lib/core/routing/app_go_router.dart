import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'app_routes.dart';

// UI chung
import '../presentation/widget/app_bottom_nav.dart';
import '../presentation/widget/app_drawer.dart';

// Pages
import 'package:appsuckhoe/feature/auth/presentation/page/login_page.dart';
import 'package:appsuckhoe/feature/auth/presentation/page/home_page.dart';
import 'package:appsuckhoe/feature/patient/presentation/page/patient_list_page.dart';

class AppGoRouter {
  static final GoRouter router = GoRouter(
    debugLogDiagnostics: true,
    initialLocation: AppRoutes.login,

    // ================= ROUTES =================
    routes: [
      // ========= LOGIN (KHÔNG CÓ SHELL) =========
      GoRoute(
        path: AppRoutes.login,
        builder: (context, state) => const LoginPage(),
      ),

      // ========= APP SAU LOGIN =========
      ShellRoute(
        builder: (context, state, child) {
          final index = _getIndex(state.matchedLocation);

          return Scaffold(
            appBar: AppBar(
              title: const Text('Health App'),
            ),
            drawer: AppDrawer(),
            body: child,
            bottomNavigationBar: AppBottomNav(
              inittialIndex: index,
            ),
          );
        },
        routes: [
          GoRoute(
            path: AppRoutes.me,
            builder: (context, state) => const HomePage(),
          ),
          GoRoute(
            path: AppRoutes.patient,
            builder: (context, state) => const PatientListPage(),
          )
        ],
      ),
    ],

    // ================= REDIRECT =================
    redirect: (context, state) async {
      debugPrint('🧭 [ROUTER] Redirect called');
      debugPrint('📍 [ROUTER] Location: ${state.matchedLocation}');

      final prefs = await SharedPreferences.getInstance();
      final token = prefs.getString('access_token');
      final role = prefs.getString('role');

      debugPrint('🔑 [ROUTER] Token: $token');
      debugPrint('👤 [ROUTER] Role: $role');

      final loggingIn = state.matchedLocation == AppRoutes.login;

      // ❌ Chưa login
      if (token == null) {
        return loggingIn ? null : AppRoutes.login;
      }

      // ❌ Login rồi nhưng KHÔNG phải admin
      if (role != 'Admin') {
        await prefs.clear();
        return AppRoutes.login;
      }

      // ✅ Login xong mà đang ở login
      if (loggingIn) {
        return AppRoutes.me;
      }

      return null;
    },
  );

  // ================= HELPER =================
  static int _getIndex(String location) {
    switch (location) {
      case AppRoutes.me:
        return 0;
      case AppRoutes.patient:
        return 1;
      default:
        return 0;
    }
  }
}
