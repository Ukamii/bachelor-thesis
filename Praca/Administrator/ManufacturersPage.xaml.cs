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
    /// Logika interakcji dla klasy ManufacturersPage.xaml
    /// </summary>
    public partial class ManufacturersPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public ManufacturersPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            List<string> filter = new List<string>();

            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter("SELECT name, manufacturer_id FROM Manufacturers", sqlConnection);
            da.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                filter.Add(dr["name"].ToString() + " (" + dr["manufacturer_id"].ToString() + ")");
            }
            
            ManufacturersListbox.ItemsSource = filter;

            ICollectionView view = CollectionViewSource.GetDefaultView(filter);

            new TextSearchFilter(view, SearchManufacturers);
            connection.CloseConnection(sqlConnection);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

            if (ManufacturersListbox.SelectedItem == null)
            {
                MessageBox.Show("No manufacturer was selected");
            }
            else
            {
                string SelectedManufacturer = ManufacturersListbox.SelectedItem.ToString();
                int found = SelectedManufacturer.IndexOf("(");
                SelectedManufacturer = SelectedManufacturer.Substring(found);
                SelectedManufacturer = SelectedManufacturer.Trim('(', ')');

                int ManufacturerID = Int32.Parse(SelectedManufacturer);

                session.EditID = ManufacturerID;

                ManufacturersPage manufacturerList = new ManufacturersPage();
                session.addPreviousWindow(manufacturerList);

                AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
                administratorWindow.ShowingFrame.Content = new AddManufacturerPage();
            }
        }

        private void SearchManufacturers_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchManufacturers.Text = "";
        }
    }
}
