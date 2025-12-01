using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseProgect_Planeta35.Models
{
    public class InventoryCheck
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("item_id")]
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public InventoryItem Item { get; set; }

        [Column("check_date")]
        public DateTime CheckDate { get; set; } = DateTime.Now;

        [Required, MaxLength(50)]
        [Column("status")]
        public string Status { get; set; }

        [Column("notes")]
        [MaxLength(1000)]
        public string Notes { get; set; }
        [Required]
        [Column("checked_by_id")]
        public int CheckedById { get; set; }

        [ForeignKey("CheckedById")]
        public User CheckedBy { get; set; }
    }
}