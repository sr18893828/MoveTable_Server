﻿using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoveTable_Server.Models.User;
using System.ComponentModel.DataAnnotations;
using System.Text;



namespace MoveTable_Server.Controllers
{
    // 套用 CORS 策略
    [EnableCors("AllowAny")]
    [Route("api/[controller]")]
    [ApiController]
    public class MemberManagementController : ControllerBase
    {
        private MoveTablesDbContext _context; 
        private readonly IWebHostEnvironment _webHost;  //上傳圖片使用

        public MemberManagementController(MoveTablesDbContext context, IWebHostEnvironment webHost)
        {
            _context = context;
            _webHost = webHost;
        }

        #region User Data
        [HttpGet("Get UserData")]
        public IEnumerable<object> GetUserData()
        {
            var result = _context.Users.Join(_context.Roles, u => u.RoleId, r => r.RoleId, (u, r) => new
            {
                Userid = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Gender = u.Gender ? "男" : "女",
                Phone = u.Phone,
                Photo = u.Photo,
                Rolename = r.RoleName,
            }) ;
            return result;
        }
        #endregion


        #region Role Data
        [HttpGet("Get RoleData")]
        public IEnumerable<object> GetRoleData()
        {
            var result = _context.Roles.Select(r => new
            {
                r.RoleId,
                r.RoleName,
            });
            return result;
        }
        #endregion


        #region Create Role View Models
        public class CreateRoleViewModels
        {
            [Required]
            [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "只能夠輸入英文")]
            public string RoleName { get; set; }
        }
        #endregion


        #region Create Role
        [HttpPost("Create Roles")]
        public async Task<IActionResult> CreateRole([FromForm] CreateRoleViewModels data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newRole = new Role { RoleName = data.RoleName };
            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();

            return Ok(new {success = true, message = "權限新增成功"});
        }
        #endregion


        #region Create Member View Models
        public class CreateMemberViewModels
        {
            [Required]
            [MaxLength(500)]
            public string Account { get; set; }

            [Required]
            [MaxLength(500)]
            public string Password { get; set; }

            [Required]
            [MaxLength(500)]
            public string ConfirmPassword { get; set; }

            [Required]
            [MaxLength(50)]
            public string Name { get; set; }

            [MaxLength(100)]
            public string? Email { get; set; }

            public bool? Gender { get; set; } = true;

            [MaxLength(10)]
            [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機號碼必須以09開頭並且是10位數字")]
            public string? Phone { get; set; }

            [MaxLength(200)]
            public IFormFile? Photo { get; set; }
        }
        #endregion


        #region Create Member
        [HttpPost("Create Member")]
        public async Task<IActionResult> CreateMember([FromForm]CreateMemberViewModels data) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string photoPath = "default.jpg"; //預設圖片
                var photoFile = data.Photo;
                if (photoFile != null && photoFile.Length > 0)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
                    string uploadfolder = Path.Combine(_webHost.WebRootPath, "images/headshots");
                    string filepath = Path.Combine(uploadfolder, filename);

                    using (var fileStream = new FileStream(filepath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(fileStream);
                    }
                    photoPath = filepath;
                }

                _context.Users.Add(new User
                {
                    Account = data.Account,
                    Password = HashPassword(data.Password),
                    Name = data.Name,
                    Email = data.Email,
                    Gender = data.Gender ?? true,
                    Phone = data.Phone,
                    Photo = photoPath,
                    RoleId = 10001

                });

                await _context.SaveChangesAsync();

                return Ok(new {success = true, message = "會員新增成功"});
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, innerException = innerMessage });
            }
        }
        #endregion


        #region Check Account 
        [HttpGet("Check Account Exists")]
        public async Task<IActionResult> CheckAccountExists([FromQuery] string account)
        {
            bool exists = await _context.Users.AnyAsync(u => u.Account == account);
            return Ok(exists); // 返回 false 表示 Account  已存在，true 表示 Account  不存在
        }
        #endregion


        #region Check Roles
        [HttpGet("Check Role Exists")]
        public async Task<IActionResult> CheckRoleExists([FromQuery]string role)
        {
            bool exists = await _context.Roles.AnyAsync(u => u.RoleName == role);
                return Ok(exists);  // 返回 false 表示 Role  已存在，true 表示 Role 不存在
        }
        #endregion


        #region Create User 結合確認帳戶是否存在
        [HttpPost("Create User")]
        public async Task<IActionResult> CreateUser([FromForm] CreateMemberViewModels data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if account already exists
            bool accountExists = await _context.Users.AnyAsync(u => u.Account == data.Account);
            if (accountExists)
            {
                return BadRequest(new { success = false, message = "此帳號已經被使用，請更換帳號" });
            }

            try
            {
                string photoPath = "default.jpg"; // Default photo
                var photoFile = data.Photo;
                if (photoFile != null && photoFile.Length > 0)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
                    string uploadfolder = Path.Combine(_webHost.WebRootPath, "images/headshots");
                    string filepath = Path.Combine(uploadfolder, filename);

                    using (var fileStream = new FileStream(filepath, FileMode.Create))
                    {
                        await photoFile.CopyToAsync(fileStream);
                    }
                    photoPath = filepath;
                }

                _context.Users.Add(new User
                {
                    Account = data.Account,
                    Password = HashPassword(data.Password),
                    Name = data.Name,
                    Email = data.Email,
                    Gender = data.Gender ?? true,
                    Phone = data.Phone,
                    Photo = photoPath,
                    RoleId = 10001

                });

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "會員新增成功" });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message, innerException = innerMessage });
            }
        }
        #endregion


        #region Create Role 結合確認Roles是否存在
        [HttpPost("Create Roles Check")]
        public async Task<IActionResult> CreateRoles([FromForm] CreateRoleViewModels data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if role already exists
            bool roleExists = await _context.Roles.AnyAsync(u => u.RoleName == data.RoleName);
            if (roleExists)
            {
                return BadRequest(new { success = false, message = "此權限已經被使用，請更換權限" });
            }

            var newRole = new Role { RoleName = data.RoleName };
            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "權限新增成功" });
        }
        #endregion


        #region Password Encryption
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        #endregion


    }
}
