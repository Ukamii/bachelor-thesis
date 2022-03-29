using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Praca
{
    /// <summary>
    /// Logika interakcji dla klasy AdministratorWindow.xaml
    /// </summary>
    public partial class AdministratorWindow : Window
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public AdministratorWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            SqlCommand command = new SqlCommand("SELECT name, surname FROM Users WHERE user_id = @userID", sqlConnection);
            command.Parameters.AddWithValue("@userID", session.UserID);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                NameLabel.Content = (string)reader[0] + (" ") + (string)reader[1];
            }

            reader.Close();

            SendEmail(sqlConnection);

            connection.CloseConnection(sqlConnection);
        }

        private void SendEmail(SqlConnection sqlConnection)
        {
            string emailTo = "";
            string adminMail = "";
            string adminPass = "";


            List<int> users = new List<int>();
            SqlCommand command = new SqlCommand("SELECT user_id FROM Rentals WHERE return_date = Convert(date,DATEADD(week, 1, GETDATE())) AND status = 'Ongoing' AND warned = 'No'", sqlConnection);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while(reader.Read())
                {
                    users.Add((int)reader["user_id"]);
                }
            }
            reader.Close();

            command.CommandText = "SELECT E.email_username, ED.domain_name FROM Users as U JOIN Contacts as C ON C.contact_id = U.contact_id JOIN Emails as E ON E.email_id = C.email_id JOIN Email_domains as ED ON ED.email_domain_id = E.email_domain_id WHERE U.user_id = @ID";
            command.Parameters.AddWithValue("@ID", session.UserID);
            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                adminMail += reader["email_username"];
                adminMail += reader["domain_name"];
            }
            reader.Close();

            command.CommandText = "SELECT mail FROM Administrators WHERE admin_id = @AdminID";
            command.Parameters.AddWithValue("@AdminID", session.UserID);
            reader = command.ExecuteReader();

            if(reader.HasRows)
            {
                reader.Read();
                adminPass = reader["mail"].ToString();
            }
            reader.Close();

            var message = new MailMessage();
            message.From = new MailAddress(adminMail, "Administrator");
            message.Subject = "Return date of rented product";
            message.Body = "Your rented product(s) have their return date in next week. If nobody is waiting for it, you can extend rental time.";

            var smtp = new SmtpClient("smtp.gmail.com");
            smtp.UseDefaultCredentials = false;
            //smtp.Credentials = new NetworkCredential(adminMail, adminPass);
            smtp.EnableSsl = true;
            smtp.Port = 587;

            foreach (int ID in users)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT E.email_username, ED.domain_name FROM Users as U JOIN Contacts as C ON C.contact_id = U.contact_id JOIN Emails as E ON E.email_id = C.email_id JOIN Email_domains as ED ON ED.email_domain_id = E.email_domain_id WHERE U.user_id = @ID";
                command.Parameters.AddWithValue("@ID", ID);
                reader = command.ExecuteReader();

                if(reader.HasRows)
                {
                    reader.Read();
                    emailTo += reader["email_username"];
                    emailTo += reader["domain_name"];
                }
                reader.Close();

                //message.To.Add(new MailAddress(emailTo));
                smtp.Send(message);

                command.CommandText = "UPDATE Rentals SET warned = 'Yes' WHERE user_id = @User";
                command.Parameters.AddWithValue("@User", ID);

                command.ExecuteNonQuery();
                emailTo = "";
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new AddUserPage();
        }

        private void AddManufacturer_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new AddManufacturerPage();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new AddExistingOrNewProductPage();
        }

        private void RentProduct_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new RentProductPage();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            Praca.Properties.Settings.Default.Save();
            MainWindow mainWindow = new MainWindow();
            session.UserID = 0;
            session.clearPreviousWindow();
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mainWindow.Show();
            this.Close();
        }

        private void RentalRequestButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new RequestedRentalsPage();
        }

        private void UserList_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new UsersPage();
        }

        private void ManufacturerList_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new ManufacturersPage();
        }

        private void ProductList_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new ProductsPage();
        }

        private void ReturnProductsButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new ReturnProductPage();
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new StatisticsPage();
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = new AdministratorWindow();
            session.addPreviousWindow(administratorWindow);

            ShowingFrame.Content = new EmailNotificationPage();
        }
    }
}
