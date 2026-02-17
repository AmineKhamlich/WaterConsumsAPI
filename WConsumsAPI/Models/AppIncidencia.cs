using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WConsumsAPI.Models
{
    [Table("APP_INCIDENCIA")]
    public class AppIncidencia
    {
        [Key]
        public int ID_INCIDENCIA { get; set; }
        // Relació lògica amb la taula DIM_CNT de l'altra BBDD
        public int ID_DIM_CNT { get; set; }
        public string? descripcio { get; set; } // nvarchar(max) no necessita StringLength
        [StringLength(255)]
        public string? foto { get; set; }
        public string? descripcio_solucio { get; set; }
        [StringLength(20)]
        public string? estat { get; set; }
        // datetime NOT NULL
        public DateTime data_creacio { get; set; }
        // datetime NULL
        public DateTime? data_tancament { get; set; }
        [Required]
        [StringLength(50)]
        public string tipus_incidencia { get; set; } = "CONSUM_ELEVAT";
        // --- RELACIÓ AMB USUARI (QUI TANCA) ---
        public int? ID_USUARI_TANCA { get; set; }
        [ForeignKey("ID_USUARI_TANCA")]
        public virtual AppUsuari? UsuariTanca { get; set; }
    }
}
