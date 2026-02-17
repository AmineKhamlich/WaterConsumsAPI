using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WConsumsAPI.Models
{
    [Table("APP_USUARI")]
    public class AppUsuari
    {
        [Key]
        public int Id_usuari { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom_usuari { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string password_hash { get; set; } = string.Empty;

        // Ara Actiu és un booleà (BIT a SQL)
        public bool? Actiu { get; set; }

        // BIT a SQL -> bool a C#
        public bool CanviPasswordObligatori { get; set; }

        // --- RELACIÓ AMB ROLS ---
        public int Id_rol { get; set; }

        [ForeignKey("Id_rol")]
        public virtual AppRol? Rol { get; set; }
    }
}
