using PersonalCreditCollectionsWorker.Models;

namespace PersonalCreditCollectionsWorker.Contracts
{
    public interface ISqlDataExtractor
    {
        Task<List<Novedad>> ObtenerRecibos(long timestamp);
        Task<List<Novedad>> ObtenerAnulacionesRecibos(long timestamp);
        Task<List<Novedad>> ObtenerNovedadesAnulacionesDeNotasDeCreditos(List<int> tipos, List<int> negocios, long timestamp);
        Task<List<Novedad>> ObtenerNovedadesDeNotasDeCreditos(List<int> tipos, List<int> negocios, long timestamp);
    }
}
