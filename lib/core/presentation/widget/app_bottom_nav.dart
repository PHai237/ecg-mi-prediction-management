import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../routing/app_routes.dart';

class AppBottomNav extends StatefulWidget {
  final int inittialIndex;
  const AppBottomNav({super.key, required this.inittialIndex});
  @override
  State<AppBottomNav> createState() => _AppBottomNavState();
}

class _AppBottomNavState extends State<AppBottomNav> {
  late int currentIndex;
  final List<IconData> _icons = [
    Icons.home,
    Icons.people,
    Icons.details,
  ];
  final List<String> _labels = [
    'Home',
    'List',
    'Details',
  ];
  @override
  void initState() {
    super.initState();
    currentIndex = widget.inittialIndex;
  }
  @override
  Widget build(BuildContext context) {
    return BottomNavigationBar(
      currentIndex: currentIndex,
      onTap: (index){
        setState(() => currentIndex = index);
        _onItemTapped(context, index);
      },
      selectedItemColor:  Colors.blue,
      items: List.generate(
        _icons.length, (index) => BottomNavigationBarItem(
        icon: Icon(_icons[index]),
        label: _labels[index],
      )),
    );
  }
  void _onItemTapped(BuildContext context, int index) {
    switch (index) {
      case 0:
        context.go(AppRoutes.me);
        break;
      case 1:
        context.go(AppRoutes.patient);
        break;
      case 2:
        context.go(AppRoutes.caseList);
        break;
    }
  }
}