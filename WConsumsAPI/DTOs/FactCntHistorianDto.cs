using System; // Necessari per utilitzar DateTime.
using System.ComponentModel.DataAnnotations; // Necessari per a la validació de dades.

namespace WConsumsAPI.DTOs
{
    // Classe DTO (Data Transfer Object) per a l'històric de lectures.
    // Serveix per transportar les dades de la taula FACT_CNT_HISTORIAN_V2 de forma segura.
    public class FactCntHistorianDto
    {
        // Identificador únic del registre històric (Primary Key).
        public int ID { get; set; }

        // Identificador del Tag (Identificador de la variable o sensor).
        // Relacionat amb la taula de configuració de tags.
        public int TagNameID { get; set; }

        // Data i hora real de la lectura (TimeStamp).
        // És un DateTime, que inclou dia, mes, any, hora, minuts i segons.
        public DateTime FechaNoel { get; set; }

        // Data d'inici del període que representa aquest valor (si és un agregat).
        public DateTime FechaInicio { get; set; }

        // Data de final del període.
        public DateTime FechaFin { get; set; }

        // Valor diferencial (el consum o increment en aquest període).
        // És un 'double?' perquè pot tenir decimals i pot ser nul.
        public double? ValorDiferencial { get; set; }

        // Valor absolut (la lectura acumulada del comptador en aquell moment).
        // També pot tenir decimals i ser nul.
        public double? ValorAbsoluto { get; set; }

        // Hora del dia (0-23) per facilitar agrupar o filtrar per franges horàries.
        // És nullable (int?) per si algun registre no ho té informat.
        public int? Hora { get; set; }

        // Cadena de text que representa l'Any i el Mes (ex: "2023-10").
        // Útil per fer informes mensuals sense haver de calcular dates complexes.
        [StringLength(20)] // Validació: no pot superar els 20 caràcters.
        public string? AñoMes { get; set; }

        // Valor diferencial modificat (pot ser el mateix que ValorDiferencial o ajustat).
        public double? ValorDifMod { get; set; }
    }
}
