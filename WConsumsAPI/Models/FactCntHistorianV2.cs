using System.ComponentModel.DataAnnotations; // Per [Key]
using System.ComponentModel.DataAnnotations.Schema; // Per [Table], [DatabaseGenerated]

namespace WConsumsAPI.Models
{
    // Aquesta classe representa la taula d'HISTÒRICS ('FACT_CNT_HISTORIAN_V2').
    // Conté les lectures temporals dels comptadors.
    [Table("FACT_CNT_HISTORIAN_V2")] 
    public class FactCntHistorianV2
    {
        // Clau primària única per a cada lectura.
        [Key]
        // [DatabaseGenerated] amb 'Identity' diu a EF Core que la BD genera aquest número automàticament (AutoIncrement).
        // Així quan fem INSERT, no hem de passar l'ID.
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int ID { get; set; }

        // Foreign Key lògica: Relaciona aquesta lectura amb un Tag o Comptador concret.
        public int TagNameID { get; set; }

        // Data i hora real de la lectura (Timestamp).
        public DateTime FechaNoel { get; set; }

        // Inici del període mesurat (per dades agregades o diferencials).
        public DateTime FechaInicio { get; set; }

        // Fi del període mesurat.
        public DateTime FechaFin { get; set; }

        // Valor Diferencial: La quantitat consumida en aquest període (Fi - Inici).
        // double? permet decimals i valors nuls.
        public double? ValorDiferencial { get; set; }

        // Valor Absolut: La lectura del comptador en 'FechaFin'.
        // És el número que marques el "rellotge" del comptador.
        public double? ValorAbsoluto { get; set; }

        // Hora del dia (0-23). Útil per agrupar dades ràpidament per hores.
        public int? Hora { get; set; }

        // Any i Mes (ex: "2024-01"). Útil per agrupar per mesos.
        public string? AñoMes { get; set; }

        public double? ValorDifMod { get; set; }
    }
}
