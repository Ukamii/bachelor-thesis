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
    /// Logika interakcji dla klasy AddUser.xaml
    /// </summary>
    public partial class AddUserPage : Page
    {

        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();
        private Hashing hashing = new Hashing();
        private CreateAssistant assistant = new CreateAssistant();

        private bool LoginChanged = false;
        private bool PasswordChanged = false;
        private bool AdminLoad = false;
        private bool AdminChange = false;

        public AddUserPage()
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

                LoadUser(sqlConnection);
            }
            connection.CloseConnection(sqlConnection);
        }


        private void AddNewUserButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            int errorCode;

            errorCode = FillInChecker();

            if(errorCode == 0)
            {
                if (session.EditID != 0)
                {
                    UpdateUser(sqlConnection);
                }
                else
                {
                    CreateUser(sqlConnection);
                }
            }
            connection.CloseConnection(sqlConnection);
        }

        private void LoadUser(SqlConnection sqlConnection)
        {
            int contactID = 0;
            int credentialID = 0;

            SqlCommand command = new SqlCommand("SELECT name, surname, contact_id, credential_id FROM Users WHERE user_id = @UserID", sqlConnection);

            command.Parameters.AddWithValue("@UserID", session.EditID);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                NameTextBox.Text = (string)reader[0];
                SurnameTextBox.Text = (string)reader[1];
                contactID = (int)reader[2];
                credentialID = (int)reader[3];
            }

            reader.Close();


            command.CommandText = "SELECT C.address, C.address_2, CN.name, C.zip_code, C.phone_number, E.email_username, ED.domain_name FROM Contacts AS C JOIN Countries AS CN ON CN.country_id = C.country_id JOIN Emails as E ON E.email_id = C.email_id JOIN Email_domains as ED ON E.email_domain_id = ED.email_domain_id WHERE C.contact_id = @ContactID";
            command.Parameters.AddWithValue("@ContactID", contactID);

            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                AddressTextBox.Text = (string)reader[0];
                Address2TextBox.Text = (string)reader[1];
                CountryComboBox.SelectedItem = (string)reader[2];
                ZipcodeTextBox.Text = (string)reader[3];
                PhonenumberTextBox.Text = (string)reader[4];
                EmailBox.Text = (string)reader[5];
                EmailBox.Text += (string)reader[6];
            }

            reader.Close();

            command.CommandText = "SELECT login FROM Credentials WHERE credential_id = @CredentialID";
            command.Parameters.AddWithValue("@CredentialID", credentialID);

            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                LoginTextBox.Text = (string)reader[0];
            }

            reader.Close();

            command.CommandText = "SELECT admin_id FROM Administrators WHERE admin_id = @AdminID";
            command.Parameters.AddWithValue("@AdminID", session.EditID);

            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                AdminCheckBox.IsChecked = true;
                AdminLoad = true;
            }

            reader.Close();
        }

        private void UpdateUser(SqlConnection sqlConnection)
        {
            try
            {
                int contactID = 0;
                int credentialID = 0;
                int countryId = 0;
                AdminChange = (bool)AdminCheckBox.IsChecked;

                SqlCommand command = new SqlCommand("SELECT contact_id, credential_id FROM Users WHERE user_id = @UserIDLoad", sqlConnection);
                command.Parameters.AddWithValue("@UserIDLoad", session.EditID);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    contactID = (int)reader[0];
                    credentialID = (int)reader[1];
                }

                reader.Close();

                SqlTransaction transaction;

                transaction = sqlConnection.BeginTransaction("Editing User");


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
                        reader.Read();
                        if (reader[0] == DBNull.Value)
                        {
                            email_id = -1;
                        }
                        else
                        {
                            email_id = (int)reader[0];
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


                    command.CommandText = "UPDATE Users SET name = @name, surname = @surname WHERE user_id = @UserID";
                    command.Parameters.AddWithValue("@name", NameTextBox.Text);
                    command.Parameters.AddWithValue("@surname", SurnameTextBox.Text);
                    command.Parameters.AddWithValue("@UserID", session.EditID);
                    command.ExecuteNonQuery();

                    if (LoginChanged)
                    {
                        command.CommandText = "UPDATE Credentials SET login = @login WHERE credential_id = @CredentialLoginID";
                        command.Parameters.AddWithValue("@login", LoginTextBox.Text);
                        command.Parameters.AddWithValue("@CredentialLoginID", credentialID);

                        command.ExecuteNonQuery();
                    }

                    if (PasswordChanged)
                    {
                        string HashedPass = hashing.Sha256Hashing(PasswordTextBox.Password.ToString());
                        command.CommandText = "UPDATE Credentials SET password = @password WHERE credential_id = @CredentialPasswordID";
                        command.Parameters.AddWithValue("@password", HashedPass);
                        command.Parameters.AddWithValue("@CredentialPasswordID", credentialID);

                        command.ExecuteNonQuery();
                    }

                    if (AdminChange && !AdminLoad)
                    {
                        command.CommandText = "INSERT INTO Administrators(admin_id) VALUES(@CredentialIDAdmin)";
                        command.Parameters.AddWithValue("@CredentialIDAdmin", session.EditID);
                        command.ExecuteNonQuery();
                    }

                    if (!AdminChange && AdminLoad)
                    {
                        command.CommandText = "DELETE FROM Administrators WHERE admin_id = @CredentialIDAdmin";
                        command.Parameters.AddWithValue("@CredentialIDAdmin", session.EditID);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("User updated");
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
                MessageBox.Show("Couldn't edit user\nMessage: " + ex.Message);
            }
        }

        private void CreateUser(SqlConnection sqlConnection)
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

                transaction = sqlConnection.BeginTransaction("Adding User");

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

                    command.CommandText = "INSERT INTO Contacts(address, address_2, zip_code, phone_number, country_id, email_id) VALUES(@address, @address2, @zipcode, @phonenumber,  @CountryID, @@IDENTITY);";
                    command.Parameters.AddWithValue("@address", AddressTextBox.Text);
                    command.Parameters.AddWithValue("@address2", Address2TextBox.Text);
                    command.Parameters.AddWithValue("@zipcode", ZipcodeTextBox.Text);
                    command.Parameters.AddWithValue("@phonenumber", PhonenumberTextBox.Text);
                    command.Parameters.AddWithValue("@CountryID", countryId);

                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO Users(contact_id, name, surname) VALUES(@@IDENTITY, @name, @surname);";
                    command.Parameters.AddWithValue("@name", NameTextBox.Text);
                    command.Parameters.AddWithValue("@surname", SurnameTextBox.Text);
                    command.ExecuteNonQuery();

                    string HashedPass = hashing.Sha256Hashing(PasswordTextBox.Password.ToString());

                    command.CommandText = "INSERT INTO Credentials(login, password) VALUES(@login, @password);";
                    command.Parameters.AddWithValue("@login", LoginTextBox.Text);
                    command.Parameters.AddWithValue("@password", HashedPass);
                    command.ExecuteNonQuery();

                    command.CommandText = "UPDATE Users SET credential_id = @@IDENTITY WHERE user_id = (SELECT MAX(user_id) FROM Users);";
                    command.ExecuteNonQuery();

                    if ((bool)AdminCheckBox.IsChecked)
                    {
                        command.CommandText = "INSERT INTO Administrators(admin_id) VALUES(@@IDENTITY)";
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("User added");
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
                MessageBox.Show("Couldn't add new user to the database \nMessage: " + ex.Message);
            }
        }

        private void LoginTextChanged(object sender, TextChangedEventArgs e)
        {
            LoginChanged = true;
        }

        private void PasswordTextChanged(object sender, RoutedEventArgs e)
        {
            PasswordChanged = true;
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;
            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }

        private int FillInChecker()
        {
            if (NameTextBox.Text == "")
            {
                MessageBox.Show("No name was given");
                return -1;
            }

            if (SurnameTextBox.Text == "")
            {
                MessageBox.Show("No surname was given");
                return -1;
            }

            if (LoginTextBox.Text == "")
            {
                MessageBox.Show("No login was given");
                return -1;
            }

            if (session.EditID == 0 && PasswordTextBox.Password == "")
            {
                MessageBox.Show("No password was given");
                return -1;
            }

            if (CountryComboBox.Text == "")
            {
                MessageBox.Show("No country was given");
                return -1;
            }

            if (ZipcodeTextBox.Text == "")
            {
                MessageBox.Show("No zipcode was given");
                return -1;
            }

            if (AddressTextBox.Text == "")
            {
                MessageBox.Show("No address was given");
                return -1;
            }

            if (PhonenumberTextBox.Text == "")
            {
                MessageBox.Show("No phone number was given");
                return -1;
            }

            if (!Regex.Match(PhonenumberTextBox.Text, @"^([0-9]{9})$").Success)
            {
                MessageBox.Show("Invalid phone number");
                return -1;
            }

            if (EmailBox.Text == "")
            {
                MessageBox.Show("No email address was given");
                return -1;
            }

            if (!new EmailAddressAttribute().IsValid(EmailBox.Text))
            {
                MessageBox.Show("Invalid email address");
                return -1;
            }

            return 0;
        }
    }
}
