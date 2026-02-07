import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';

import '../../domain/usecases/login.dart';
import '../../domain/entities/user.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();

  bool _loading = false;
  String? _error;

  Future<void> _handleLogin() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final loginUsecase = context.read<Login>();

      final User user = await loginUsecase(
        _usernameController.text.trim(),
        _passwordController.text.trim(),
      );

      debugPrint('✅ Login success - role: ${user.role}');

      // ❌ Không phải admin
      if (user.role != 'Admin') {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Chỉ tài khoản Admin mới được phép đăng nhập'),
            backgroundColor: Colors.red,
          ),
        );
        return;
      }

      if (!mounted) return;

      // ✅ Doctor login OK
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Đăng nhập thành công'),
          backgroundColor: Colors.green,
        ),
      );

      context.go('/me');
    } catch (e) {
      setState(() {
        _error = 'Sai username hoặc mật khẩu';
      });
    } finally {
      if (mounted) {
        setState(() {
          _loading = false;
        });
      }
    }
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Doctor Login')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // USERNAME
            TextField(
              controller: _usernameController,
              decoration: const InputDecoration(
                labelText: 'Username',
                border: OutlineInputBorder(),
              ),
            ),

            const SizedBox(height: 12),

            // PASSWORD
            TextField(
              controller: _passwordController,
              obscureText: true,
              decoration: const InputDecoration(
                labelText: 'Password',
                border: OutlineInputBorder(),
              ),
            ),

            const SizedBox(height: 16),

            // ERROR
            if (_error != null)
              Text(
                _error!,
                style: const TextStyle(color: Colors.red),
              ),

            const SizedBox(height: 16),

            // LOGIN BUTTON
            SizedBox(
              width: double.infinity,
              height: 48,
              child: ElevatedButton(
                onPressed: _loading ? null : _handleLogin,
                child: _loading
                    ? const CircularProgressIndicator(color: Colors.white)
                    : const Text('Login'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
