using System.ComponentModel.DataAnnotations; // Importa els atributs de validació com [Required] o [StringLength].

namespace WConsumsAPI.DTOs // Defineix l'espai de noms on viuen els DTOs (Data Transfer Objects).
{
    // Aquesta classe (DimCntDto) serveix per TRANSPORTAR dades entre l'API i l'usuari/client.
    // NO té dependències d'altres classes.
    // EL SEU OBJECTIU: Filtrar què mostrem de la base de dades (seguretat) i definir què acceptem quan creem/editem (validació).
    public class DimCntDto
    {
        // Identificador únic del comptador. És un enter (int).
        // Coincideix amb la Primary Key de la base de dades.
        public int ID { get; set; }

        // Descripció del comptador.
        // [StringLength(100)] valida que el text no superi els 100 caràcters.
        // '?' significa que pot ser nul (buit).
        [StringLength(100)]
        public string? Descripcio { get; set; }

        // Nom de la planta o ubicació on està el comptador.
        // [StringLength(50)] limita la llargada a 50 caràcters.
        [StringLength(50)]
        public string? Planta { get; set; }

        // Etiqueta tècnica del sensor o comptador (ex: 'FIC-101').
        // [StringLength(100)] limita la llargada a 100 caràcters.
        [StringLength(100)]
        public string? TagName { get; set; }
    }
}
