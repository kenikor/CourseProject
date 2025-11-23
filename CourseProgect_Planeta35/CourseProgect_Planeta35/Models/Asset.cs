using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class Asset
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("inventory_number")]
        public string InventoryNumber { get; set; }

        [Required, MaxLength(150)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public AssetCategory Category { get; set; }

        [Required]
        [Column("department_id")]
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [Column("responsible_id")]
        public int? ResponsibleId { get; set; }
        [ForeignKey("ResponsibleId")]
        public User Responsible { get; set; }

        [Column("purchase_date")]
        public DateTime? PurchaseDate { get; set; }

        [Column("cost")]
        public decimal? Cost { get; set; }

        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "В эксплуатации";

        public ICollection<InventoryItem> InventoryItems { get; set; }
        public ICollection<ChangeLog> ChangeLogs { get; set; }
    }
}
