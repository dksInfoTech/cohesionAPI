using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Product.Dapper.Lib
{
    public class DapperDbContext : IDapperDbContext
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DapperDbContext(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<T>> ExecuteStoreProcedure<T>(string spName, object parameters = null)
        {
            using (var conn = _connectionFactory.GetConnection())
            {
                return await conn.QueryAsync<T>(spName, parameters, commandTimeout: 0, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<T> Get<T>(string query)
        {
            using (var conn = _connectionFactory.GetConnection())
            {
                return await conn.QueryFirstOrDefaultAsync<T>(query);
            }
        }

        public async Task<IEnumerable<T>> GetAll<T>(string query)
        {
            using (var conn = _connectionFactory.GetConnection())
            {
                return await conn.QueryAsync<T>(query);
            }
        }

    }
}
