using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using APIAlturas.Models;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace APIAlturas.DAO
{
    public class UsersDAO
    {
        private IConfiguration configuration;

        public UsersDAO(IConfiguration _configuration)
        {
            this.configuration = _configuration;
        }

        public User Find(string id)
        {
            using (SqlConnection conn = new SqlConnection(configuration.GetConnectionString("ExemploJWT")))
            {
                return conn.QueryFirstOrDefault<User>(
                    "SELECT UserID, AccessKey " +
                    "FROM dbo.Users " +
                    "WHERE UserID = @userID",
                    new { UserID = id });
            }
        }
    }
}
