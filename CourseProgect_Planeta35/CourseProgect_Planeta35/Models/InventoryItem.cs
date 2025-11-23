using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class InventoryItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("inventory_id")]
        public int InventoryId { get; set; }
        [ForeignKey("InventoryId")]
        public Inventory Inventory { get; set; }

        [Required]
        [Column("asset_id")]
        public int AssetId { get; set; }
        [ForeignKey("AssetId")]
        public Asset Asset { get; set; }

        [Required, MaxLength(50)]
        [Column("status")]
        public string Status { get; set; }

        [MaxLength(255)]
        [Column("note")]
        public string Note { get; set; }

        public int ResponsiblePersonId { get; set; }
    }
}
