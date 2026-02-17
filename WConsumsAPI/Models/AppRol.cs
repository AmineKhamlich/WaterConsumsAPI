using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WConsumsAPI.Models
{
    [Table("APP_ROL")]
    public class AppRol
    {
        [Key]
        public int Id_rol { get; set; }

        [Required]
        [StringLength(50)]
        public string Nom_Rol { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Descripcio { get; set; }
    }
}
