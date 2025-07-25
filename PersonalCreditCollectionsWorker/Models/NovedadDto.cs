namespace PersonalCreditCollectionsWorker.Models
{
    public class NovedadDto
    {
        public DateTime Fecha { get; set; }
        public string Recibo { get; set; }
        public int Cuota { get; set; }
        public string Credito { get; set; }
        public decimal Importe { get; set; }
        public decimal ImportePunitorios { get; set; }
        public byte Tipo { get; set; }
        public byte Estado { get; set; }
    }
}
