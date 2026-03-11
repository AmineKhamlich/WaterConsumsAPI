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

        // CORREGIT: Ara és un int? (perquè pot ser NULL a l'SQL)
        // Hem tret el [StringLength(20)] perquè als int no se'ls hi posa.
        public int? estat { get; set; }

        public DateTime data_creacio { get; set; }

        public DateTime? data_tancament { get; set; }

        // --- NOUS CAMPS DEL MOTOR D'ALARMES ---
        public DateTime? data_detectat_H { get; set; }

        public DateTime? data_detectat_HH { get; set; }

        // A l'SQL posa NOT NULL, així que aquí és int (sense l'interrogant)
        public int nivell_actual { get; set; }

        // --- RELACIÓ AMB USUARI (QUI TANCA) ---
        public int? ID_USUARI_TANCA { get; set; }

        [ForeignKey("ID_USUARI_TANCA")]
        public virtual AppUsuari? UsuariTanca { get; set; }
    }
}