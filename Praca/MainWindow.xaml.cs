using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Cryptography;


namespace Praca
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();
        private Hashing hashing = new Hashing();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            bool Admin = false;
            bool User = false;
            int CredID = 0;
            

            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            try
            {
                string HashedPass = hashing.Sha256Hashing(PasswordTextBox.Password.ToString());



                SqlCommand command = new SqlCommand();

                command.Connection = sqlConnection;
                command.CommandText = "SELECT credential_id FROM Credentials WHERE login = @UserLogin AND password = @UserPassword;";
                command.Parameters.AddWithValue("@UserLogin", LoginTextBox.Text);
                command.Parameters.AddWithValue("@UserPassword", HashedPass);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        CredID = (int)reader["credential_id"];
                    }
                    User = true;
                }
                else
                {
                    MessageBox.Show("Login lub haslo błędne");
                    PasswordTextBox.Clear();
                }
                reader.Close();

                command.CommandText = "SELECT user_id FROM Users WHERE credential_id = @Password";
                command.Parameters.AddWithValue("@Password", CredID);

                reader = command.ExecuteReader();

                if(reader.HasRows)
                {
                    reader.Read();
                    session.UserID = (int)reader["user_id"];
                }
                reader.Close();

                command.CommandText = "SELECT admin_id FROM Administrators WHERE admin_id = @ID";
                command.Parameters.AddWithValue("@ID", session.UserID);

                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    Admin = true;
                }
                reader.Close();


                if (Admin && User)
                {
                    Admin = false;
                    User = false;
                    AdministratorWindow administratorWindow = new AdministratorWindow();
                    administratorWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    administratorWindow.Show();
                    this.Close();
                }
                else if(User)
                {
                    User = false;
                    UserWindow userWindow = new UserWindow();
                    userWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    userWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            finally
            {
                connection.CloseConnection(sqlConnection);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
