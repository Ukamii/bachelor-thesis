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
    /// Logika interakcji dla klasy AddCopyOfProduct.xaml
    /// </summary>
    public partial class AddCopyOfProductPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();
        private DateTime returnDate = DateTime.Now;
        private int ProductID;
        private string category;

        public AddCopyOfProductPage()
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
            ExistingProducts.SelectedIndex = 0;

            connection.CloseConnection(sqlConnection);

            LicenseWarrantyDate.DisplayDateStart = DateTime.Now;
            LicenseWarrantyDate.SelectedDate = returnDate.AddYears(1);
        }

        private void AddCopyButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            string[] serialnumbers = SerialNumbers.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            int errorCode = FillInChecker(serialnumbers);

            if(errorCode == 0)
            {
                AddCopyOfProduct(sqlConnection, serialnumbers);
            }

            connection.CloseConnection(sqlConnection);
        }

        private void ExistingProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string SelectedProduct = ExistingProducts.SelectedItem.ToString();
            int found = SelectedProduct.IndexOf("(");
            SelectedProduct = SelectedProduct.Substring(found);
            SelectedProduct = SelectedProduct.Trim('(', ')');

            ProductID = Int32.Parse(SelectedProduct);

            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            SqlCommand command = new SqlCommand("SELECT category FROM Products WHERE product_id = @ProductID", sqlConnection);
            command.Parameters.AddWithValue("@ProductID", ProductID);

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

        private void AmountOfCopies_GotFocus(object sender, RoutedEventArgs e)
        {
            AmountOfCopies.Text = "";
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }

        private int FillInChecker(string[] serialnumbers)
        {
            if (SerialNumbers.Text == "")
            {
                MessageBox.Show("Enter serial number");
                return -1;
            }

            if (AmountOfCopies.Text == "")
            {
                MessageBox.Show("Enter amount of copies");
                return -1;
            }
            
            if (AmountOfCopies.Text == 0.ToString())
            {
                MessageBox.Show("Can't add 0 copies");
                return -1;
            }
            
            if (serialnumbers.Length != Int32.Parse(AmountOfCopies.Text))
            {
                MessageBox.Show("Amount of copies doesn't match amount of serial numbers");
                return -1;
            }

            return 0;
        }

        private void AddCopyOfProduct(SqlConnection sqlConnection, string[] serialnumbers)
        {
            try
            {
                SqlCommand command = sqlConnection.CreateCommand();
                SqlTransaction transaction;

                transaction = sqlConnection.BeginTransaction("Adding copies of product");

                command.Connection = sqlConnection;
                command.Transaction = transaction;
                try
                {
                    foreach (string number in serialnumbers)
                    {
                        command.Parameters.Clear();
                        if (category == "Hardware")
                        {
                            command.CommandText = "INSERT INTO Product_copies(product_id, serial_number, status, warranty_date) VALUES(@ProductIDCopy, @serialnumber, 'Ready', @WarrantyDate)";
                            command.Parameters.AddWithValue("@ProductIDCopy", ProductID);
                            command.Parameters.AddWithValue("@serialnumber", number);
                            command.Parameters.AddWithValue("@WarrantyDate", LicenseWarrantyDate.SelectedDate);
                        }
                        else
                        {
                            command.CommandText = "INSERT INTO Product_copies(product_id, serial_number, status, license_date) VALUES(@ProductIDCopy, @serialnumber, 'Ready', @LicenseDate)";
                            command.Parameters.AddWithValue("@ProductIDCopy", ProductID);
                            command.Parameters.AddWithValue("@serialnumber", number);
                            command.Parameters.AddWithValue("@LicenseDate", LicenseWarrantyDate.SelectedDate);
                        }

                        command.ExecuteNonQuery();
                    }

                    command.CommandText = "UPDATE Products SET copies_owned = copies_owned + @AmountOfCopies, copies_available = copies_available + @AmountOfCopies WHERE product_id = @ProductID";
                    command.Parameters.AddWithValue("@AmountOfCopies", Int32.Parse(AmountOfCopies.Text));
                    command.Parameters.AddWithValue("@ProductID", ProductID);

                    command.ExecuteNonQuery();
                    transaction.Commit();

                    MessageBox.Show("Copy added");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Commit Exception Type:" + ex.GetType() + "\nMessage: " + ex.Message);
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show("Commit Exception Type:" + ex2.GetType() + "\nMessage: " + ex2.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't add new copy to the database \nMessage: " + ex.Message);
                throw;
            }
        }
    }
}
