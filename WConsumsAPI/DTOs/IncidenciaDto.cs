namespace WConsumsAPI.DTOs
{
    public class IncidenciaDto
    {
        public int Id { get; set; }
        public string Descripcio { get; set; }
        public string foto { get; set; }
        public string Solucio { get; set; }
        public DateTime Data_creacio { get; set; }
        public DateTime? Data_tancament { get; set; }
        public string Estat { get; set; }
        public int ConsumId { get; set; }
    }
}
