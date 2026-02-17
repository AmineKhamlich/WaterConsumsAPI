using System.ComponentModel.DataAnnotations; // Atributs de validació (Key, Required...)
using System.ComponentModel.DataAnnotations.Schema; // Atributs de mapeig a BD (Table, Column...)

namespace WConsumsAPI.Models
{
    // Aquesta classe MAPEJA (representa) exactament una fila de la taula 'DIM_CNT'.
    // L'atribut [Table] indica el nom real de la taula a la Base de Dades.
    [Table("DIM_CNT")] 
    public class DimCnt
    {
        // [Key] indica que aquesta propietat és la CLAU PRIMÀRIA (Primary Key).
        // És l'identificador únic de cada comptador.
        [Key] 
        public int ID { get; set; }

        // Identificador de la font de dades original (si n'hi ha).
        public int SourceID { get; set; }

        // Identificador del tipus de quantitat o unitat mesurada.
        public short QuantityID { get; set; } 

        // Descripció humana del comptador (ex: "Consum Aigua Planta 1").
        // '?' vol dir que la columna a la BD permet NULLs, així que a C# també.
        public string? Descripcio { get; set; }

        // Nom de la planta o zona.
        public string? Planta { get; set; }

        // Totalitzador de la planta (pot ser un valor acumulat).
        public int TotalPlanta { get; set; }

        // Pot indicar si és un comptador final o parcial.
        public int Final { get; set; }

        // Indica si pertany a processos de neteja.
        // int? vol dir que pot ser NULL si no aplica.
        public int? Neteja { get; set; } 

        // Nom tècnic del tag o sensor (ex: "FIC-203").
        public string? TagName { get; set; }

        // 1 = Habilitat, 0 = Deshabilitat.
        public int? Habilitat { get; set; }

        // Possiblement flags per indicar si és comptador d'entrada o sortida.
        public int? Entrada { get; set; }
        public int? Sortida { get; set; }

        // Nom del servidor d'històrics associat.
        public string? SrvHistorian { get; set; }

        // Horaris o índexs relacionats amb la neteja.
        public int? IniciNeteja { get; set; }
        public int? FiNeteja { get; set; }
        public int? SP_H_ACUM { get; set; }
    }
}
