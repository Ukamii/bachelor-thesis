using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Praca
{
    /// <summary>
    /// Logika interakcji dla klasy AddManufacturer.xaml
    /// </summary>
    /// 


    public partial class AddManufacturerPage : Page
    {

        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();
        private CreateAssistant assistant = new CreateAssistant();

        public AddManufacturerPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            ReturnButton.Visibility = Visibility.Hidden;
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            CountryComboBox.ItemsSource = assistant.LoadCountries(sqlConnection);

            if (session.EditID != 0)
            {
                ReturnButton.Visibility = Visibility.Visible;

                LoadManufacturer(sqlConnection);
            }

            connection.CloseConnection(sqlConnection);
        }

        private void AddNewManufacturerButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            int errorCode;

            errorCode = FillInChecker();

            if(errorCode == 0)
            {
                errorCode = CheckExistingManufacturer(sqlConnection);

                if(errorCode != -1)
                {
                    if (session.EditID != 0)
                    {
                        UpdateManufacturer(sqlConnection);
                    }
                    else
                    {
                        CreateManufacturer(sqlConnection);
                    }
                }
            }

            connection.CloseConnection(sqlConnection);
        }

        private void CreateManufacturer(SqlConnection sqlConnection)
        {
            try
            {
                int countryId = 0, domain_id = 0;
                SqlCommand command = sqlConnection.CreateCommand();
                SqlTransaction transaction;

                string[] email = EmailBox.Text.Split('@');

                string email_name = email[0];
                string email_domain = email[1];

                email_domain = email_domain.Insert(0, "@");

                transaction = sqlConnection.BeginTransaction("Adding Manufacturer");

                command.Connection = sqlConnection;
                command.Transaction = transaction;
                try
                {
                    if (CountryComboBox.SelectedIndex == -1)
                    {
                        countryId = assistant.CreateCountry(command, CountryComboBox.Text);
                    }
                    else
                    {
                        countryId = assistant.LoadCountry(command, CountryComboBox.SelectedItem.ToString());
                    }

                    domain_id = assistant.LoadEmailDomain(command, email_domain);

                    command.CommandText = "INSERT INTO Emails(email_username, email_domain_id) VALUES(@EmailName, @EmailDomain)";
                    command.Parameters.AddWithValue("@EmailName", email_name);
                    command.Parameters.AddWithValue("@EmailDomain", domain_id);
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO Contacts(address, address_2, zip_code, phone_number, country_id, email_id) VALUES(@address, @address2, @zipcode, @phonenumber, @CountryID, @@IDENTITY);";
                    command.Parameters.AddWithValue("@address", AddressTextBox.Text);
                    command.Parameters.AddWithValue("@address2", Address2TextBox.Text);
                    command.Parameters.AddWithValue("@zipcode", ZipcodeTextBox.Text);
                    command.Parameters.AddWithValue("@phonenumber", PhonenumberTextBox.Text);
                    command.Parameters.AddWithValue("@CountryID", countryId);

                    command.ExecuteNonQuery();
                    command.CommandText = "INSERT INTO Manufacturers(contact_id, name) VALUES(@@IDENTITY, @name);";
                    command.Parameters.AddWithValue("@name", NameTextBox.Text);

                    command.ExecuteNonQuery();
                    transaction.Commit();

                    MessageBox.Show("Manufacturer added");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Commit Exception Type:" + ex.GetType() + "\nMessage: " + ex.Message);
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show("Commit Exception Type:" + ex2.GetType() + "\nMessage: " + ex2.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't add new manufacturer to the database \nMessage: " + ex.Message);
                throw;
            }
        }

        private void UpdateManufacturer(SqlConnection sqlConnection)
        {
            try
            {
                int contactID = 0;
                int countryId = 0;

                SqlCommand command = new SqlCommand("SELECT contact_id FROM Manufacturers WHERE manufacturer_id = @ManufacturerIDLoad", sqlConnection);
                command.Parameters.AddWithValue("@ManufacturerIDLoad", session.EditID);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        contactID = (int)reader[0];
                    }
                }

                reader.Close();

                SqlTransaction transaction;

                transaction = sqlConnection.BeginTransaction("Editing Manufacturer");


                command.Transaction = transaction;
                try
                {
                    string[] email = EmailBox.Text.Split('@');

                    string email_name = email[0];
                    string email_domain = email[1];

                    email_domain = email_domain.Insert(0, "@");

                    if (CountryComboBox.SelectedIndex == -1)
                    {
                        countryId = assistant.CreateCountry(command, CountryComboBox.Text);
                    }
                    else
                    {
                        countryId = assistant.LoadCountry(command, CountryComboBox.SelectedItem.ToString());
                    }

                    int domain_id = -1;
                    int email_id = -1;

                    domain_id = assistant.LoadEmailDomain(command, email_domain);

                    command.CommandText = "SELECT email_id FROM Contacts WHERE contact_id = @ContactID";
                    command.Parameters.AddWithValue("@ContactID", contactID);

                    reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader[0] == DBNull.Value)
                            {
                                email_id = -1;
                            }
                            else
                            {
                                email_id = (int)reader[0];
                            }
                        }
                    }
                    reader.Close();
                    command.Parameters.Clear();

                    if (email_id == -1)
                    {
                        command.CommandText = "INSERT INTO Emails(email_username, email_domain_id) VALUES(@EmailName, @EmailDomain)";
                        command.Parameters.AddWithValue("@EmailName", email_name);
                        command.Parameters.AddWithValue("@EmailDomain", domain_id);

                        command.ExecuteNonQuery();

                        command.CommandText = "UPDATE Contacts SET address = @address, address_2 = @address2, zip_code = @zipcode, phone_number = @phonenumber, country_id = @CountryId, email_id = @@IDENTITY WHERE contact_id = @ContactID";
                        command.Parameters.AddWithValue("@address", AddressTextBox.Text);
                        command.Parameters.AddWithValue("@address2", Address2TextBox.Text);
                        command.Parameters.AddWithValue("@zipcode", ZipcodeTextBox.Text);
                        command.Parameters.AddWithValue("@phonenumber", PhonenumberTextBox.Text);
                        command.Parameters.AddWithValue("@ContactID", contactID);
                        command.Parameters.AddWithValue("@CountryId", countryId);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "UPDATE Emails SET email_username = @EmailName, email_domain_id = @EmailDomain WHERE email_id = @EmailID";
                        command.Parameters.AddWithValue("@EmailName", email_name);
                        command.Parameters.AddWithValue("@EmailDomain", domain_id);
                        command.Parameters.AddWithValue("@EmailID", email_id);
                        command.ExecuteNonQuery();

                        command.Parameters.Clear();


                        command.CommandText = "UPDATE Contacts SET address = @address, address_2 = @address2, zip_code = @zipcode, phone_number = @phonenumber, country_id = @CountryId, email_id = @EmailID WHERE contact_id = @ContactID";
                        command.Parameters.AddWithValue("@address", AddressTextBox.Text);
                        command.Parameters.AddWithValue("@address2", Address2TextBox.Text);
                        command.Parameters.AddWithValue("@zipcode", ZipcodeTextBox.Text);
                        command.Parameters.AddWithValue("@phonenumber", PhonenumberTextBox.Text);
                        command.Parameters.AddWithValue("@ContactID", contactID);
                        command.Parameters.AddWithValue("@CountryId", countryId);
                        command.Parameters.AddWithValue("@EmailID", email_id);
                        command.ExecuteNonQuery();
                    }

                    command.CommandText = "UPDATE Manufacturers SET name = @name WHERE manufacturer_id = @ManufacturerID";
                    command.Parameters.AddWithValue("@name", NameTextBox.Text);
                    command.Parameters.AddWithValue("@ManufacturerID", session.EditID);
                    command.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Manufacturer updated");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Commit Exception Type:" + ex.GetType() + "\nMessage: " + ex.Message);
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show("Commit Exception Type:" + ex2.GetType() + "\nMessage: " + ex2.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't edit manufacturer\nMessage: " + ex.Message);
            }
        }

        private void LoadManufacturer(SqlConnection sqlConnection)
        {
            int contactID = 0;

            SqlCommand command = new SqlCommand("SELECT name, contact_id FROM Manufacturers WHERE manufacturer_id = @ManufacturerID", sqlConnection);

            command.Parameters.AddWithValue("@ManufacturerID", session.EditID);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    NameTextBox.Text = (string)reader[0];
                    contactID = (int)reader[1];
                }
            }

            reader.Close();

            command.CommandText = "SELECT C.address, C.address_2, CN.name, C.zip_code, C.phone_number, E.email_username, ED.domain_name FROM Contacts AS C JOIN Countries AS CN ON CN.country_id = C.country_id JOIN Emails as E ON E.email_id = C.email_id JOIN Email_domains as ED ON E.email_domain_id = ED.email_domain_id WHERE C.contact_id = @ContactID";
            command.Parameters.AddWithValue("@ContactID", contactID);

            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    AddressTextBox.Text = (string)reader[0];
                    Address2TextBox.Text = (string)reader[1];
                    CountryComboBox.SelectedItem = (string)reader[2];
                    ZipcodeTextBox.Text = (string)reader[3];
                    PhonenumberTextBox.Text = (string)reader[4];
                    EmailBox.Text = (string)reader[5];
                    EmailBox.Text += (string)reader[6];
                }
            }

            reader.Close();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;
            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }




        private int FillInChecker()
        {
            if(NameTextBox.Text == "")
            {
                MessageBox.Show("No name was given");
                return -1;
            }

            if(CountryComboBox.Text == "")
            {
                MessageBox.Show("No country was given");
                return -1;
            }

            if(ZipcodeTextBox.Text == "")
            {
                MessageBox.Show("No zipcode was given");
                return -1;
            }

            if(AddressTextBox.Text == "")
            {
                MessageBox.Show("No address was given");
                return -1;
            }

            if(PhonenumberTextBox.Text == "")
            {
                MessageBox.Show("No phone number was given");
                return -1;
            }

            if(!Regex.Match(PhonenumberTextBox.Text, @"^([0-9]{9})$").Success)
            {
                MessageBox.Show("Invalid phone number");
                return -1;
            }

            if(EmailBox.Text == "")
            {
                MessageBox.Show("No email address was given");
                return -1;
            }

            if(!new EmailAddressAttribute().IsValid(EmailBox.Text))
            {
                MessageBox.Show("Invalid email address");
                return -1;
            }

            return 0;
        }

        private int CheckExistingManufacturer(SqlConnection sqlConnection)
        {
            SqlCommand command = new SqlCommand("SELECT name, manufacturer_id FROM Manufacturers WHERE name = @name", sqlConnection);
            command.Parameters.AddWithValue("@name", NameTextBox.Text);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();

                if((int)reader["manufacturer_id"] != session.EditID )
                {
                    MessageBox.Show("Manufacturer with this name already exists in database");
                    reader.Close();
                    return -1;
                }
                else
                {
                    reader.Close();
                    return 0;
                }

            }
            else
            {
                reader.Close();
                return 0;
            }

        }
    }
}
