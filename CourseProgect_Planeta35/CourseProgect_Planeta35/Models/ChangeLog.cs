using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class ChangeLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Column("asset_id")]
        public int? AssetId { get; set; }
        [ForeignKey("AssetId")]
        public Asset Asset { get; set; }

        [Required, MaxLength(50)]
        [Column("operation")]
        public string Operation { get; set; }

        [Column("change_time")]
        public DateTime ChangeTime { get; set; } = DateTime.Now;

        [Column("old_value")]
        public string OldValue { get; set; }

        [Column("new_value")]
        public string NewValue { get; set; }

        [MaxLength(255)]
        [Column("comment")]
        public string Comment { get; set; }
    }
}
