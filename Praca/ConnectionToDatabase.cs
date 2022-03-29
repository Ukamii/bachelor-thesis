using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Praca
{
    class ConnectionToDatabase
    {

        private static SqlConnection _Connection;

        public SqlConnection Connection => _Connection;


        public ConnectionToDatabase()
        {

            try
            {
                //Połączenie z App.config
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["DB"];
                _Connection = new SqlConnection(settings.ConnectionString); 

                /*SqlConnectionStringBuilder SqlBuilderLocal = new SqlConnectionStringBuilder();
                SqlBuilderLocal.DataSource = @"(LocalDB)\MSSQLLocalDB";
                SqlBuilderLocal.AttachDBFilename = @"|DataDirectory|PracaDB.mdf";
                SqlBuilderLocal.IntegratedSecurity = true;
                SqlBuilderLocal.ConnectTimeout = 5;

                _Connection = new SqlConnection(SqlBuilderLocal.ConnectionString);*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't create the connection to the database \nMessage: " + ex.Message);
            }

        }

        public void OpenConnection(SqlConnection con)
        {
            try
            {
                if (con.State == System.Data.ConnectionState.Closed)
                {
                    con.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't open the connection to the database \nMessage: " + ex.Message);
            }

        }

        public void CloseConnection(SqlConnection con)
        {
            try
            {
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't close the connection to the database \nMessage: " + ex.Message);
            }

        }
    }
}
