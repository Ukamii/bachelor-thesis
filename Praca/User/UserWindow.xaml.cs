using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
    /// Logika interakcji dla klasy UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public UserWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            NameLoad(sqlConnection);
            DateWarningLoad(sqlConnection);

            connection.CloseConnection(sqlConnection);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            session.UserID = 0;
            session.clearPreviousWindow();
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mainWindow.Show();
            this.Close();
        }

        private void RentedListButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            UserWindow userWindow = new UserWindow();
            session.addPreviousWindow(userWindow);

            ShowingFrame.Content = new UserRentedProductsPage();
        }

        private void AskForRentButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            UserWindow userWindow = new UserWindow();
            session.addPreviousWindow(userWindow);

            ShowingFrame.Content = new UserRentalRequestPage();
        }

        private void SentRequests_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            UserWindow userWindow = new UserWindow();
            session.addPreviousWindow(userWindow);

            ShowingFrame.Content = new UserSentRequestPage();
        }

        private void NameLoad(SqlConnection sqlConnection)
        {
            SqlCommand sqlCommand = new SqlCommand("SELECT name, surname FROM Users WHERE user_id = @userID", sqlConnection);
            sqlCommand.Parameters.AddWithValue("@userID", session.UserID);
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                NameLabel.Content = (string)reader[0] + (" ") + (string)reader[1];
            }
            reader.Close();
        }

        private void DateWarningLoad(SqlConnection sqlConnection)
        {
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Rentals WHERE user_id = @userID AND return_date <= DATEADD(week, 1, GETDATE()) AND status != 'Finished'", sqlConnection);
            sqlCommand.Parameters.AddWithValue("@userID", session.UserID);
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                RentedListButton.Background = Brushes.Red;
            }
            reader.Close();
        }


    }
}
