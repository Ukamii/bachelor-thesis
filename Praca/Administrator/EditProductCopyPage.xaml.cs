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
    /// Logika interakcji dla klasy EditProductCopyPage.xaml
    /// </summary>
    public partial class EditProductCopyPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();
        private bool FirstTime = true;

        public EditProductCopyPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter("SELECT name, product_id FROM Products", sqlConnection);

            da.Fill(dt);

            List<string> ProductList = new List<string>();

            foreach (DataRow dr in dt.Rows)
            {
                ProductList.Add(dr["name"].ToString() + " (" + dr["product_id"].ToString() + ")");
            }
            ExistingProducts.ItemsSource = ProductList;

            var StatusOptions = new List<string> { "Ready", "In_use", "Malfunction" };
            StatusBox.ItemsSource = StatusOptions;


            LoadCopy(sqlConnection);

            connection.CloseConnection(sqlConnection);
            FirstTime = false;
        }

        private void UpdateCopyButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            int errorCode = FillInCheck();

            if (errorCode == 0)
            {
                UpdateProductCopy(sqlConnection);
            }

            connection.CloseConnection(sqlConnection);

            session.EditID = 0;
            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }

        private void LoadCopy(SqlConnection sqlConnection)
        {
            int Product = -1;
            string category;

            SqlCommand command = new SqlCommand("SELECT product_id FROM Product_copies WHERE product_copy_id = @ProductIDLoad", sqlConnection);
            command.Parameters.AddWithValue("@ProductIDLoad", session.EditID);

            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                Product = (int)reader[0];
            }
            reader.Close();

            command.CommandText = "SELECT P.name, PC.serial_number, PC.status, P.category, PC.license_date, PC.warranty_date, P.product_id, PC.comment FROM Products as P JOIN Product_copies as PC ON P.product_id = PC.product_id WHERE P.product_id = @ID AND PC.product_copy_id = @CopyID";
            command.Parameters.AddWithValue("@ID", Product);
            command.Parameters.AddWithValue("@CopyID", session.EditID);

            reader = command.ExecuteReader();
            string fullname;
            if (reader.HasRows)
            {
                reader.Read();
                fullname = reader[0].ToString();

                ExistingProducts.SelectedIndex = 4;
                SerialNumbers.Text = reader[1].ToString();
                StatusBox.SelectedItem = reader[2].ToString();
                category = reader[3].ToString();

                if(category == "Hardware")
                {
                    LicenseWarrantyDate.SelectedDate = (DateTime)reader[5];
                    LicenseWarrantyDate.Text = reader[5].ToString();
                    LicenseWarrantyLabel.Content = "Choose warranty date";
                }
                else
                {
                    LicenseWarrantyDate.SelectedDate = (DateTime)reader[4];
                    LicenseWarrantyDate.Text = reader[4].ToString();
                    LicenseWarrantyLabel.Content = "Choose license date";
                }

                CommentBox.Text = reader[7].ToString();
                fullname += " (";
                fullname += reader[6];
                fullname += ")";

                ExistingProducts.SelectedItem = fullname;
            }
            reader.Close();

            connection.CloseConnection(sqlConnection);         
        }

        private void UpdateProductCopy(SqlConnection sqlConnection)
        {
            int Product = -1;
            string category = null;

            SqlCommand command = new SqlCommand("SELECT product_id FROM Product_copies WHERE product_copy_id = @ProductIDLoad", sqlConnection);
            command.Parameters.AddWithValue("@ProductIDLoad", session.EditID);

            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                Product = (int)reader[0];
            }
            reader.Close();

            command.CommandText = "SELECT category FROM Products WHERE product_id = @ID";
            command.Parameters.AddWithValue("@ID", Product);

            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                category = reader[0].ToString();
            }
            reader.Close();

            string SelectedProduct = ExistingProducts.SelectedItem.ToString();
            int found = SelectedProduct.IndexOf("(");
            SelectedProduct = SelectedProduct.Substring(found);
            SelectedProduct = SelectedProduct.Trim('(', ')');


            if (category == "Hardware")
            {
                command.CommandText = "UPDATE Product_copies SET product_id = @ProductID, serial_number = @SerialNumber, status = @status, warranty_date = @WarrantyDate, comment = @Comment WHERE product_copy_id = @CopyID";
                command.Parameters.AddWithValue("@ProductID", SelectedProduct);
                command.Parameters.AddWithValue("@SerialNumber", SerialNumbers.Text);
                command.Parameters.AddWithValue("@status", StatusBox.SelectedItem.ToString());
                command.Parameters.AddWithValue("@WarrantyDate", LicenseWarrantyDate.SelectedDate);
                command.Parameters.AddWithValue("@Comment", CommentBox.Text);
                command.Parameters.AddWithValue("@CopyID", session.EditID);
            }
            else
            {
                command.CommandText = "UPDATE Product_copies SET product_id = @ProductID, serial_number = @SerialNumber, status = @status, license_date = @LicenseDate, comment = @Comment WHERE product_copy_id = @CopyID";
                command.Parameters.AddWithValue("@ProductID", SelectedProduct);
                command.Parameters.AddWithValue("@SerialNumber", SerialNumbers.Text);
                command.Parameters.AddWithValue("@status", StatusBox.SelectedItem.ToString());
                command.Parameters.AddWithValue("@LicenseDate", LicenseWarrantyDate.SelectedDate);
                command.Parameters.AddWithValue("@Comment", CommentBox.Text);
                command.Parameters.AddWithValue("@CopyID", session.EditID);
            }

            command.ExecuteNonQuery();
            MessageBox.Show("Product copy changed");
        }

        private void ExistingProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(FirstTime != true)
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);
                string category = null;

                string SelectedProduct = ExistingProducts.SelectedItem.ToString();
                int found = SelectedProduct.IndexOf("(");
                SelectedProduct = SelectedProduct.Substring(found);
                SelectedProduct = SelectedProduct.Trim('(', ')');

                SqlCommand command = new SqlCommand("SELECT category FROM Products WHERE product_id = @ProductID", sqlConnection);
                command.Parameters.AddWithValue("@ProductID", SelectedProduct);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    category = reader[0].ToString();
                }
                reader.Close();

                connection.CloseConnection(sqlConnection);

                if(category == "Hardware")
                {
                    LicenseWarrantyLabel.Content = "Choose warranty date";
                }
                else
                {
                    LicenseWarrantyLabel.Content = "Choose license date";
                }
            }
        }

        private int FillInCheck()
        {
            if (SerialNumbers.Text == "")
            {
                MessageBox.Show("Copy has to have a serial number");
                return -1;
            }

            return 0;
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            session.EditID = 0;

            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }

        private void CommentBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CommentBox.Text = "";
        }
    }
}
