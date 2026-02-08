using System.Security.Claims;
using ECG.Api.Data;
using ECG.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ECG.Api.Contracts;

namespace ECG.Api.Controllers
{
    [ApiController]
    [Route("api/cases")]
    [Produces("application/json")]
    [Authorize]
    public class CasesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public CasesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET /api/cases?patientId=1&status=uploaded&from=2026-02-01&to=2026-02-07
        // from/to chấp nhận:
        // - YYYY-MM-DD (date-only)
        // - ISO 8601: 2026-02-01T00:00:00Z hoặc 2026-02-01T00:00:00+07:00
        [HttpGet]
        [ProducesResponseType(typeof(List<CaseListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<CaseListItemDto>>> GetList(
            [FromQuery] int? patientId,
            [FromQuery] string? status,
            [FromQuery] string? from,
            [FromQuery] string? to,
            [FromQuery] bool includeArchived = false
        )
        {
            var q = _db.Cases
                .AsNoTracking()
                .Include(c => c.Patient)
                .Include(c => c.Images)
                .Include(c => c.CreatedByUser)
                .Include(c => c.PredictedByUser)
                .AsQueryable();

            if (includeArchived)
                q = q.IgnoreQueryFilters();

            if (patientId.HasValue)
                q = q.Where(x => x.PatientId == patientId.Value);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normStatus = status.Trim().ToLowerInvariant();
                // an toàn hơn so với so sánh đúng-case
                q = q.Where(x => x.Status.ToLower() == normStatus);
            }

            // Parse from/to
            if (!string.IsNullOrWhiteSpace(from))
            {
                if (!TryParseToUtc(from!, out var fromUtc, out var err))
                    return BadRequest(new ApiError($"from không hợp lệ. {err}"));

                q = q.Where(x => x.MeasuredAt >= fromUtc);
            }

            if (!string.IsNullOrWhiteSpace(to))
            {
                // nếu to là date-only => lọc < next day (bao trọn ngày)
                if (!TryParseToUtc(to!, out var toUtcOrExclusive, out var errTo, out bool isDateOnly))
                    return BadRequest(new ApiError($"to không hợp lệ. {errTo}"));

                if (isDateOnly)
                {
                    // toExclusive = (toDate + 1 day) 00:00Z
                    q = q.Where(x => x.MeasuredAt < toUtcOrExclusive);
                }
                else
                {
                    // to có time => inclusive
                    q = q.Where(x => x.MeasuredAt <= toUtcOrExclusive);
                }
            }

            var data = await q
                .OrderByDescending(x => x.Id)
                .Select(x => new CaseListItemDto(
                    x.Id,
                    x.PatientId,
                    x.Patient != null ? x.Patient.Code : null,
                    x.Patient != null ? x.Patient.Name : null,
                    x.MeasuredAt,
                    x.Status,
                    x.Images.Count,
                    x.CreatedAt,

                    x.CreatedByUserId,
                    x.CreatedByUser != null ? x.CreatedByUser.Username : null,
                    x.CreatedByUser != null ? x.CreatedByUser.FullName : null,
                    x.CreatedByUser != null ? x.CreatedByUser.Title : null,
                    x.CreatedByUser != null ? x.CreatedByUser.Department : null,

                    x.PredictedLabel,
                    x.PredictedConfidence,
                    x.PredictedAt,
                    x.PredictedByUserId,
                    x.PredictedByUser != null ? x.PredictedByUser.Username : null,
                    x.PredictedByUser != null ? x.PredictedByUser.FullName : null,
                    x.PredictedByUser != null ? x.PredictedByUser.Title : null,
                    x.PredictedByUser != null ? x.PredictedByUser.Department : null
                ))
                .ToListAsync();

            return Ok(data);
        }

        // GET /api/cases/{id}?includeArchived=true
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CaseDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CaseDetailDto>> GetById(int id, [FromQuery] bool includeArchived = false)
        {
            var q = _db.Cases
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Images)
                .Include(x => x.CreatedByUser)
                .Include(x => x.PredictedByUser)
                .Include(x => x.Predictions)
                    .ThenInclude(p => p.PredictedByUser)
                .AsQueryable();

            if (includeArchived)
                q = q.IgnoreQueryFilters();

            var c = await q.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();

            var dto = new CaseDetailDto(
                c.Id,
                c.PatientId,
                c.Patient?.Code,
                c.Patient?.Name,
                c.MeasuredAt,
                c.Status,
                c.Note,
                c.CreatedAt,

                c.CreatedByUserId,
                c.CreatedByUser?.Username,
                c.CreatedByUser?.FullName,
                c.CreatedByUser?.Title,
                c.CreatedByUser?.Department,

                c.PredictedLabel,
                c.PredictedConfidence,
                c.PredictedAt,
                c.PredictedByUserId,
                c.PredictedByUser?.Username,
                c.PredictedByUser?.FullName,
                c.PredictedByUser?.Title,
                c.PredictedByUser?.Department,

                c.Images
                    .OrderByDescending(i => i.Id)
                    .Select(i => new CaseImageDto(
                        i.Id,
                        i.OriginalFileName,
                        i.ContentType,
                        i.SizeBytes,
                        ToAbsoluteUrl(i.UrlPath),
                        i.UploadedAt
                    ))
                    .ToList(),

                c.Predictions
                    .OrderByDescending(p => p.Id)
                    .Select(p => new CasePredictionDto(
                        p.Id,
                        p.Label,
                        p.Confidence,
                        p.Algorithm,
                        p.PredictedAt,
                        p.PredictedByUserId,
                        p.PredictedByUser != null ? p.PredictedByUser.Username : null,
                        p.PredictedByUser != null ? p.PredictedByUser.FullName : null,
                        p.PredictedByUser != null ? p.PredictedByUser.Title : null,
                        p.PredictedByUser != null ? p.PredictedByUser.Department : null,
                        p.Note
                    ))
                    .ToList()
            );

            return Ok(dto);
        }

        // POST /api/cases
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CaseDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CaseDetailDto>> Create([FromBody] CaseCreateDto dto)
        {
            var uid = TryGetUid();
            if (uid == null) return Unauthorized(new ApiError("Thiếu thông tin người dùng trong token."));

            var patientExists = await _db.Patients.AnyAsync(p => p.Id == dto.PatientId && p.IsActive);
            if (!patientExists) return BadRequest(new ApiError("PatientId không tồn tại hoặc đã ngưng theo dõi."));

            var measuredUtc =
                dto.MeasuredAt.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(dto.MeasuredAt, DateTimeKind.Utc)
                    : dto.MeasuredAt.ToUniversalTime();

            var c = new EcgCase
            {
                PatientId = dto.PatientId,
                MeasuredAt = measuredUtc,
                Note = dto.Note,
                Status = "new",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = uid.Value,
                IsDeleted = false
            };

            _db.Cases.Add(c);
            await _db.SaveChangesAsync();

            var created = new CaseDetailDto(
                c.Id,
                c.PatientId,
                null,
                null,
                c.MeasuredAt,
                c.Status,
                c.Note,
                c.CreatedAt,

                c.CreatedByUserId,
                null,
                null,
                null,
                null,

                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,

                new List<CaseImageDto>(),
                new List<CasePredictionDto>()
            );

            return CreatedAtAction(nameof(GetById), new { id = c.Id }, created);
        }

        // POST /api/cases/{id}/images
        [HttpPost("{id:int}/images")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(List<CaseImageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<List<CaseImageDto>>> UploadImages(int id, [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new ApiError("Vui lòng chọn ít nhất 1 file."));

            const int maxFiles = 10;
            if (files.Count > maxFiles)
                return BadRequest(new ApiError($"Tối đa {maxFiles} file mỗi lần upload."));

            var c = await _db.Cases
                .IgnoreQueryFilters()
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return NotFound();
            if (c.IsDeleted) return Conflict(new ApiError("Case đã được lưu trữ (archive), không thể upload ảnh."));

            var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads");
            var caseFolder = Path.Combine(uploadsRoot, "cases", id.ToString());
            Directory.CreateDirectory(caseFolder);

            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
            var added = new List<EcgCaseImage>();

            foreach (var file in files)
            {
                if (file.Length <= 0) continue;

                var originalName = Path.GetFileName(file.FileName);
                var ext = Path.GetExtension(originalName);

                if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext))
                    return BadRequest(new ApiError($"File không hợp lệ: {originalName}. Chỉ nhận JPG/JPEG/PNG/WEBP."));

                if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiError($"File không hợp lệ: {originalName}. Content-Type phải là image/*"));

                const long maxBytes = 15L * 1024 * 1024;
                if (file.Length > maxBytes)
                    return BadRequest(new ApiError($"File quá lớn: {originalName}. Tối đa 15MB/file."));

                var signatureOk = await LooksLikeSupportedImageAsync(file, ext);
                if (!signatureOk)
                    return BadRequest(new ApiError($"File không hợp lệ: {originalName}. Nội dung không giống định dạng ảnh {ext}."));

                var savedName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
                var physicalPath = Path.Combine(caseFolder, savedName);

                await using (var stream = System.IO.File.Create(physicalPath))
                {
                    await file.CopyToAsync(stream);
                }

                var urlPath = $"/uploads/cases/{id}/{savedName}";

                added.Add(new EcgCaseImage
                {
                    CaseId = id,
                    FileName = savedName,
                    OriginalFileName = originalName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    SizeBytes = file.Length,
                    UrlPath = urlPath,
                    UploadedAt = DateTime.UtcNow
                });
            }

            if (added.Count == 0)
                return BadRequest(new ApiError("Không có file hợp lệ để upload."));

            _db.CaseImages.AddRange(added);

            // upload mới => reset prediction summary để tránh stale
            c.Status = "uploaded";
            c.PredictedLabel = null;
            c.PredictedConfidence = null;
            c.PredictedAt = null;
            c.PredictedByUserId = null;

            await _db.SaveChangesAsync();

            var result = added
                .OrderByDescending(x => x.Id)
                .Select(i => new CaseImageDto(
                    i.Id,
                    i.OriginalFileName,
                    i.ContentType,
                    i.SizeBytes,
                    ToAbsoluteUrl(i.UrlPath),
                    i.UploadedAt
                ))
                .ToList();

            return Ok(result);
        }

        // POST /api/cases/{id}/predict (mock)
        [HttpPost("{id:int}/predict")]
        [ProducesResponseType(typeof(PredictResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PredictResultDto>> RunPrediction(int id)
        {
            var uid = TryGetUid();
            if (uid == null) return Unauthorized(new ApiError("Thiếu thông tin người dùng trong token."));

            var c = await _db.Cases
                .IgnoreQueryFilters()
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return NotFound();
            if (c.IsDeleted) return Conflict(new ApiError("Case đã được lưu trữ (archive), không thể chạy dự đoán."));
            if (c.Images.Count == 0) return Conflict(new ApiError("Case chưa có ảnh ECG. Vui lòng upload ảnh trước khi dự đoán."));

            var anyMiName = c.Images.Any(i =>
                (!string.IsNullOrWhiteSpace(i.OriginalFileName) &&
                 i.OriginalFileName.Contains("MI", StringComparison.OrdinalIgnoreCase))
            );

            var rng = Random.Shared;
            string label;
            double confidence;

            if (anyMiName)
            {
                label = "MI";
                confidence = Round2(0.85 + rng.NextDouble() * 0.10);
            }
            else
            {
                var roll = rng.NextDouble();
                if (roll < 0.15)
                {
                    label = "MI";
                    confidence = Round2(0.60 + rng.NextDouble() * 0.25);
                }
                else if (roll < 0.90)
                {
                    label = "non-MI";
                    confidence = Round2(0.65 + rng.NextDouble() * 0.30);
                }
                else
                {
                    label = "uncertain";
                    confidence = Round2(0.45 + rng.NextDouble() * 0.15);
                }
            }

            var now = DateTime.UtcNow;

            var pred = new EcgCasePrediction
            {
                CaseId = c.Id,
                Label = label,
                Confidence = confidence,
                Algorithm = "mock-v1",
                PredictedAt = now,
                PredictedByUserId = uid.Value,
                Note = anyMiName ? "Heuristic: filename contains 'MI'." : "Weighted random mock."
            };

            _db.CasePredictions.Add(pred);

            c.PredictedLabel = label;
            c.PredictedConfidence = confidence;
            c.PredictedAt = now;
            c.PredictedByUserId = uid.Value;
            c.Status = "predicted";

            await _db.SaveChangesAsync();

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == uid.Value);

            return Ok(new PredictResultDto(
                c.Id,
                c.PredictedLabel,
                c.PredictedConfidence,
                c.PredictedAt,
                c.PredictedByUserId,
                user?.Username,
                user?.FullName,
                user?.Title,
                user?.Department,
                pred.Algorithm,
                pred.Note
            ));
        }

        // PATCH /api/cases/{id}/archive (Admin)
        [HttpPatch("{id:int}/archive")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Archive(int id)
        {
            var c = await _db.Cases.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();

            if (c.IsDeleted) return NoContent();

            c.IsDeleted = true;
            c.DeletedAt = DateTime.UtcNow;
            c.DeletedByUserId = TryGetUid();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ===== Helpers =====

        // Parse date-only: YYYY-MM-DD
        // Parse ISO8601 DateTime/DateTimeOffset
        private static bool TryParseToUtc(string input, out DateTime utc, out string error, out bool isDateOnly)
        {
            error = "";
            utc = default;
            isDateOnly = false;

            input = input.Trim();

            // date-only
            if (DateOnly.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                isDateOnly = true;
                // return next day 00:00Z for "to" exclusive handling
                utc = d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
                return true;
            }

            // datetime / datetimeoffset
            if (DateTimeOffset.TryParse(
                    input,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dto
                ))
            {
                utc = dto.UtcDateTime;
                isDateOnly = false;
                return true;
            }

            error = "Định dạng hợp lệ: 'YYYY-MM-DD' hoặc ISO 8601 (vd: 2026-02-01T00:00:00Z, 2026-02-01T00:00:00+07:00).";
            return false;
        }

        // overload dùng cho from (date-only => midnight start)
        private static bool TryParseToUtc(string input, out DateTime utc, out string error)
        {
            error = "";
            utc = default;

            input = input.Trim();

            if (DateOnly.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                utc = d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                return true;
            }

            if (DateTimeOffset.TryParse(
                    input,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dto
                ))
            {
                utc = dto.UtcDateTime;
                return true;
            }

            error = "Định dạng hợp lệ: 'YYYY-MM-DD' hoặc ISO 8601 (vd: 2026-02-01T00:00:00Z, 2026-02-01T00:00:00+07:00).";
            return false;
        }

        private int? TryGetUid()
        {
            var uidStr = User.FindFirstValue("uid");
            if (int.TryParse(uidStr, out var uid)) return uid;
            return null;
        }

        private static double Round2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private static async Task<bool> LooksLikeSupportedImageAsync(IFormFile file, string ext)
        {
            byte[] header = new byte[12];
            int read;
            await using (var s = file.OpenReadStream())
            {
                read = await s.ReadAsync(header, 0, header.Length);
            }

            if (read < 12) return false;

            bool isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

            bool isPng =
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;

            bool isWebp =
                header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F' &&
                header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P';

            ext = ext.ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => isJpeg,
                ".png" => isPng,
                ".webp" => isWebp,
                _ => false
            };
        }

        private string ToAbsoluteUrl(string urlPath)
        {
            return $"{Request.Scheme}://{Request.Host}{urlPath}";
        }

        public record CaseListItemDto(
            int Id,
            int PatientId,
            string? PatientCode,
            string? PatientName,
            DateTime MeasuredAt,
            string Status,
            int ImageCount,
            DateTime CreatedAt,

            int? CreatedByUserId,
            string? CreatedByUsername,
            string? CreatedByFullName,
            string? CreatedByTitle,
            string? CreatedByDepartment,

            string? PredictedLabel,
            double? PredictedConfidence,
            DateTime? PredictedAt,
            int? PredictedByUserId,
            string? PredictedByUsername,
            string? PredictedByFullName,
            string? PredictedByTitle,
            string? PredictedByDepartment
        );

        public record CaseImageDto(
            int Id,
            string OriginalFileName,
            string ContentType,
            long SizeBytes,
            string Url,
            DateTime UploadedAt
        );

        public record CasePredictionDto(
            int Id,
            string Label,
            double Confidence,
            string Algorithm,
            DateTime PredictedAt,
            int PredictedByUserId,
            string? PredictedByUsername,
            string? PredictedByFullName,
            string? PredictedByTitle,
            string? PredictedByDepartment,
            string? Note
        );

        public record CaseDetailDto(
            int Id,
            int PatientId,
            string? PatientCode,
            string? PatientName,
            DateTime MeasuredAt,
            string Status,
            string? Note,
            DateTime CreatedAt,

            int? CreatedByUserId,
            string? CreatedByUsername,
            string? CreatedByFullName,
            string? CreatedByTitle,
            string? CreatedByDepartment,

            string? PredictedLabel,
            double? PredictedConfidence,
            DateTime? PredictedAt,
            int? PredictedByUserId,
            string? PredictedByUsername,
            string? PredictedByFullName,
            string? PredictedByTitle,
            string? PredictedByDepartment,

            List<CaseImageDto> Images,
            List<CasePredictionDto> Predictions
        );

        public record PredictResultDto(
            int CaseId,
            string? Label,
            double? Confidence,
            DateTime? PredictedAt,
            int? PredictedByUserId,
            string? PredictedByUsername,
            string? PredictedByFullName,
            string? PredictedByTitle,
            string? PredictedByDepartment,
            string Algorithm,
            string? Note
        );

        public class CaseCreateDto
        {
            public int PatientId { get; set; }
            public DateTime MeasuredAt { get; set; }
            public string? Note { get; set; }
        }
    }
}
