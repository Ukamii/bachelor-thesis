using System;
using System.Collections.Generic;
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
    /// Logika interakcji dla klasy EmailNotificationPage.xaml
    /// </summary>
    public partial class EmailNotificationPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public EmailNotificationPage()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);

            SqlCommand command = new SqlCommand("UPDATE Administrators SET mail = @PASS WHERE admin_id = @ID", sqlConnection);
            command.Parameters.AddWithValue("@PASS", EmailPassword.Password.ToString());
            command.Parameters.AddWithValue("@ID", session.UserID);

            command.ExecuteNonQuery();

            connection.CloseConnection(sqlConnection);

            MessageBox.Show("Password applied");
            EmailPassword.Clear();
        }
    }
}
