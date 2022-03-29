using System;
using System.Collections.Generic;
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
    /// Logika interakcji dla klasy RentProductPage.xaml
    /// </summary>
    public partial class RentProductPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        private DateTime returnDate = DateTime.Now;
        private int PickedDate = 0;

        public RentProductPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            List<string> filter = new List<string>();

            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter("SELECT user_id, name, surname FROM Users ORDER BY surname", sqlConnection);
            da.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                filter.Add(dr["surname"].ToString() + " " + dr["name"].ToString() + " (" + dr["user_id"].ToString() + ")");
            }

            connection.CloseConnection(sqlConnection);

            UserList.ItemsSource = filter;

            ICollectionView view = CollectionViewSource.GetDefaultView(filter);

            new TextSearchFilter(view, SearchUsers);

            var CategoryOptions = new List<string> { "Hardware", "Software" };
            CategoryList.ItemsSource = CategoryOptions;
            CategoryList.SelectedIndex = 0;

            var DateOptions = new List<string> { "Week", "2 weeks", "3 weeks", "Month", "3 Months", "Half a year", "Year", "Other" };
            ReturnDatePick.ItemsSource = DateOptions;
            ReturnDatePick.SelectedIndex = 3;

            ReturnDateOther.Visibility = Visibility.Collapsed;
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
            ReturnDatePick.SelectedIndex = 3;
            returnDate = DateTime.Now;

            List<string> filter = new List<string>();
            DataTable dt = new DataTable();
            SqlDataAdapter da;

            if (CategoryList.SelectedItem.ToString() == "Hardware")
            {
                da = new SqlDataAdapter("SELECT name, product_id FROM Products WHERE category = 'Hardware' AND copies_available > 0", sqlConnection);
            }
            else
            {
                da = new SqlDataAdapter("SELECT name, product_id FROM Products WHERE category = 'Software' AND copies_available > 0", sqlConnection);
            }

            da.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                filter.Add(dr["name"].ToString() + " (" + dr["product_id"].ToString() + ")");
            }

            ProductList.ItemsSource = filter;

            ICollectionView view = CollectionViewSource.GetDefaultView(filter);

            new TextSearchFilter(view, SearchProduct);
        }

        private void RentButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            int errorCode = FillInChecker();

            if(errorCode == 0)
            {
                RentProduct(sqlConnection);
            }

            connection.CloseConnection(sqlConnection);
        }

        private void DateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ReturnDatePick.SelectedIndex)
            {
                case 0:
                    returnDate = returnDate.AddDays(7);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    returnDate = returnDate.AddDays(2 * 7);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    returnDate = returnDate.AddDays(3 * 7);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    returnDate = returnDate.AddMonths(1);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 4:
                    returnDate = returnDate.AddMonths(3);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 5:
                    returnDate = returnDate.AddMonths(6);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 6:
                    returnDate = returnDate.AddYears(1);
                    PickedDate = 1;
                    ReturnDateOther.Visibility = Visibility.Collapsed;
                    break;
                case 7:
                    ReturnDateOther.Visibility = Visibility.Visible;
                    ReturnDateOther.DisplayDateStart = DateTime.Today;
                    break;
            }
        }

        private void OtherDateChanged(object sender, SelectionChangedEventArgs e)
        {
            PickedDate = 1;
            returnDate = (DateTime)ReturnDateOther.SelectedDate;
        }

        private void SearchUsers_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchUsers.Text = "";
        }

        private void SearchProduct_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchProduct.Text = "";
        }

        private int FillInChecker()
        {
            if (UserList.SelectedItem == null)
            {
                MessageBox.Show("No user was selected");
                return -1;
            }
            if (ProductList.SelectedItem == null)
            {
                MessageBox.Show("No product was selected");
                return -1;
            }
            if (PickedDate == 0)
            {
                MessageBox.Show("No return date was set");
                return -1;
            }

            return 0;
        }

        private void RentProduct(SqlConnection sqlConnection)
        {
            string SelectedUser = UserList.SelectedItem.ToString();
            int found = SelectedUser.IndexOf("(");
            SelectedUser = SelectedUser.Substring(found);
            SelectedUser = SelectedUser.Trim('(', ')');

            int UserID = Int32.Parse(SelectedUser);

            string SelectedProduct = ProductList.SelectedItem.ToString();
            found = SelectedProduct.IndexOf("(");
            SelectedProduct = SelectedProduct.Substring(found);
            SelectedProduct = SelectedProduct.Trim('(', ')');

            int ProductID = Int32.Parse(SelectedProduct);
            int CopyID = 0;

            SqlCommand command = new SqlCommand("SELECT product_copy_id FROM Product_copies WHERE product_id = @ID AND status = 'Ready'", sqlConnection);
            command.Parameters.AddWithValue("@ID", ProductID);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                CopyID = (int)reader[0];
            }

            reader.Close();
            command.Parameters.Clear();

            command.CommandText = "INSERT INTO Rentals(product_id, user_id, rent_date, return_date, status, product_copy_id, warned) VALUES(@ProductID,@UserID,GETDATE(), @ReturnDate, 'Ongoing', @CopyID, 'No');";
            command.Parameters.AddWithValue("@ProductID", ProductID);
            command.Parameters.AddWithValue("@UserID", UserID);
            command.Parameters.AddWithValue("@ReturnDate", returnDate);
            command.Parameters.AddWithValue("@CopyID", CopyID);

            command.ExecuteNonQuery();


            command.CommandText = "UPDATE Products SET times_rented = times_rented + 1, copies_available = copies_available - 1 WHERE product_id = @ID;";
            command.Parameters.AddWithValue("ID", ProductID);

            command.ExecuteNonQuery();

            command.CommandText = "UPDATE Product_copies SET status = 'In_use' WHERE product_copy_id = @CopyIDUse";
            command.Parameters.AddWithValue("@CopyIDUse", CopyID);

            command.ExecuteNonQuery();


            MessageBox.Show("Product successfully rented");

            RefreshList(sqlConnection);
        }
    }
}
