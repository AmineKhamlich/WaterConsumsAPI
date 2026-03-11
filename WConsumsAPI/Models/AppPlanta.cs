using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WConsumsAPI.Models
{
    [Table("APP_PLANTA")]
    public class AppPlanta
    {
        [Key]
        public int Id_planta { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom_planta { get; set; } = string.Empty;

        // Camp perquè l'Admin activi/desactivi plantes
        public bool Activa { get; set; } = true;

        // Relació (Una planta pot tenir molts registres a la taula pont)
        public virtual ICollection<AppUsuariPlanta>? UsuarisPlantes { get; set; }
    }
}