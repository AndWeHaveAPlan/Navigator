using System.ComponentModel.DataAnnotations;

namespace Navigator.Crow.DataTypes
{
    public class GiveRequest
    {
        [Required]
        public string ItemName { get; set; }

        [Range(1, 99)]
        [Required]
        public int ItemCount { get; set; }
    }
}
