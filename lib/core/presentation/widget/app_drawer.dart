import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';

import '../../routing/app_routes.dart';
import '/feature/auth/domain/repositories/auth_repository.dart';

class AppDrawer extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: Column(
        children: [
          const DrawerHeader(
            child: Text(
              'App Drawer',
              style: TextStyle(fontSize: 24),
            ),
          ),

          ListTile(
            leading: const Icon(Icons.home),
            title: const Text('Home'),
            onTap: () => context.go(AppRoutes.me),
          ),

          ListTile(
            leading: const Icon(Icons.book),
            title: const Text('Patient List'),
            onTap: () => context.go(AppRoutes.patient),
          ),

          const Spacer(), // ⬅️ đẩy logout xuống dưới

          // ================= LOGOUT =================
          ListTile(
            leading: const Icon(Icons.logout, color: Colors.red),
            title: const Text(
              'Logout',
              style: TextStyle(color: Colors.red),
            ),
            onTap: () async {
              final auth = context.read<AuthRepository>();
              await auth.logout();

              // quay về login
              context.go(AppRoutes.login);
            },
          ),
        ],
      ),
    );
  }
}
