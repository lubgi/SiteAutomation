using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Text;
using MySql.Data.MySqlClient;

namespace SiteAutomation
{
    sealed class DBConnection // : IDisposable
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnectionString"].ConnectionString;
        private readonly MySqlConnection MySQLConnection;

        private DBConnection()
        {
            MySQLConnection = new MySqlConnection(connectionString);
            MySQLConnection.Open();

        }

        public static MySqlConnection GetMySQLConnection()
        {
            return new DBConnection().MySQLConnection;
        }

        //public void Dispose()
        //{
        //    MySQLConnection.Close();
        //}
    }
}
