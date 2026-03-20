namespace WConsumsAPI.DTOs
{
    public class IncidenciaVistaDto
    {
        public int Id { get; set; }
        public string Gravetat { get; set; } = string.Empty;
        public string Estat { get; set; } = string.Empty;
        public string Ubicacio { get; set; } = string.Empty;
        public string DescripcioComptador { get; set; } = string.Empty;
        public int? LimitH { get; set; }
        public int? LimitHH { get; set; }
        public string DataCreacio { get; set; } = string.Empty;
        public string? DataTancament { get; set; }
        public string TecnicTancament { get; set; } = string.Empty;
        public string TempsTranscorregut { get; set; } = string.Empty;
        public double ConsumDiaAlarma { get; set; }
        public string? Descripcio { get; set; }
        public string? DescripcioSolucio { get; set; }
        public string? Foto { get; set; }

        // Mantinguts per AlarmaCard (alarmes actives)
        public string Comptador { get; set; } = string.Empty;
        public string DetallAlarma { get; set; } = string.Empty;
        public DateTime? HoraAvisH { get; set; }
        public DateTime? HoraCriticHH { get; set; }
        public double ConsumRealAvui { get; set; }
    }
}