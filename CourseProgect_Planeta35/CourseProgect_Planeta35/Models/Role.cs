using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class Role
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("name")]
        public string Name { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
