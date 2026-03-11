namespace WConsumsAPI.DTOs
{
    public class IncidenciaVistaDto
    {
        public int Id { get; set; }
        public DateTime DataCreacio { get; set; }
        public string Gravetat { get; set; } = string.Empty; // "AVÍS", "ALERTA", "CRÍTICA"
        public string Estat { get; set; } = string.Empty;    // "Activa", "Tancada"
        public string Comptador { get; set; } = string.Empty;
        public string Ubicacio { get; set; } = string.Empty;
        public string DetallAlarma { get; set; } = string.Empty;
        // Poden ser nuls
        public DateTime? HoraAvisH { get; set; }
        public DateTime? HoraCriticHH { get; set; }
        public double ConsumRealAvui { get; set; }
        public int? LimitH { get; set; }
        public int? LimitHH { get; set; }
        public DateTime? DataTancament { get; set; }
        public string TempsTranscorregut { get; set; } = string.Empty; // Format "X dies, Y hores"
    }
}
