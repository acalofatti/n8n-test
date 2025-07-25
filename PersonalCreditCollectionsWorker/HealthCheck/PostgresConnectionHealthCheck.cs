using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace PersonalCreditCollectionsWorker.HealthCheck
{
    public class PostgresConnectionHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public PostgresConnectionHealthCheck(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken);
                return HealthCheckResult.Healthy("PostgreSQL is reachable");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("PostgreSQL unreachable", ex);
            }
        }
    }
}
