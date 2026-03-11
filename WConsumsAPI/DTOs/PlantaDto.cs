using System.Collections.Generic;

namespace WConsumsAPI.DTOs
{
    // DTO per enviar la llista de plantes a l'App Android
    public class PlantaDto
    {
        public int Id_planta { get; set; }
        public string Nom_planta { get; set; } = string.Empty;
        public bool Activa { get; set; }
    }

    // DTO per rebre de l'App quines plantes han d'estar actives
    public class UpdatePlantesActivesDto
    {
        // Llista d'IDs de les plantes que l'Admin ha marcat amb el Checkbox
        public List<int> IdsPlantesActives { get; set; } = new List<int>();
    }
}