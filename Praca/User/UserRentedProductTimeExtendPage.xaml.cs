using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Praca
{
    /// <summary>
    /// Logika interakcji dla klasy UserRentedItemTimeExtendPage.xaml
    /// </summary>
    public partial class UserRentedProductTimeExtendPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();
        private DateTime returndate = new DateTime();
        private int PickedDate = 0;
        private bool FirstTime = true;

        public UserRentedProductTimeExtendPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            var CategoryOptions = new List<string> { "Week", "2 weeks", "3 weeks", "Month", "Other" };
            DateExtend.ItemsSource = CategoryOptions;
            DateExtend.SelectedIndex = 3;

            OtherDate.Visibility = Visibility.Collapsed;
            OtherDate.DisplayDateStart = DateTime.Now;
            FirstTime = false;
        }

        private void DateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            SqlCommand sqlCommand = new SqlCommand("SELECT return_date FROM Rentals WHERE product_copy_id = @CopyID", sqlConnection);
            sqlCommand.Parameters.AddWithValue("@CopyID", session.EditID);

            SqlDataReader reader = sqlCommand.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    returndate = (DateTime)reader[0];
                }
            }

            reader.Close();

            switch (DateExtend.SelectedIndex)
            {
                case 0:
                    returndate = returndate.AddDays(7);
                    PickedDate = 1;
                    OtherDate.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    returndate = returndate.AddDays(2 * 7);
                    PickedDate = 1;
                    OtherDate.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    returndate = returndate.AddDays(3 * 7);
                    PickedDate = 1;
                    OtherDate.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    returndate = returndate.AddMonths(1);
                    PickedDate = 1;
                    OtherDate.Visibility = Visibility.Collapsed;
                    break;
                case 4:
                    OtherDate.Visibility = Visibility.Visible;
                    OtherDate.DisplayDateStart = DateTime.Today;
                    OtherDate.DisplayDateEnd = DateTime.Today.AddMonths(1);
                    break;
            }
        }
        private void DateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(FirstTime != true)
            {
                PickedDate = 1;
                DateTime dateTime = new DateTime();
                dateTime = returndate;
                returndate = (DateTime)OtherDate.SelectedDate;

                if (returndate < dateTime)
                {
                    MessageBox.Show("You've chosen date that is earlier than your current return date", "Earlier date", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PickedDate == 0)
            {
                MessageBox.Show("You didn't choose any date");
            }
            else
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);

                SqlCommand command = new SqlCommand("UPDATE Rentals SET return_date = @Date WHERE product_copy_id = @CopyID AND status = 'Ongoing'", sqlConnection);
                command.Parameters.AddWithValue("@Date", returndate);
                command.Parameters.AddWithValue("@CopyID", session.EditID);
                command.ExecuteNonQuery();

                MessageBox.Show("Return date changed");
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            UserWindow userWindow = (UserWindow)Window.GetWindow(this);
            userWindow.ShowingFrame.Content = session.getPreviousWindow();
        }
    }
}
