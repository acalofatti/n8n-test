using PersonalCreditCollectionsWorker.Contracts;
using PersonalCreditCollectionsWorker.Models;

namespace PersonalCreditCollectionsWorker.Services
{
    public class NovedadesProcessor : INovedadesProcessor
    {
        private readonly ISqlDataExtractor _extractor;
        private readonly IQueuePublisher _publisher;
        private readonly ILogger<NovedadesProcessor> _logger;
        private readonly IPostgresDataExtractor _postgresDataExtractor;

        public NovedadesProcessor(
            ISqlDataExtractor extractor,
            IQueuePublisher publisher,
            ILogger<NovedadesProcessor> logger,
            IPostgresDataExtractor postgresDataExtractor)
        {
            _extractor = extractor;
            _publisher = publisher;
            _logger = logger;
            _postgresDataExtractor = postgresDataExtractor; 
        }

        public async Task ProcesarNovedadesAsync()
        {
            _logger.LogInformation("Iniciando procesamiento de novedades...");
            var result = await ObtenerTimeStampPorTipo(1);

            var (recibos, anulacionesRecibos, notasDeCreditos, anulacionesNotas) = await ObtenerTodasLasNovedadesEnParalelo();

            LogConteos(recibos, anulacionesRecibos, notasDeCreditos, anulacionesNotas);

            var novedades = new List<Novedad>();
            novedades.AddRange(recibos);
            novedades.AddRange(anulacionesRecibos);
            novedades.AddRange(notasDeCreditos);
            novedades.AddRange(anulacionesNotas);

            if (novedades.Count == 0)
            {
                _logger.LogInformation("No se encontraron novedades.");
                return;
            }

            foreach (var novedad in novedades)
            {
                await _publisher.PublishAsync(MapToDto(novedad));
            }

            await RegistrarPunteros(recibos, anulacionesRecibos, notasDeCreditos, anulacionesNotas);

            _logger.LogInformation("Procesamiento de novedades finalizado. Total: {count}", novedades.Count);
        }

        private async Task<(List<Novedad> Recibos, List<Novedad> AnulacionesRecibos, List<Novedad> Notas, List<Novedad> AnulacionesNotas)> ObtenerTodasLasNovedadesEnParalelo()
        {
            var filtros = ObtenerParametrosFiltroNotasDeCreditos();

            var t1 = _extractor.ObtenerRecibos(await ObtenerTimeStampPorTipo(1));
            var t2 = _extractor.ObtenerAnulacionesRecibos(await ObtenerTimeStampPorTipo(2));
            var t3 = _extractor.ObtenerNovedadesDeNotasDeCreditos(filtros.SubTipos, filtros.Negocios, await ObtenerTimeStampPorTipo(3));
            var t4 = _extractor.ObtenerNovedadesAnulacionesDeNotasDeCreditos(filtros.SubTipos, filtros.Negocios, await ObtenerTimeStampPorTipo(4));

            await Task.WhenAll(t1, t2, t3, t4);

            var recibos = await t1;
            var anulacionesRecibos = await t2;
            var notas = await t3;
            var anulacionesNotas = await t4;
            await RegistrarPunteros(recibos, anulacionesRecibos, notas, anulacionesNotas);

            return (recibos, anulacionesRecibos, notas, anulacionesNotas);
        }

        private async Task RegistrarPunteros(List<Novedad> recibos, List<Novedad> anulacionesRecibos, List<Novedad> notas, List<Novedad> anulacionesNotas)
        {
            if (recibos.Count > 0)
                await RegistrarTimeStamp(tipo: 1, recibos[^1].TimeStamp); // último elemento

            if (anulacionesRecibos.Count > 0)
                await RegistrarTimeStamp(tipo: 2, anulacionesRecibos[^1].TimeStamp);

            if (notas.Count > 0)
                await RegistrarTimeStamp(tipo: 3, notas[^1].TimeStamp);

            if (anulacionesNotas.Count > 0)
                await RegistrarTimeStamp(tipo: 4, anulacionesNotas[^1].TimeStamp);
        }

        private static NovedadDto MapToDto(Novedad novedad) => new()
        {
            Recibo = novedad.Recibo,
            Cuota = novedad.Cuota,
            Fecha = novedad.Fecha,
            Credito = novedad.Credito,
            Importe = novedad.Importe,
            ImportePunitorios = novedad.ImportePunitorios,
            Tipo = novedad.Tipo,
            Estado = novedad.Estado
        };

        private static void LogConteos(List<Novedad> r, List<Novedad> ar, List<Novedad> nc, List<Novedad> anc)
        {
            Console.WriteLine($"Recibos: {r.Count}");
            Console.WriteLine($"Anulaciones recibos: {ar.Count}");
            Console.WriteLine($"Notas de crédito: {nc.Count}");
            Console.WriteLine($"Anulaciones de notas: {anc.Count}");
        }

        private static (List<int> SubTipos, List<int> Negocios) ObtenerParametrosFiltroNotasDeCreditos()
        {
            var subTipo = new List<int> { 1, 2, 3, 4, 8, 21, 60, 66, 67, 74, 82, 90, 94, 100, 101, 102, 103 };
            var negocios = new List<int> { 1, 2 };
            return (subTipo, negocios);
        }

        private async Task<long> ObtenerTimeStampPorTipo(int tipo)
        {
            return await _postgresDataExtractor.ObtenerUltimoTimeStampAsync(tipo);
        }

        private async Task RegistrarTimeStamp(int tipo, long timeStamp)
        {
            await _postgresDataExtractor.GuardarPunteroAsync(tipo, timeStamp);
        }
    }
}
