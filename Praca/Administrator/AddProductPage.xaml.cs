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
    /// Logika interakcji dla klasy AddProduct.xaml
    /// </summary>
    public partial class AddProductPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();
        private DateTime licenseWarranty = DateTime.Now;

        public AddProductPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
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

            var StatusOptions = new List<string> { "Ready", "In_use", "Malfunction" };
            var CategoryOptions = new List<string> { "Hardware", "Software" };

            CategoryList.ItemsSource = CategoryOptions;
            StatusList.ItemsSource = StatusOptions;

            CategoryList.SelectedIndex = 0;
            StatusList.SelectedIndex = 0;
            ManufacturerList.SelectedIndex = 0;
            LicenseWarrantyDate.DisplayDateStart = DateTime.Now;
            LicenseWarrantyDate.SelectedDate = licenseWarranty.AddYears(1);

            connection.CloseConnection(sqlConnection);
        }

        private void AddNewProductButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            string[] serialnumbers = SerialnumberTextBox.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            int errorCode = FillInChecker(serialnumbers);

            if(errorCode == 0)
            {
                AddProduct(sqlConnection, serialnumbers);
            }
            
            connection.CloseConnection(sqlConnection);
        }

        private void AddProduct(SqlConnection sqlConnection, string[] serialnumbers)
        {
            try
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
                SqlTransaction transaction;
                transaction = sqlConnection.BeginTransaction("Adding Product");

                command.Connection = sqlConnection;
                command.Transaction = transaction;

                try
                {

                    if (CategoryList.SelectedItem.ToString() == "Hardware")
                    {

                        command.CommandText = "INSERT INTO Products(name, manufacturer_id, category, price, copies_owned, copies_available) VALUES(@name, @manufacturer_id, @category, @price, @Amount, @Amount)";

                        command.Parameters.AddWithValue("@name", NameTextBox.Text);
                        command.Parameters.AddWithValue("@manufacturer_id", ManuID);
                        command.Parameters.AddWithValue("@category", CategoryList.SelectedValue.ToString());
                        command.Parameters.AddWithValue("@price", decimal.Parse(PriceTextBox.Text));
                        command.Parameters.AddWithValue("@Amount", Int32.Parse(AmountOfCopies.Text));
                    }
                    else
                    {

                        command.CommandText = "INSERT INTO Products(name, manufacturer_id, category, price, operating_system, copies_owned, copies_available) VALUES(@name, @manufacturer_id, @category, @price, @operating_system, @Amount, @Amount)";

                        command.Parameters.AddWithValue("@name", NameTextBox.Text);
                        command.Parameters.AddWithValue("@manufacturer_id", ManuID);
                        command.Parameters.AddWithValue("@category", CategoryList.SelectedValue.ToString());
                        command.Parameters.AddWithValue("@operating_system", OperatingsystemTextBox.Text);
                        command.Parameters.AddWithValue("@price", decimal.Parse(PriceTextBox.Text));
                        command.Parameters.AddWithValue("@Amount", Int32.Parse(AmountOfCopies.Text));

                    }
                    command.ExecuteNonQuery();

                    foreach (string number in serialnumbers)
                    {
                        command.Parameters.Clear();
                        if (CategoryList.SelectedItem.ToString() == "Hardware")
                        {
                            command.CommandText = "INSERT INTO Product_copies(product_id, serial_number, status, warranty_date) VALUES(@@IDENTITY, @serialnumber, 'Ready', @WarrantyDate)";
                            command.Parameters.AddWithValue("@serialnumber", number);
                            command.Parameters.AddWithValue("@WarrantyDate", LicenseWarrantyDate.SelectedDate);
                        }
                        else
                        {
                            command.CommandText = "INSERT INTO Product_copies(product_id, serial_number, status, license_date) VALUES(@@IDENTITY , @serialnumber, 'Ready', @LicenseDate)";
                            command.Parameters.AddWithValue("@serialnumber", number);
                            command.Parameters.AddWithValue("@LicenseDate", LicenseWarrantyDate.SelectedDate);
                        }

                        command.ExecuteNonQuery();
                    }


                    transaction.Commit();

                    MessageBox.Show("Product added");

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
                MessageBox.Show("Couldn't add new product to the database \nMessage: " + ex.Message);
            }
        }

        private void CategoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryList.SelectedValue.ToString() == "Hardware")
            {
                LicenseWarrantyLabel.Content = "Choose warranty date";
                OperatingsystemTextBox.Visibility = Visibility.Collapsed;
                OperatingSystemLabel.Visibility = Visibility.Collapsed;
            }
            else if (CategoryList.SelectedValue.ToString() == "Software")
            {
                LicenseWarrantyLabel.Content = "Choose license date";
                OperatingsystemTextBox.Visibility = Visibility.Visible;
                OperatingSystemLabel.Visibility = Visibility.Visible;
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }

        private void AmountOfCopies_GotFocus(object sender, RoutedEventArgs e)
        {
            AmountOfCopies.Text = "";
        }

        private int FillInChecker(string[] serialnumbers)
        {
            if (NameTextBox.Text == "")
            {
                MessageBox.Show("No name was given");
                return -1;
            }

            if (ManufacturerList.SelectedItem == null)
            {
                MessageBox.Show("No manufacturer was selected");
                return -1;
            }

            if (CategoryList.SelectedItem == null)
            {
                MessageBox.Show("No category was selected");
                return -1;
            }

            if (StatusList.SelectedItem == null)
            {
                MessageBox.Show("No status was selected");
                return -1;
            }

            if (SerialnumberTextBox.Text == "")
            {
                MessageBox.Show("Enter serial number");
                return -1;
            }

            if (AmountOfCopies.Text == "")
            {
                MessageBox.Show("Enter amount of copies");
                return -1;
            }

            if (!int.TryParse(AmountOfCopies.Text, out _))
            {
                MessageBox.Show("You have to number in amount of copies");
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
    }
}
