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
    /// Logika interakcji dla klasy ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();
        

        public ProductsPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            var CategoryOptions = new List<string> { "Hardware", "Software" };
            CategoryList.ItemsSource = CategoryOptions;
            CategoryList.SelectedIndex = 0;

            RefreshList(sqlConnection);

            connection.CloseConnection(sqlConnection);
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
            List<string> filter = new List<string>();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand();
            da.SelectCommand.Connection = sqlConnection;

            if (CategoryList.SelectedItem.ToString() == "Hardware")
            {
                da.SelectCommand.CommandText = "SELECT name, product_id FROM Products WHERE category = 'Hardware'";
                da.Fill(dt);
            }
            else
            {
                da.SelectCommand.CommandText = "SELECT name, product_id FROM Products WHERE category = 'Software'";
                da.Fill(dt);
            }

            foreach (DataRow dr in dt.Rows)
            {
                filter.Add(dr["name"].ToString() + " (" + dr["product_id"].ToString() + ")");
            }

            ProductListBox.ItemsSource = filter;

            ICollectionView view = CollectionViewSource.GetDefaultView(filter);

            new TextSearchFilter(view, SearchProduct);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

            if (CategoryList.SelectedItem == null)
            {
                MessageBox.Show("No product was selected");
            }
            else
            {
                string SelectedProduct = ProductListBox.SelectedItem.ToString();
                int found = SelectedProduct.IndexOf("(");
                SelectedProduct = SelectedProduct.Substring(found);
                SelectedProduct = SelectedProduct.Trim('(', ')');

                int ProductID = Int32.Parse(SelectedProduct);

                session.EditID = ProductID;

                ProductsPage productList = new ProductsPage();
                session.addPreviousWindow(productList);

                AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
                administratorWindow.ShowingFrame.Content = new EditProductPage();
            }
        }

        private void SearchProduct_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchProduct.Text = "";
        }
    }
}
