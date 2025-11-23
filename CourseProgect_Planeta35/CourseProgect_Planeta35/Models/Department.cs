using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CourseProgect_Planeta35.Models
{
    public class Department
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [MaxLength(150)]
        [Column("location")]
        public string Location { get; set; }

        public ICollection<User> Users { get; set; }
        public ICollection<Asset> Assets { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
    }
}
