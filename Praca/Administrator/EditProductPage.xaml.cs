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
    /// Logika interakcji dla klasy EditProductPage.xaml
    /// </summary>
    public partial class EditProductPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public EditProductPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if(session.EditID == 0)
            {
                AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
                administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
            }
            else
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter("SELECT name FROM Manufacturers", sqlConnection);
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    ManufacturerList.Items.Add(dr["name"].ToString());
                }

                var CategoryOptions = new List<string> { "Hardware", "Software" };
                var StatusOptions = new List<string> { "All", "Ready", "In_use", "Malfunction" };
                CategoryList.ItemsSource = CategoryOptions;
                StatusList.ItemsSource = StatusOptions;

                CategoryList.SelectedIndex = 0;
                StatusList.SelectedIndex = 0;

                connection.CloseConnection(sqlConnection);
                connection.OpenConnection(sqlConnection);

                LoadProduct(sqlConnection);

                connection.CloseConnection(sqlConnection);
            }
        }

        private void ChangeProductButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            int errorCode = FillInCheck();

            if(errorCode == 0)
            {
                UpdateProduct(sqlConnection);
            }
            
            connection.CloseConnection(sqlConnection);
        }

        private void LoadProduct(SqlConnection sqlConnection)
        {
            string category = null;

            SqlCommand command = new SqlCommand("SELECT category FROM Products WHERE product_id = @ProductIDLoad", sqlConnection);
            command.Parameters.AddWithValue("@ProductIDLoad", session.EditID);

            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                category = (string)reader[0];
            }
            reader.Close();

            decimal price;

            if (category == "Hardware")
            {
                OperatingSystemLabel.Visibility = Visibility.Collapsed;
                OperatingSystemTextBox.Visibility = Visibility.Collapsed;

                command.CommandText = "SELECT P.name, M.name as manufacturer_name, P.category, P.price FROM Products as P JOIN Manufacturers as M ON P.manufacturer_id = M.manufacturer_id WHERE P.product_id = @ProductID";
                command.Parameters.AddWithValue("@ProductID", session.EditID);

                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    NameTextBox.Text = (string)reader[0];
                    ManufacturerList.SelectedItem = (string)reader[1];
                    CategoryList.SelectedItem = (string)reader[2];
                    price = (Decimal)reader[3];

                    PriceTextBox.Text = price.ToString();
                }

                reader.Close();

            }
            else
            {
                command.CommandText = "SELECT P.name, M.name as manufacturer_name, P.category, P.price, P.operating_system FROM Products as P JOIN Manufacturers as M ON P.manufacturer_id = M.manufacturer_id WHERE P.product_id = @ProductID";
                command.Parameters.AddWithValue("@ProductID", session.EditID);

                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    NameTextBox.Text = (string)reader[0];
                    ManufacturerList.SelectedItem = (string)reader[1];
                    CategoryList.SelectedItem = (string)reader[2];
                    price = (Decimal)reader[3];
                    OperatingSystemTextBox.Text = (string)reader[4];

                    PriceTextBox.Text = price.ToString();
                }
                reader.Close();
            }
        }

        private void UpdateProduct(SqlConnection sqlConnection)
        {
            int ManuID = -1;

            SqlCommand sqlCommand = new SqlCommand("SELECT manufacturer_id FROM Manufacturers WHERE name = @name", sqlConnection);
            sqlCommand.Parameters.AddWithValue("@name", ManufacturerList.SelectedValue.ToString());
            SqlDataReader reader = sqlCommand.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                ManuID = (int)reader[0];
            }
            reader.Close();

            SqlCommand command = sqlConnection.CreateCommand();
            command.Connection = sqlConnection;

            if (CategoryList.SelectedItem.ToString() == "Hardware")
            {
                command.CommandText = "UPDATE Products SET name = @name, manufacturer_id = @manufacturer_id, category = @category, price = @price WHERE product_id = @ProductID";
                command.Parameters.AddWithValue("@name", NameTextBox.Text);
                command.Parameters.AddWithValue("@manufacturer_id", ManuID);
                command.Parameters.AddWithValue("@category", CategoryList.SelectedValue.ToString());
                command.Parameters.AddWithValue("@price", decimal.Parse(PriceTextBox.Text));
                command.Parameters.AddWithValue("@ProductID", session.EditID);
            }
            else
            {
                command.CommandText = "UPDATE Products SET name = @name, manufacturer_id = @manufacturer_id, category = @category, operating_system = @operating_system, price = @price WHERE product_id = @ProductID";
                command.Parameters.AddWithValue("@name", NameTextBox.Text);
                command.Parameters.AddWithValue("@manufacturer_id", ManuID);
                command.Parameters.AddWithValue("@category", CategoryList.SelectedValue.ToString());
                command.Parameters.AddWithValue("@operating_system", OperatingSystemTextBox.Text);
                command.Parameters.AddWithValue("@price", decimal.Parse(PriceTextBox.Text));
                command.Parameters.AddWithValue("@ProductID", session.EditID);
            }

            command.ExecuteNonQuery();
            MessageBox.Show("Product changed");
        }

        private void StatusList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            RefreshList(sqlConnection);

            connection.CloseConnection(sqlConnection);
        }

        private void RefreshList(SqlConnection sqlConnection)
        {
            List<string> filter = new List<string>();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand();
            da.SelectCommand.Connection = sqlConnection;

            switch (StatusList.SelectedIndex)
            {
                case 0:
                    da.SelectCommand.CommandText = "SELECT serial_number, status FROM Product_copies WHERE product_id = @ID";
                    da.SelectCommand.Parameters.AddWithValue("@ID", session.EditID);
                    da.Fill(dt);
                    break;
                case 1:
                    da.SelectCommand.CommandText = "SELECT serial_number, status FROM Product_copies WHERE product_id = @ID AND status = 'Ready'";
                    da.SelectCommand.Parameters.AddWithValue("@ID", session.EditID);
                    da.Fill(dt);
                    break;
                case 2:
                    da.SelectCommand.CommandText = "SELECT serial_number, status FROM Product_copies WHERE product_id = @ID AND status = 'In_use'";
                    da.SelectCommand.Parameters.AddWithValue("@ID", session.EditID);
                    da.Fill(dt);
                    break;
                case 3:
                    da.SelectCommand.CommandText = "SELECT serial_number, status FROM Product_copies WHERE product_id = @ID AND status = 'Malfunction'";
                    da.SelectCommand.Parameters.AddWithValue("@ID", session.EditID);
                    da.Fill(dt);
                    break;   
            }

            foreach (DataRow dr in dt.Rows)
            {
                filter.Add(dr["serial_number"].ToString());
            }

            ProductCopyListBox.ItemsSource = filter;

            ICollectionView view = CollectionViewSource.GetDefaultView(filter);

            new TextSearchFilter(view, SearchProductCopy);
        }

        private void EditCopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCopyListBox.SelectedItem == null)
            {
                MessageBox.Show("No product was selected");
            }
            else
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);

                SqlCommand command = new SqlCommand("SELECT product_copy_id FROM Product_copies WHERE serial_number = @SerialNumber", sqlConnection);
                command.Parameters.AddWithValue("@SerialNumber", ProductCopyListBox.SelectedItem.ToString());

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    session.EditID = (int)reader[0];
                }
                reader.Close();

                EditProductPage editProductPage = new EditProductPage();
                session.addPreviousWindow(editProductPage);

                AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
                administratorWindow.ShowingFrame.Content = new EditProductCopyPage();
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;
            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }

        private void SearchProductCopy_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchProductCopy.Text = "";
        }

        private int FillInCheck()
        {
            if (NameTextBox.Text == "")
            {
                MessageBox.Show("Product has to have a name");
                return -1;
            }

            return 0;
        }
    }
}
