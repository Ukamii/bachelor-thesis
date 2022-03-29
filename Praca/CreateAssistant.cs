using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Praca
{
    class CreateAssistant
    {
        public List<string> LoadCountries(SqlConnection sqlConnection)
        {
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter("SELECT name FROM Countries", sqlConnection);

            da.Fill(dt);

            List<string> countryList = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                countryList.Add(dr["name"].ToString());
            }
            return countryList;
        }

        public int LoadEmailDomain(SqlCommand command, string domain)
        {
            int domain_id = -1;

            command.CommandText = "SELECT email_domain_id FROM Email_domains WHERE domain_name = @domain_name";
            command.Parameters.AddWithValue("@domain_name", domain);

            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                domain_id = (int)reader[0];

                reader.Close();
                return domain_id;
            }
            else
            {
                reader.Close();
                domain_id = CreateEmailDomain(command, domain);

                return domain_id;
            }
        }

        public int CreateEmailDomain(SqlCommand command, string domain)
        {
            int domain_id = -1;

            command.CommandText = "INSERT INTO Email_domains(domain_name) VALUES(@NewDomain)";
            command.Parameters.AddWithValue("@NewDomain", domain);
            command.ExecuteNonQuery();

            command.CommandText = "SELECT email_domain_id FROM Email_domains WHERE email_domain_id = @@IDENTITY";

            SqlDataReader reader = command.ExecuteReader();
            reader.Read();

            domain_id = (int)reader[0];

            reader.Close();

            return domain_id;

        }

        public int LoadCountry(SqlCommand command, string name)
        {
            int countryId = 0;
            command.CommandText = "SELECT country_id FROM Countries WHERE name = @CountryName";
            command.Parameters.AddWithValue("@CountryName", name);

            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                countryId = (int)reader[0];
            }

            reader.Close();

            return countryId;
        }

        public int CreateCountry(SqlCommand command, string name)
        {
            int countryId = 0;
            command.CommandText = "INSERT INTO Countries (name) VALUES (@NewCountryName)";
            command.Parameters.AddWithValue("@NewCountryName", name);

            command.ExecuteNonQuery();

            command.CommandText = "SELECT country_id FROM Countries WHERE country_id = @@IDENTITY";

            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                countryId = (int)reader[0];
            }

            reader.Close();

            return countryId;
        }
    }
}
