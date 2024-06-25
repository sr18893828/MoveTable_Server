using System.ComponentModel.DataAnnotations;

namespace MoveTable_Server.Models.User
{
    public class Role
    {
        [Key]
        [Required]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
