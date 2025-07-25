using Microsoft.Data.SqlClient;
using System.Data;
using System.Transactions;
using PersonalCreditCollectionsWorker.Contracts;
using PersonalCreditCollectionsWorker.Models;

namespace PersonalCreditCollectionsWorker.Infraestructure
{
    public class SqlDataExtractor : ISqlDataExtractor
    {
        private readonly string _connectionString;

        public SqlDataExtractor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Novedad>> ObtenerRecibos(long timestamp)
        {
            var novedades = new List<Novedad>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            SqlCommand command = conn.CreateCommand();
            command.CommandText = "CMP_Recibo_SEL_porTStamp";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@numeroTStamp", timestamp);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                novedades.Add(new Novedad
                {
                    Recibo = reader["NUMEROCOMPROBANTE"].ToString(),
                    Cuota = (int)reader["CUOTA"],
                    Fecha = DateTime.Parse(reader["FECHA"].ToString()),
                    Credito = reader["NUMEROCREDITO"].ToString(),
                    Importe = (decimal)reader["IMPORTE"],
                    ImportePunitorios = (decimal)reader["IMPORTEPUNITORIOS"],
                    Tipo = 1,
                    Estado = 1,
                    TimeStamp = reader["TStamp"] is byte[] bytes && bytes.Length == 8
                            ? BitConverter.ToInt64(bytes.Reverse().ToArray(), 0)
                            : 0
                });
            }

            return novedades;
        }

        public async Task<List<Novedad>> ObtenerAnulacionesRecibos(long timestamp)
        {
            var novedades = new List<Novedad>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            SqlCommand command = conn.CreateCommand();
            command.CommandText = "CMP_AnulacionRecibo_SEL_porTStamp";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@numeroTStamp", timestamp);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                novedades.Add(new Novedad
                {
                    Recibo = reader["NUMEROCOMPROBANTE"].ToString(),
                    Cuota = (int)reader["CUOTA"],
                    Fecha = DateTime.Parse(reader["FECHA"].ToString()),
                    Credito = reader["NUMEROCREDITO"].ToString(),
                    Importe = (decimal)reader["IMPORTE"],
                    ImportePunitorios = (decimal)reader["IMPORTEPUNITORIOS"],
                    Tipo = 2,
                    Estado = 1,
                    TimeStamp = reader["TStamp"] is byte[] bytes && bytes.Length == 8
                            ? BitConverter.ToInt64(bytes.Reverse().ToArray(), 0)
                            : 0
                });
            }

            return novedades;
        }

        public async Task<List<Novedad>> ObtenerNovedadesAnulacionesDeNotasDeCreditos(List<int> tipos, List<int> negocios, long timestamp)
        {
            var novedades = new List<Novedad>();
            var subTipoTable = CreateSubtipoTable(tipos);
            var negocioTable = CreateNegocioTable(negocios);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var command = conn.CreateCommand();
            command.CommandText = "CMP_AnulacionNotaDeCredito_SEL_CreditoWeb";
            command.CommandType = CommandType.StoredProcedure;

            // Parámetros de fechas
            //command.Parameters.AddWithValue("@desde", DateTime.MinValue);
            //command.Parameters.AddWithValue("@hasta", DateTime.Now);

            // Parámetros opcionales sin filtro
            command.Parameters.AddWithValue("@numeroDocumento", 0);
            command.Parameters.AddWithValue("@cuitCuil", 0);
            command.Parameters.AddWithValue("@numeroDeCredito", string.Empty);
            command.Parameters.AddWithValue("@tStamp", timestamp);

            // Parámetros estructurados
            var paramSubTipo = command.Parameters.AddWithValue("@subTipo", subTipoTable);
            paramSubTipo.SqlDbType = SqlDbType.Structured;
            paramSubTipo.TypeName = "Tabla_CMP_SubtipoPorTipo_pk";

            var paramNegocio = command.Parameters.AddWithValue("@negocio", negocioTable);
            paramNegocio.SqlDbType = SqlDbType.Structured;
            paramNegocio.TypeName = "Tabla_GN_LineaDeNegocio_PK";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                novedades.Add(new Novedad
                {
                    Recibo = $"{reader["Letra"]}{reader["Numero"]}",
                    Cuota = Convert.ToInt32(reader["Cuota"]),
                    Fecha = DateTime.Parse(reader["FechaAlta"].ToString()),
                    Credito = reader["NumeroDeCredito"].ToString(),
                    Importe = Convert.ToDecimal(reader["ImporteAplicado"]),
                    ImportePunitorios = 0,
                    Tipo = 4,
                    Estado = 1,
                    TimeStamp = reader["TStamp"] is byte[] bytes && bytes.Length == 8
                            ? BitConverter.ToInt64(bytes.Reverse().ToArray(), 0)
                            : 0
                });
            }

            return novedades;
        }


        public async Task<List<Novedad>> ObtenerNovedadesDeNotasDeCreditos(List<int> tipos, List<int> negocios, long timestamp)
        {
            var novedades = new List<Novedad>();
            var subTipoTable = CreateSubtipoTable(tipos);
            var negocioTable = CreateNegocioTable(negocios);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var command = conn.CreateCommand();
            command.CommandText = "CMP_NotaDeCredito_SEL_CreditoWeb";
            command.CommandType = CommandType.StoredProcedure;

            // Parámetros de fechas
            //command.Parameters.AddWithValue("@desde", DateTime.MinValue);
            //command.Parameters.AddWithValue("@hasta", DateTime.Now);

            // Parámetros opcionales sin filtro
            command.Parameters.AddWithValue("@numeroDocumento", 0);
            command.Parameters.AddWithValue("@cuitCuil", 0);
            command.Parameters.AddWithValue("@numeroDeCredito", string.Empty);
            command.Parameters.AddWithValue("@tStamp", timestamp);

            // Parámetros estructurados
            var paramSubTipo = command.Parameters.AddWithValue("@subTipo", subTipoTable);
            paramSubTipo.SqlDbType = SqlDbType.Structured;
            paramSubTipo.TypeName = "Tabla_CMP_SubtipoPorTipo_pk";

            var paramNegocio = command.Parameters.AddWithValue("@negocio", negocioTable);
            paramNegocio.SqlDbType = SqlDbType.Structured;
            paramNegocio.TypeName = "Tabla_GN_LineaDeNegocio_PK";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                novedades.Add(new Novedad
                {
                    Recibo = $"{reader["Letra"]}{reader["Numero"]}",
                    Cuota = reader["cuota"] != DBNull.Value ? Convert.ToInt32(reader["cuota"]) : 0,
                    Fecha = DateTime.Parse(reader["FechaAlta"].ToString()),
                    Credito = reader["NumeroDeCredito"].ToString(),
                    Importe = Convert.ToDecimal(reader["ImporteAplicado"]),
                    ImportePunitorios = 0,
                    Tipo = 3,
                    Estado = 1,
                    TimeStamp = reader["TStamp"] is byte[] bytes && bytes.Length == 8
                            ? BitConverter.ToInt64(bytes.Reverse().ToArray(), 0)
                            : 0
                });
            }

            return novedades;
        }

        private DataTable CreateSubtipoTable(List<int> tipos)
        {
            var table = new DataTable();
            table.Columns.Add("iden", typeof(int));

            foreach (var tipo in tipos)
                table.Rows.Add(tipo);

            return table;
        }

        private DataTable CreateNegocioTable(List<int> negocios)
        {
            var table = new DataTable();
            table.Columns.Add("iden", typeof(int));

            foreach (var negocio in negocios)
                table.Rows.Add(negocio);

            return table;
        }

    }
}
