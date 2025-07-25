using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PersonalCreditCollectionsWorker.HealthCheck
{
    public class SqlConnectionHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public SqlConnectionHealthCheck(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(cancellationToken);
                return HealthCheckResult.Healthy("SQL Server is reachable");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("SQL Server unreachable", ex);
            }
        }
    }

}
