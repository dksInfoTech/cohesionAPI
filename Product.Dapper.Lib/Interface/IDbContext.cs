using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Product.Dapper.Lib
{
    public interface IDapperDbContext
    {
        public Task<T> Get<T>(string query);
        public Task<IEnumerable<T>> GetAll<T>(string query);
        public Task<IEnumerable<T>> ExecuteStoreProcedure<T>(string spName, object parameters = null);
    }
}
