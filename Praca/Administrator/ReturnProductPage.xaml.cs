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
    /// Logika interakcji dla klasy ReturnProductPage.xaml
    /// </summary>
    public partial class ReturnProductPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public ReturnProductPage()
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

            RefreshList(sqlConnection);

            connection.CloseConnection(sqlConnection);
        }

        private void RefreshList(SqlConnection sqlConnection)
        {
            if (RentedProductsList.HasItems)
            {
                RentedProductsList.Items.Clear();
            }

            DataTable dt = new DataTable();
            SqlDataAdapter da;

            if (CategoryList.SelectedItem.ToString() == "Hardware")
            {
                da = new SqlDataAdapter("SELECT U.user_id, U.name, U.surname, P.name as product, R.status FROM Rentals as R JOIN Products as P ON P.product_id = R.product_id JOIN Users as U ON U.user_id = R.user_id WHERE R.status = 'Ongoing' AND P.category = 'Hardware' ORDER BY U.user_id", sqlConnection);
            }
            else
            {
                da = new SqlDataAdapter("SELECT U.user_id, U.name, U.surname, P.name as product, R.status FROM Rentals as R JOIN Products as P ON P.product_id = R.product_id JOIN Users as U ON U.user_id = R.user_id WHERE R.status = 'Ongoing' AND P.category = 'Software' ORDER BY U.user_id", sqlConnection);
            }

            da.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                RentedProductsList.Items.Add(new ListBoxItem { Content = dr["user_id"].ToString() + " " + dr["name"].ToString() + " " + dr["surname"].ToString() + " " + dr["product"] });
            }
        }

        private void ReturnProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (RentedProductsList.SelectedItem == null)
            {
                MessageBox.Show("No product was selected");
            }
            else
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);

                SqlCommand command = new SqlCommand();
                command.Connection = sqlConnection;

                int ProductID = 0, RentID = 0, CopyID = 0;
                int SelectedProductIndex = RentedProductsList.SelectedIndex + 1;

                if (CategoryList.SelectedItem.ToString() == "Hardware")
                {
                    command.CommandText = "with Records AS(select row_number() over(order by U.user_id) as 'row', U.user_id, P.product_id, R.rental_id, R.product_copy_id FROM Rentals as R JOIN Products as P ON P.product_id = R.product_id JOIN Users as U ON U.user_id = R.user_id WHERE R.status = 'Ongoing' AND P.category = 'Hardware') select* from records where row = @ProductRow;";
                    command.Parameters.AddWithValue("@ProductRow", SelectedProductIndex);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ProductID = (int)reader["product_id"];
                            RentID = (int)reader["rental_id"];
                            CopyID = (int)reader["product_copy_id"];
                        }
                    }

                    reader.Close();
                }
                else
                {
                    command.CommandText = "with Records AS(select row_number() over(order by U.user_id) as 'row', U.user_id, P.product_id, R.rental_id, R.product_copy_id FROM Rentals as R JOIN Products as P ON P.product_id = R.product_id JOIN Users as U ON U.user_id = R.user_id WHERE R.status = 'Ongoing' AND P.category = 'Software') select* from records where row = @ProductRow;";
                    command.Parameters.AddWithValue("@ProductRow", SelectedProductIndex);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ProductID = (int)reader["product_id"];
                            RentID = (int)reader["rental_id"];
                            CopyID = (int)reader["product_copy_id"];
                        }
                    }

                    reader.Close();
                }

                command.CommandText = "UPDATE Product_copies SET status = 'Ready' WHERE product_copy_id = @UpdateProductCopyID";
                command.Parameters.AddWithValue("@UpdateProductCopyID", CopyID);

                command.ExecuteNonQuery();

                command.CommandText = "UPDATE Products SET copies_available = copies_available + 1 WHERE product_id = @ProductID";
                command.Parameters.AddWithValue("@ProductID", ProductID);

                command.ExecuteNonQuery();

                command.CommandText = "UPDATE Rentals SET status = 'Finished', return_date = GETDATE() WHERE rental_id = @RentalID";
                command.Parameters.AddWithValue("@RentalID", RentID);

                command.ExecuteNonQuery();

                MessageBox.Show("Product returned");
                RefreshList(sqlConnection);

                connection.CloseConnection(sqlConnection);
            }
        }
    }
}
