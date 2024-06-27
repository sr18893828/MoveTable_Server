using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoveTable_Server.Controllers
{
    // 套用 CORS 策略
    [EnableCors("AllowAny")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserIdentityController : ControllerBase
    {
        private MoveTablesDbContext _context;

        public IConfiguration _configuration; //引用JWT

        public UserIdentityController(MoveTablesDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        #region Log in View Models
        public class LoginViewModels
        {
            public string Account { get; set; }

            public string Password { get; set; }
        }
        #endregion


        #region Log in
        [HttpPost("Log in")]
        public async Task<IActionResult> Login([FromBody] LoginViewModels data)
        {
            var user = _context.Users.Include(u => u.Role)
                .SingleOrDefault(a => a.Account == data.Account && a.Password == data.Password);

            if (user == null)
            {
                return BadRequest("帳號密碼錯誤");
            }

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //token識別標籤，唯一值
        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString()), //將顯示發行時間修正為Unix 時間

        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Role, user.Role.RoleName),
    };

            // Jwt 金鑰資訊
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1), // 登入過期時間
                signingCredentials: signIn);

            //先宣告再回傳結果
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                Message = $"{user.Name} 已登入成功!!",
                Token = tokenString,
            });
        }
        #endregion



    }
}
