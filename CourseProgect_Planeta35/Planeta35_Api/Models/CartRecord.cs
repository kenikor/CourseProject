using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class CartRecord
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } // Привязка к пользователю

        public string ProcurementItemId { get; set; }

        [ForeignKey("ProcurementItemId")]
        public ProcurementItem Item { get; set; }

        public int Quantity { get; set; }
    }
}
