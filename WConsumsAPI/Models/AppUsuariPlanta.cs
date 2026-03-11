using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WConsumsAPI.Models
{
    [Table("APP_USUARI_PLANTA")]
    public class AppUsuariPlanta
    {
        // Nota: Les claus primàries compostes s'han de definir al DbContext de C#
        public int Id_usuari { get; set; }
        [ForeignKey("Id_usuari")]
        public virtual AppUsuari? Usuari { get; set; }

        public int Id_planta { get; set; }
        [ForeignKey("Id_planta")]
        public virtual AppPlanta? Planta { get; set; }
    }
}