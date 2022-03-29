using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Logika interakcji dla klasy UserRentalRequestPage.xaml
    /// </summary>
    /// 

    public partial class UserRentalRequestPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        private DateTime returndaterequest = DateTime.Now;
        private int PickedDate = 0;

        public UserRentalRequestPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            var CategoryOptions = new List<string> { "Hardware", "Software" };
            CategoryList.ItemsSource = CategoryOptions;
            CategoryList.SelectedIndex = 0;

            var DateOptions = new List<string> { "Week", "2 weeks", "3 weeks", "Month", "3 Months", "Half a year", "Year", "Other" };
            ReturnDatePick.ItemsSource = DateOptions;
            ReturnDatePick.SelectedIndex = 3;

            ReturnDateOther.Visibility = Visibility.Collapsed;
            //ReturnDateOther.SelectedDate = DateTime.Now;
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            RefreshList(sqlConnection);

            connection.CloseConnection(sqlConnection);
        }


        private void RentButton_Click(object sender, RoutedEventArgs e)
        {

            if (ProductList.SelectedItem == null)
            {
                MessageBox.Show("No product was selected");
            }
            else if (PickedDate == 0)
            {
                MessageBox.Show("No return date was set");
            }
            else
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);

                SqlCommand command = new SqlCommand();
                command.Connection = sqlConnection;

                int Requests = 0;
                int available = 0;

                string SelectedProduct = ProductList.SelectedItem.ToString();
                int found = SelectedProduct.IndexOf("(");
                SelectedProduct = SelectedProduct.Substring(found);
                SelectedProduct = SelectedProduct.Trim('(', ')');

                int ProductID = Int32.Parse(SelectedProduct);

                command.CommandText = "SELECT requests_waiting, copies_available FROM Products WHERE product_id = @ProductID";
                command.Parameters.AddWithValue("@ProductID", ProductID);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Requests = (int)reader[0];
                        available = (int)reader[1];
                    }
                }

                reader.Close();

                command.Parameters.Clear();

                if (Requests > 0 || available == 0)
                {
                    MessageBoxResult result = MessageBox.Show("This product is currently in use, do you want to sign up in queue for this product?",
                                                                "Product in use",
                                                                MessageBoxButton.YesNo,
                                                                MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        SentRequest(command, ProductID);
                    }
                }
                else
                {
                    SentRequest(command, ProductID);
                }

                RefreshList(sqlConnection);
                connection.CloseConnection(sqlConnection);
            }
        }

        private void RefreshList(SqlConnection sqlConnection)
        {
            ReturnDatePick.SelectedIndex = 3;
            returndaterequest = DateTime.Now;

            List<string> filter = new List<string>();
            DataTable dt = new DataTable();
            SqlDataAdapter da;

            if (CategoryList.SelectedItem.ToString() == "Hardware")
            {
                da = new SqlDataAdapter("SELECT name, P.product_id, P.requests_waiting FROM Products as P WHERE P.category = 'Hardware' AND P.product_id NOT IN (SELECT P.product_id FROM Products as P JOIN Requests as R ON P.product_id = R.product_id WHERE R.user_id = @userID AND P.category = 'Hardware' AND R.status = 'Sent')", sqlConnection); 
            }
            else
            {
                da = new SqlDataAdapter("SELECT name, P.product_id, P.requests_waiting FROM Products as P WHERE P.category = 'Software' AND P.product_id NOT IN (SELECT P.product_id FROM Products as P JOIN Requests as R ON P.product_id = R.product_id WHERE R.user_id = @userID AND P.category = 'Software' AND R.status = 'Sent')", sqlConnection);
            }
            da.SelectCommand.Parameters.AddWithValue("@userID", session.UserID);
            da.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                if ((int)dr["requests_waiting"] > 0)
                {
                    //ProductList.Items.Add(new ListBoxItem { Content = dr["name"].ToString() + " (" + dr["product_id"].ToString() + ")", Background = Brushes.Gray });
                    filter.Add(dr["name"].ToString() + " (" + dr["product_id"].ToString() + ")");
                }
                else
                {
                    //ProductList.Items.Add(new ListBoxItem { Content = dr["name"].ToString() + " (" + dr["product_id"].ToString() + ")"});
                    filter.Add(dr["name"].ToString() + " (" + dr["product_id"].ToString() + ")");
                }
            }

            ProductList.ItemsSource = filter;

            ICollectionView view = CollectionViewSource.GetDefaultView(filter);

            new TextSearchFilter(view, SearchProduct);

        }

        private void DateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ReturnDatePick.SelectedIndex)
            {
                case 0:
                    returndaterequest = returndaterequest.AddDays(7);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    returndaterequest = returndaterequest.AddDays(2 * 7);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    returndaterequest = returndaterequest.AddDays(3 * 7);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    returndaterequest = returndaterequest.AddMonths(1);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 4:
                    returndaterequest = returndaterequest.AddMonths(3);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 5:
                    returndaterequest = returndaterequest.AddMonths(6);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 6:
                    returndaterequest = returndaterequest.AddYears(1);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 7:
                    ReturnDateOther.Visibility = Visibility.Visible;
                    ReturnDateOther.DisplayDateStart = DateTime.Today;
                    ReturnDateOther.DisplayDateEnd = DateTime.Today.AddYears(1);
                    break;
            }
        }

        private void OtherDateChanged(object sender, SelectionChangedEventArgs e)
        {
            PickedDate = 1;
            returndaterequest = (DateTime)ReturnDateOther.SelectedDate;
        }

        private void SearchProduct_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchProduct.Text = "";
        }

        private void SentRequest(SqlCommand command, int ProductID)
        {
            command.CommandText = "INSERT INTO Requests(user_id, product_id, request_date, request_return_date, status) VALUES(@UserID, @ProductID, GETDATE(), @ReturnDate, 'Sent');";
            command.Parameters.AddWithValue("@UserID", session.UserID);
            command.Parameters.AddWithValue("@ProductID", ProductID);
            command.Parameters.AddWithValue("@ReturnDate", returndaterequest);


            command.ExecuteNonQuery();

            command.CommandText = "UPDATE Products SET times_requested = times_requested + 1, requests_waiting = requests_waiting + 1 WHERE product_id = @RequestProductID";
            command.Parameters.AddWithValue("@RequestProductID", ProductID);

            command.ExecuteNonQuery();

            MessageBox.Show("Request for product sent");
        }
    }
}
