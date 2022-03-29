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
    /// Logika interakcji dla klasy UserRentedItemInfoPage.xaml
    /// </summary>
    public partial class UserRentedProductInfoPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public UserRentedProductInfoPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            int ProductID = 0;
            DateWarningImage.Visibility = Visibility.Collapsed;
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            SqlCommand command = new SqlCommand();
            command.Connection = sqlConnection;
            SqlDataReader reader;

            ProductID = LoadProductId(command);
            command.Parameters.Clear();

            command.CommandText = "SELECT P.name, P.category, M.name, R.rent_date, R.return_date FROM Products as P JOIN Manufacturers as M ON P.manufacturer_id = M.manufacturer_id JOIN Rentals as R ON P.product_id = R.product_id WHERE P.product_id = @product_id AND R.product_copy_id = @CopyID";
            command.Parameters.AddWithValue("@product_id", ProductID);
            command.Parameters.AddWithValue("@CopyID", session.EditID);

            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                NameOfProductText.Content = (string)reader[0];
                CategoryOfProductText.Content = (string)reader[1];
                NameOfManufacturerText.Content = (string)reader[2];
                RentalDateText.Content = (DateTime?)reader[3];
                ReturnDateText.Content = (DateTime?)reader[4];
            }

            reader.Close();

            command.CommandText = "SELECT * FROM Rentals WHERE user_id = @userID AND return_date <= DATEADD(week, 1, GETDATE()) AND status != 'Finished' AND product_copy_id = @ProductID";
            command.Parameters.AddWithValue("@userID", session.UserID);
            command.Parameters.AddWithValue("@ProductID", session.EditID);
            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                DateWarningImage.Visibility = Visibility.Visible;
            }
            reader.Close();

            connection.CloseConnection(sqlConnection);
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            int ProductID = 0;
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);
            SqlCommand command = new SqlCommand();
            command.Connection = sqlConnection;
            SqlDataReader reader;

            ProductID = LoadProductId(command);

            command.CommandText = "SELECT requests_waiting FROM Products WHERE product_id =  @product_id";
            command.Parameters.AddWithValue("@product_id", ProductID);

            reader = command.ExecuteReader();

            int Requests = 0;

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    Requests = (int)reader[0];
                }
            }
            reader.Close();
            connection.CloseConnection(sqlConnection);

            if (Requests > 0)
            {
                MessageBox.Show("Somebody is waiting for this product so you can't extend your rental time");
            }
            else
            {
                UserRentedProductInfoPage userRentedItemInfo = new UserRentedProductInfoPage();
                session.addPreviousWindow(userRentedItemInfo);

                UserWindow userWindow = (UserWindow)Window.GetWindow(this);
                userWindow.ShowingFrame.Content = new UserRentedProductTimeExtendPage();
            }
            
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;
            UserWindow userWindow = (UserWindow)Window.GetWindow(this);
            userWindow.ShowingFrame.Content = session.getPreviousWindow();
        }

        private int LoadProductId(SqlCommand command)
        {
            int ProductID = -1;
            command.CommandText = "SELECT product_id FROM Product_copies WHERE product_copy_id = @CopyID";
            command.Parameters.AddWithValue("@CopyID", session.EditID);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                ProductID = (int)reader[0];
            }
            reader.Close();

            return ProductID;
        }
    }
}
