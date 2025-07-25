namespace PersonalCreditCollectionsWorker.Models
{
    public class Novedad
    {
        public DateTime Fecha { get; set; }
        public string Recibo { get; set; }
        public int Cuota { get; set; }
        public string Credito { get; set; }
        public decimal Importe { get; set; }
        public decimal ImportePunitorios { get; set; }
        public byte Tipo { get; set; }
        public byte Estado { get; set; }
        public long TimeStamp { get; set; }
    }
}
