using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProgect_Planeta35.Models
{
    public class CartItem
    {
        public ProcurementItem Item { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Item.Price * Quantity;
    }
}
