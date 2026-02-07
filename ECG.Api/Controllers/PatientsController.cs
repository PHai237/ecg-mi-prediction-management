using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ECG.Api.Data;
using ECG.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECG.Api.Contracts;

namespace ECG.Api.Controllers
{
    [ApiController]
    [Route("api/patients")]
    [Produces("application/json")]
    [Authorize] // Admin & Technician đều xem được (WPF cần chọn bệnh nhân)
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PatientsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/patients?includeInactive=true
        [HttpGet]
        [ProducesResponseType(typeof(List<Patient>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Patient>>> GetAll([FromQuery] bool includeInactive = false)
        {
            var q = _db.Patients.AsNoTracking().AsQueryable();

            if (!includeInactive)
                q = q.Where(p => p.IsActive);

            var data = await q
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return Ok(data);
        }

        // GET /api/patients/{id}?includeInactive=true
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Patient), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Patient>> GetById(int id, [FromQuery] bool includeInactive = false)
        {
            var q = _db.Patients.AsNoTracking().AsQueryable();

            if (!includeInactive)
                q = q.Where(p => p.IsActive);

            var patient = await q.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null) return NotFound();

            return Ok(patient);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Patient), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Patient>> Create([FromBody] PatientCreateDto dto)
        {
            var code = dto.Code.Trim();
            var name = dto.Name.Trim();

            if (name.Length <= 2)
                return BadRequest(new ApiError("Tên bệnh nhân phải lớn hơn 2 ký tự."));

            var exists = await _db.Patients.AnyAsync(p => p.Code == code);
            if (exists)
                return Conflict(new ApiError("Mã bệnh nhân đã tồn tại."));

            var patient = new Patient
            {
                Code = code,
                Name = name,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,                 // true/false/null
                IsExamined = dto.IsExamined,         // true/false
                Note = dto.Note,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] PatientUpdateDto dto)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null) return NotFound();

            if (!patient.IsActive)
                return Conflict(new ApiError("Bệnh nhân đã ngưng theo dõi, không thể cập nhật."));

            var name = dto.Name.Trim();
            if (name.Length <= 2)
                return BadRequest(new ApiError("Tên bệnh nhân phải lớn hơn 2 ký tự."));

            patient.Name = name;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.Gender = dto.Gender;           // true/false/null
            patient.IsExamined = dto.IsExamined;   // true/false
            patient.Note = dto.Note;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PATCH /api/patients/{id}/deactivate  (Admin only)
        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null) return NotFound();

            if (!patient.IsActive)
                return NoContent();

            patient.IsActive = false;
            patient.DeactivatedAt = DateTime.UtcNow;
            patient.DeactivatedByUserId = TryGetUid();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        private int? TryGetUid()
        {
            var uidStr = User.FindFirstValue("uid");
            if (int.TryParse(uidStr, out var uid)) return uid;
            return null;
        }
    }

    public class PatientCreateDto
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        public bool? Gender { get; set; }

        public bool IsExamined { get; set; } = false;

        public string? Note { get; set; }
    }

    public class PatientUpdateDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        public bool? Gender { get; set; }

        public bool IsExamined { get; set; } = false;

        public string? Note { get; set; }
    }
}
