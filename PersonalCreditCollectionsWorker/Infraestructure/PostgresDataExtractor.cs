using Npgsql;
using PersonalCreditCollectionsWorker.Contracts;

namespace PersonalCreditCollectionsWorker.Infraestructure
{
    public class PostgresDataExtractor : IPostgresDataExtractor
    {
        private readonly string _connectionString;

        public PostgresDataExtractor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<long> ObtenerUltimoTimeStampAsync(int tipo)
        {
            const string query = "SELECT \"TimeStamp\" FROM public.\"CMP_NovedadesExtraccionPunteros\" WHERE \"Tipo\" = @tipo LIMIT 1";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tipo", tipo);

            var result = await cmd.ExecuteScalarAsync();

            return result != null && long.TryParse(result.ToString(), out var tstamp)
                ? tstamp
                : 0;
        }

        public async Task GuardarPunteroAsync(int tipo, long tstamp)
        {
            const string upsertQuery = @"
                INSERT INTO public.""CMP_NovedadesExtraccionPunteros"" (""Tipo"", ""TimeStamp"")
                VALUES (@tipo, @tstamp)
                ON CONFLICT (""Tipo"")
                DO UPDATE SET ""TimeStamp"" = EXCLUDED.""TimeStamp"";";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(upsertQuery, conn);
            cmd.Parameters.AddWithValue("@tipo", tipo);
            cmd.Parameters.AddWithValue("@tstamp", tstamp);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
