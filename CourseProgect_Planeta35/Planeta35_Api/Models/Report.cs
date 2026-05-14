using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class Report
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required, MaxLength(100)]
        [Column("report_type")]
        public string ReportType { get; set; }

        [Required, MaxLength(255)]
        [Column("file_path")]
        public string FilePath { get; set; }

        [Column("generated_at")]
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
