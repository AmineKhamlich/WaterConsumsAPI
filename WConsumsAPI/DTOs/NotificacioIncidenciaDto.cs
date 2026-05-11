namespace WConsumsAPI.DTOs
{
    public class NotificacioIncidenciaDto
    {
        public int Id { get; set; }
        public int IdDimCnt { get; set; }
        public string Titol { get; set; } = string.Empty;
        public string Missatge { get; set; } = string.Empty;
        public string Gravetat { get; set; } = string.Empty;
        public string Ubicacio { get; set; } = string.Empty;
        public string Comptador { get; set; } = string.Empty;
        public string TagName { get; set; } = string.Empty;
        public string DetallAlarma { get; set; } = string.Empty;
        public double ConsumRealAvui { get; set; }
        public int? LimitH { get; set; }
        public int? LimitHH { get; set; }
        public DateTime DataCreacio { get; set; }
        public DateTime? HoraAvisH { get; set; }
        public DateTime? HoraCriticHH { get; set; }
        public int NivellActual { get; set; }
    }
}
