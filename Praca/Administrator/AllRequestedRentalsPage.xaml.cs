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
    /// Logika interakcji dla klasy AllRequestedRentalsPage.xaml
    /// </summary>
    public partial class AllRequestedRentalsPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        private DateTime returnDate = new DateTime();

        public AllRequestedRentalsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
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

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {

            if (RequestList.SelectedItem == null)
            {
                MessageBox.Show("No request was selected");
            }
            else
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);

                int SelectedRentalRequest = RequestList.SelectedIndex + 1;

                int RequestID = 0;
                int UserID = 0;
                int ProductID = 0;
                int CopiesAvailable = 0;
                int CopyID = 0;

                SqlCommand command = new SqlCommand();
                command.Connection = sqlConnection;
                SqlDataReader reader;

                if (CategoryList.SelectedItem.ToString() == "Hardware")
                {
                    command.CommandText = "with Records AS(select row_number() over(order by request_id) as 'row', R.request_id, R.user_id, R.product_id, R.request_return_date FROM Requests as R JOIN Products as P ON P.product_id = R.product_id WHERE P.category = 'Hardware' AND R.status = 'Sent') select* from records where row = @RentalRow";

                    command.Parameters.AddWithValue("@RentalRow", SelectedRentalRequest);

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            RequestID = (int)reader[1];
                            UserID = (int)reader[2];
                            ProductID = (int)reader[3];
                            returnDate = (DateTime)reader[4];
                        }
                    }

                    reader.Close();
                }
                else
                {
                    command.CommandText = "with Records AS(select row_number() over(order by request_id) as 'row', R.request_id, R.user_id, R.product_id, R.request_return_date FROM Requests as R JOIN Products as P ON P.product_id = R.product_id WHERE P.category = 'Software' AND R.status = 'Sent') select* from records where row = @RentalRow";

                    command.Parameters.AddWithValue("@RentalRow", SelectedRentalRequest);

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            RequestID = (int)reader[1];
                            UserID = (int)reader[2];
                            ProductID = (int)reader[3];
                            returnDate = (DateTime)reader[4];
                        }
                    }

                    reader.Close();
                }

                command.CommandText = "SELECT copies_available FROM Products WHERE product_id = @ID";
                command.Parameters.AddWithValue("@ID", ProductID);
                reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        CopiesAvailable = (int)reader[0];
                    }
                }

                reader.Close();
                command.Parameters.Clear();
                if(CopiesAvailable == 0)
                {
                    MessageBox.Show("You can't rent this product because there's no copies left");
                }
                else
                {
                    command.CommandText = "SELECT product_copy_id FROM Product_copies WHERE product_id = @ID AND status = 'Ready'";
                    command.Parameters.AddWithValue("@ID", ProductID);
                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        CopyID = (int)reader[0];
                    }
                    reader.Close();
                    command.Parameters.Clear();

                    command.CommandText = "INSERT INTO Rentals(product_id, user_id, rent_date, return_date, status, product_copy_id, warned) VALUES(@ProductID, @UserID, GETDATE(), @ReturnDate, 'Ongoing', @CopyID, 'No')";
                    command.Parameters.AddWithValue("@ProductID", ProductID);
                    command.Parameters.AddWithValue("@UserID", UserID);
                    command.Parameters.AddWithValue("@ReturnDate", returnDate);
                    command.Parameters.AddWithValue("@CopyID", CopyID);

                    command.ExecuteNonQuery();

                    command.CommandText = "UPDATE Products SET times_rented = times_rented + 1, requests_waiting = requests_waiting - 1, copies_available = copies_available - 1 WHERE product_id = @ID;";
                    command.Parameters.AddWithValue("@ID", ProductID);

                    command.ExecuteNonQuery();

                    command.CommandText = "UPDATE Product_copies SET status = 'In_use' WHERE product_copy_id = @ProductCopyID";
                    command.Parameters.AddWithValue("@ProductCopyID", CopyID);

                    command.ExecuteNonQuery();

                    command.CommandText = "UPDATE Requests SET status = 'Accepted', request_check_date = GETDATE() WHERE request_id = @RequestID";
                    command.Parameters.AddWithValue("@RequestID", RequestID);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Request accepted");

                    RefreshList(sqlConnection);
                }
                connection.CloseConnection(sqlConnection);
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {

            if (RequestList.SelectedItem == null)
            {
                MessageBox.Show("No request was selected");
            }
            else
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);

                int SelectedRentalRequest = RequestList.SelectedIndex + 1;

                int RequestID = 0;
                int ProductID = 0;

                SqlCommand command = new SqlCommand();
                command.Connection = sqlConnection;

                if (CategoryList.SelectedItem.ToString() == "Hardware")
                {
                    command.CommandText = "with Records AS(select row_number() over(order by request_id) as 'row', R.request_id, R.product_id FROM Requests as R JOIN Products as P ON P.product_id = R.product_id WHERE P.category = 'Hardware' AND R.status = 'Sent') select* from records where row = @RentalRow";

                    command.Parameters.AddWithValue("@RentalRow", SelectedRentalRequest);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            RequestID = (int)reader[1];
                            ProductID = (int)reader[2];
                        }
                    }

                    reader.Close();
                }
                else
                {
                    command.CommandText = "with Records AS(select row_number() over(order by request_id) as 'row', R.request_id, R.product_id FROM Requests as R JOIN Products as P ON P.product_id = R.product_id WHERE P.category = 'Software' AND R.status = 'Sent') select* from records where row = @RentalRow";

                    command.Parameters.AddWithValue("@RentalRow", SelectedRentalRequest);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            RequestID = (int)reader[1];
                            ProductID = (int)reader[2];
                        }
                    }

                    reader.Close();
                }


                command.CommandText = "UPDATE Products SET requests_waiting = requests_waiting - 1 WHERE product_id = @ID;";
                command.Parameters.AddWithValue("@ID", ProductID);

                command.ExecuteNonQuery();

                command.CommandText = "UPDATE Requests SET status = 'Rejected', request_check_date = GETDATE() WHERE request_id = @RequestID";
                command.Parameters.AddWithValue("@RequestID", RequestID);

                command.ExecuteNonQuery();

                MessageBox.Show("Request rejected");

                RefreshList(sqlConnection);

                connection.CloseConnection(sqlConnection);
            }
        }

        private void RefreshList(SqlConnection sqlConnection)
        {
            if (CategoryList.SelectedItem.ToString() == "Hardware")
            {
                if (RequestList.HasItems)
                {
                    RequestList.Items.Clear();
                }

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter("SELECT U.name, U.surname, P.name as product, R.request_return_date, P.copies_available FROM Requests as R JOIN Users as U ON U.user_id = R.user_id JOIN Products as P ON P.product_id = R.product_id WHERE P.category = 'Hardware' AND R.status = 'Sent' ORDER BY R.request_id", sqlConnection);
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    DateTime returnDate = (DateTime)dr["request_return_date"];
                    string shortReturnDate = returnDate.ToString("d");

                    if ((int)dr["copies_available"] == 0)
                    {
                        RequestList.Items.Add(new ListBoxItem { Content = dr["surname"].ToString() + " " + dr["name"].ToString() + ": " + dr["product"].ToString() + " return date: " + shortReturnDate, Background = Brushes.Gray});
                    }
                    else
                    {
                        RequestList.Items.Add(new ListBoxItem { Content = dr["surname"].ToString() + " " + dr["name"].ToString() + ": " + dr["product"].ToString() + " return date: " + shortReturnDate });
                    }
                    
                }

            }
            else
            {
                if (RequestList.HasItems)
                {
                    RequestList.Items.Clear();
                }

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter("SELECT U.name, U.surname, P.name as product, R.request_return_date, P.copies_available FROM Requests as R JOIN Users as U ON U.user_id = R.user_id JOIN Products as P ON P.product_id = R.product_id WHERE P.category = 'Software' AND R.status = 'Sent' ORDER BY R.request_id", sqlConnection);
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    DateTime returnDate = (DateTime)dr["request_return_date"];
                    string shortReturnDate = returnDate.ToString("d");

                    if ((int)dr["copies_available"] == 0)
                    {
                        RequestList.Items.Add(new ListBoxItem { Content = dr["surname"].ToString() + " " + dr["name"].ToString() + ": " + dr["product"].ToString() + " return date: " + shortReturnDate, Background = Brushes.Gray });
                    }
                    else
                    {
                        RequestList.Items.Add(new ListBoxItem { Content = dr["surname"].ToString() + " " + dr["name"].ToString() + ": " + dr["product"].ToString() + " return date: " + shortReturnDate });
                    }
                }
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }
    }
}
