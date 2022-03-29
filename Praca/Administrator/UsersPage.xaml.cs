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
    /// Logika interakcji dla klasy UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public UsersPage()
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

            UserListbox.ItemsSource = filter;

            ICollectionView view = CollectionViewSource.GetDefaultView(filter);

            new TextSearchFilter(view, SearchUsers);
            connection.CloseConnection(sqlConnection);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

            if (UserListbox.SelectedItem == null)
            {
                MessageBox.Show("No user was selected");
            }
            else
            {
                string SelectedUser = UserListbox.SelectedItem.ToString();
                int found = SelectedUser.IndexOf("(");
                SelectedUser = SelectedUser.Substring(found);
                SelectedUser = SelectedUser.Trim('(', ')');

                int UserID = Int32.Parse(SelectedUser);

                session.EditID = UserID;

                UsersPage userList = new UsersPage();
                session.addPreviousWindow(userList);

                AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
                administratorWindow.ShowingFrame.Content = new AddUserPage();
            }
        }

        private void SearchUsers_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchUsers.Text = "";
        }
    }
}
