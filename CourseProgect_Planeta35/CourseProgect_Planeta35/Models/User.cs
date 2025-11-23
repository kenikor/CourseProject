using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Column("username")]
        public string Username { get; set; }

        [Required, MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [MaxLength(150)]
        [Column("full_name")]
        public string FullName { get; set; }

        [Column("department_id")]
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
