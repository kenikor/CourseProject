using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class Inventory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("conducted_by")]
        public int ConductedBy { get; set; }
        [ForeignKey("ConductedBy")]
        public User User { get; set; }

        [Required]
        [Column("department_id")]
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [Column("date_conducted")]
        public DateTime DateConducted { get; set; } = DateTime.Now;

        [MaxLength(255)]
        [Column("comment")]
        public string Comment { get; set; }

        public ICollection<InventoryItem> Items { get; set; }
    }
}
