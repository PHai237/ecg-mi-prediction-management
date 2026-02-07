import 'dart:nativewrappers/_internal/vm/lib/ffi_allocation_patch.dart';

import 'package:appsuckhoe/feature/cases/domain/entities/ecg_image.dart';
import 'package:appsuckhoe/feature/cases/domain/repositories/case_repository.dart';

class GetEcgImage {
  final CaseRepository repo;
  GetEcgImage(this.repo);

  Future<EcgImage> getEcgImageById(int id) => repo.call(id);
}