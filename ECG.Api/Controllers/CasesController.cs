using ECG.Api.Data;
using ECG.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECG.Api.Controllers
{
    [ApiController]
    [Route("api/cases")]
    [Produces("application/json")]
    [Authorize] // Admin & Technician đều dùng được (WPF cần)
    public class CasesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public CasesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET /api/cases?patientId=1&status=uploaded&from=2026-02-01&to=2026-02-05
        [HttpGet]
        [ProducesResponseType(typeof(List<CaseListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CaseListItemDto>>> GetList(
            [FromQuery] int? patientId,
            [FromQuery] string? status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to
        )
        {
            var q = _db.Cases
                .AsNoTracking()
                .Include(c => c.Patient)
                .Include(c => c.Images)
                .AsQueryable();

            if (patientId.HasValue)
                q = q.Where(x => x.PatientId == patientId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(x => x.Status == status.Trim().ToLowerInvariant());

            if (from.HasValue)
                q = q.Where(x => x.MeasuredAt >= from.Value);

            if (to.HasValue)
                q = q.Where(x => x.MeasuredAt <= to.Value);

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
                    x.CreatedAt
                ))
                .ToListAsync();

            return Ok(data);
        }

        // GET /api/cases/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CaseDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CaseDetailDto>> GetById(int id)
        {
            var c = await _db.Cases
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id);

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
                    .ToList()
            );

            return Ok(dto);
        }

        // POST /api/cases
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(CaseDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CaseDetailDto>> Create([FromBody] CaseCreateDto dto)
        {
            var patientExists = await _db.Patients.AnyAsync(p => p.Id == dto.PatientId);
            if (!patientExists) return BadRequest(new ApiError("PatientId không tồn tại."));

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
                CreatedAt = DateTime.UtcNow
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
                new List<CaseImageDto>()
            );

            return CreatedAtAction(nameof(GetById), new { id = c.Id }, created);
        }

        // POST /api/cases/{id}/images (multipart/form-data)
        [HttpPost("{id:int}/images")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(List<CaseImageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<CaseImageDto>>> UploadImages(int id, [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new ApiError("Vui lòng chọn ít nhất 1 file."));

            var c = await _db.Cases.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();

            var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads");
            var caseFolder = Path.Combine(uploadsRoot, "cases", id.ToString());
            Directory.CreateDirectory(caseFolder);

            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

            var added = new List<EcgCaseImage>();

            foreach (var file in files)
            {
                if (file.Length <= 0) continue;

                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(ext) || !allowedExt.Contains(ext))
                    return BadRequest(new ApiError($"File không hợp lệ: {file.FileName}. Chỉ nhận JPG/JPEG/PNG/WEBP."));

                const long maxBytes = 15L * 1024 * 1024;
                if (file.Length > maxBytes)
                    return BadRequest(new ApiError($"File quá lớn: {file.FileName}. Tối đa 15MB/file."));

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
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    SizeBytes = file.Length,
                    UrlPath = urlPath,
                    UploadedAt = DateTime.UtcNow
                });
            }

            if (added.Count == 0)
                return BadRequest(new ApiError("Không có file hợp lệ để upload."));

            _db.CaseImages.AddRange(added);

            if (c.Status == "new") c.Status = "uploaded";

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

        // DELETE /api/cases/{id} (Admin only)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Cases.FindAsync(id);
            if (c == null) return NotFound();

            _db.Cases.Remove(c);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private string ToAbsoluteUrl(string urlPath)
        {
            return $"{Request.Scheme}://{Request.Host}{urlPath}";
        }

        public record ApiError(string Message);

        public record CaseListItemDto(
            int Id,
            int PatientId,
            string? PatientCode,
            string? PatientName,
            DateTime MeasuredAt,
            string Status,
            int ImageCount,
            DateTime CreatedAt
        );

        public record CaseImageDto(
            int Id,
            string OriginalFileName,
            string ContentType,
            long SizeBytes,
            string Url,
            DateTime UploadedAt
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
            List<CaseImageDto> Images
        );

        public class CaseCreateDto
        {
            public int PatientId { get; set; }
            public DateTime MeasuredAt { get; set; }
            public string? Note { get; set; }
        }
    }
}
