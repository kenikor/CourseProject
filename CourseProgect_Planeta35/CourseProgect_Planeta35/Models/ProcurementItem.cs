using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class ProcurementItem
    {
        [Key]
        public string Id { get; set; } 

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Category { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public bool InStock { get; set; }

        public string Supplier { get; set; }

        public string Unit { get; set; }

        public string ImagePathToShow { get; set; }
    }
}
