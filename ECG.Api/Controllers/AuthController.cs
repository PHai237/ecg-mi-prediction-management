using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECG.Api.Data;
using ECG.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ECG.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Vui lòng nhập username và password." });

            var username = dto.Username.Trim();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });

            var ok = PasswordHasher.Verify(dto.Password, user.PasswordHash);
            if (!ok)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });

            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("uid", user.Id.ToString())
            };

            var expMinutes = int.Parse(jwt["ExpMinutes"] ?? "240");

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expMinutes),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expiresAt = token.ValidTo,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Role,
                    user.StaffCode,
                    user.FullName,
                    user.Title,
                    user.Department
                }
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var uidStr = User.FindFirstValue("uid");
            if (!int.TryParse(uidStr, out var uid))
                return Unauthorized(new { message = "Token không hợp lệ." });

            // Query DB để lấy profile thật (không dựa vào claim)
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == uid);

            if (user == null)
                return Unauthorized(new { message = "User không tồn tại hoặc đã bị vô hiệu." });

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role,
                user.StaffCode,
                user.FullName,
                user.Title,
                user.Department
            });
        }
    }

    public record LoginDto(string Username, string Password);
}
