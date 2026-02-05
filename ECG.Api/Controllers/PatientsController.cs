using System.ComponentModel.DataAnnotations;
using ECG.Api.Data;
using ECG.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECG.Api.Controllers
{
    [ApiController]
    [Route("api/patients")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PatientsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Get all patients</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<Patient>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<Patient>>> GetAll()
        {
            var data = await _db.Patients
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return Ok(data);
        }

        /// <summary>Get patient by id</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Patient), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Patient>> GetById(int id)
        {
            var patient = await _db.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            return Ok(patient);
        }

        /// <summary>Create a new patient</summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Patient), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<Patient>> Create([FromBody] PatientCreateDto dto)
        {
            var code = dto.Code.Trim();
            var name = dto.Name.Trim();
            var gender = dto.Gender.Trim().ToLowerInvariant();

            if (name.Length <= 2)
                return BadRequest(new ApiError("Tên bệnh nhân phải lớn hơn 2 ký tự."));

            if (gender is not ("nam" or "nu" or "khac"))
                return BadRequest(new ApiError("Giới tính phải là: nam, nu, khac."));

            var exists = await _db.Patients.AnyAsync(p => p.Code == code);
            if (exists)
                return Conflict(new ApiError("Mã bệnh nhân đã tồn tại."));

            var patient = new Patient
            {
                Code = code,
                Name = name,
                DateOfBirth = dto.DateOfBirth,
                Gender = gender,
                Note = dto.Note
            };

            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
        }

        /// <summary>Update an existing patient</summary>
        [HttpPut("{id:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] PatientUpdateDto dto)
        {
            var patient = await _db.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            var name = dto.Name.Trim();
            var gender = dto.Gender.Trim().ToLowerInvariant();

            if (name.Length <= 2)
                return BadRequest(new ApiError("Tên bệnh nhân phải lớn hơn 2 ký tự."));

            if (gender is not ("nam" or "nu" or "khac"))
                return BadRequest(new ApiError("Giới tính phải là: nam, nu, khac."));

            patient.Name = name;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.Gender = gender;
            patient.Note = dto.Note;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Delete a patient</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _db.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }

    public record ApiError(string Message);

    public class PatientCreateDto
    {
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required, MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        public string? Note { get; set; }
    }

    public class PatientUpdateDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required, MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}
