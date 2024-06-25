using System.ComponentModel.DataAnnotations;
using System.Data;

namespace MoveTable_Server.Models.User
{
    public class User
    {
        [Key]
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }

        [Required]
        [MaxLength(500)]
        public string Account { get; set; }

        [Required]
        [MaxLength(500)]
        public string Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public bool Gender { get; set; } = true;

        [MaxLength(10)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Photo { get; set; }
    }
}
