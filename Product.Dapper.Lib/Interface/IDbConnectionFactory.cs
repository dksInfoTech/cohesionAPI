using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Product.Dapper.Lib
{
    public interface IDbConnectionFactory
    {
        public IDbConnection GetConnection();
        public void CloseConnection();
    }
}
