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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Praca
{
    /// <summary>
    /// Logika interakcji dla klasy UserRentedProductsPage.xaml
    /// </summary>
    public partial class UserRentedProductsPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public UserRentedProductsPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            var CategoryOptions = new List<string> { "Hardware", "Software" };
            CategoryList.ItemsSource = CategoryOptions;
            CategoryList.SelectedIndex = 0;
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            if (RentedList.HasItems)
            {
                RentedList.Items.Clear();
            }

            DataTable dt = new DataTable();
            SqlDataAdapter da;
            SqlCommand command = new SqlCommand();
            command.Connection = sqlConnection;

            if (CategoryList.SelectedItem.ToString() == "Hardware")
            {
                command.CommandText = "SELECT p.name, r.return_date FROM Products as p JOIN Rentals r ON r.product_id = p.product_id WHERE r.user_id = @Login AND r.status = 'Ongoing' AND p.category = 'Hardware' ORDER BY p.product_id; ";
            }
            else
            {
                command.CommandText = "SELECT p.name, r.return_date FROM Products as p JOIN Rentals r ON r.product_id = p.product_id WHERE r.user_id = @Login AND r.status = 'Ongoing' AND p.category = 'Software' ORDER BY p.product_id; ";
            }
            command.Parameters.AddWithValue("@Login", session.UserID);

            da = new SqlDataAdapter(command);
            da.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                DateTime returnDate = new DateTime();
                returnDate = (DateTime)dr["return_date"];
                DateTime dateTime = DateTime.Today;
                dateTime = dateTime.AddDays(7);

                if (returnDate <= dateTime)
                {
                    RentedList.Items.Add(new ListBoxItem { Content = dr["name"].ToString(), Background = Brushes.Red });
                }
                else
                {
                    RentedList.Items.Add(new ListBoxItem { Content = dr["name"].ToString() });
                }
            }

            connection.CloseConnection(sqlConnection);
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {

            if (RentedList.SelectedItem == null)
            {
                MessageBox.Show("No product was selected");
            }
            else
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);
                SqlCommand command = new SqlCommand();
                command.Connection = sqlConnection;
                if (CategoryList.SelectedItem.ToString() == "Hardware")
                {
                    command.CommandText = "with Records AS(select row_number() over(order by p.product_id) as 'row', p.product_id, r.product_copy_id FROM Products as p JOIN Rentals r ON r.product_id = p.product_id WHERE r.user_id = @userID AND r.status = 'Ongoing' AND p.category = 'Hardware') select* from records where row = @product_id;";
                }
                else
                {
                    command.CommandText = "with Records AS(select row_number() over(order by p.product_id) as 'row', p.product_id, r.product_copy_id FROM Products as p JOIN Rentals r ON r.product_id = p.product_id WHERE r.user_id = @userID AND r.status = 'Ongoing' AND p.category = 'Software') select* from records where row = @product_id;";

                }
                command.Parameters.AddWithValue("@userID", session.UserID);
                command.Parameters.AddWithValue("@product_id", RentedList.SelectedIndex + 1);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        session.EditID = (int)reader["product_copy_id"];
                    }
                }

                reader.Close();
                connection.CloseConnection(sqlConnection);

                UserRentedProductsPage userRentedList = new UserRentedProductsPage();
                session.addPreviousWindow(userRentedList);

                UserWindow userWindow = (UserWindow)Window.GetWindow(this);
                userWindow.ShowingFrame.Content = new UserRentedProductInfoPage();
            }
        }
    }
}
