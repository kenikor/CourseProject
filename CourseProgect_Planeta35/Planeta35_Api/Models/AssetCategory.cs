using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class AssetCategory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [MaxLength(255)]
        [Column("description")]
        public string Description { get; set; }

        [Column("color")]
        [MaxLength(10)]
        public string Color { get; set; }

        public ICollection<Asset> Assets { get; set; }
    }
}
