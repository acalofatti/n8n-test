using System.Threading.Tasks;

namespace PersonalCreditCollectionsWorker.Contracts
{
    public interface IPostgresDataExtractor
    {
        Task<long> ObtenerUltimoTimeStampAsync(int tipo);
        Task GuardarPunteroAsync(int tipo, long tstamp);
    }
}
