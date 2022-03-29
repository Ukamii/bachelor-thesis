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
    /// Logika interakcji dla klasy UserSentRequestPage.xaml
    /// </summary>
    public partial class UserSentRequestPage : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public UserSentRequestPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            var CategoryOptions = new List<string> { "Hardware", "Software" };
            CategoryList.ItemsSource = CategoryOptions;
            CategoryList.SelectedIndex = 0;
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
            if (RequestsList.HasItems)
            {
                RequestsList.Items.Clear();
            }
            DataTable dt = new DataTable();
            SqlDataAdapter da;

            if (CategoryList.SelectedItem.ToString() == "Hardware")
            {
                da = new SqlDataAdapter("SELECT P.name, R.request_date FROM Requests as R JOIN Products as P ON R.product_id = P.product_id  WHERE P.category = 'Hardware' AND R.user_id = @ID AND R.status = 'Sent'", sqlConnection);
            }
            else
            {
                da = new SqlDataAdapter("SELECT P.name, R.request_date FROM Requests as R JOIN Products as P ON R.product_id = P.product_id  WHERE P.category = 'Software' AND R.user_id = @ID AND R.status = 'Sent'", sqlConnection);
            }

            da.SelectCommand.Parameters.AddWithValue("@ID", session.UserID);
            da.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {
                DateTime time = (DateTime)dr["request_date"];
                RequestsList.Items.Add(dr["name"].ToString() + " " + string.Format("{0:d}", time));
            }

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsList.SelectedItem == null)
            {
                MessageBox.Show("No request was selected");
            }
            else
            {
                SqlConnection sqlConnection = connection.Connection;
                connection.OpenConnection(sqlConnection);
                SqlCommand command = new SqlCommand();
                command.Connection = sqlConnection;
                int ID = 0;

                if (CategoryList.SelectedItem.ToString() == "Hardware")
                {
                    command.CommandText = "with Records AS(select row_number() over(order by r.request_id) as 'row', r.request_id, p.product_id FROM Products as p JOIN Requests r ON r.product_id = p.product_id WHERE r.user_id = @userID AND p.category = 'Hardware' AND r.status = 'Sent') select* from records where row = @requestRow;";
                }
                else
                {
                    command.CommandText = "with Records AS(select row_number() over(order by r.request_id) as 'row', r.request_id, p.product_id FROM Products as p JOIN Requests r ON r.product_id = p.product_id WHERE r.user_id = @userID AND p.category = 'Software' AND r.status = 'Sent') select* from records where row = @requestRow;";
                }

                command.Parameters.AddWithValue("@userID", session.UserID);
                command.Parameters.AddWithValue("@requestRow", RequestsList.SelectedIndex + 1);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    session.EditID = (int)reader[1];
                    ID = (int)reader[2];
                }

                reader.Close();

                MessageBoxResult result = MessageBox.Show("Do you want to cancel this request?",
                                          "Cancel request",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    command.CommandText = "Update Requests SET status = 'Canceled', request_check_date = GETDATE() WHERE request_id = @Rental_ID;";
                    command.Parameters.AddWithValue("@Rental_ID", session.EditID);
                    command.ExecuteNonQuery();

                    command.CommandText = "UPDATE Products SET requests_waiting = requests_waiting - 1 WHERE product_id = @ID";
                    command.Parameters.AddWithValue("@ID", ID);
                    command.ExecuteNonQuery();
                }

                session.EditID = 0;
                RefreshList(sqlConnection);
                connection.CloseConnection(sqlConnection);
            }
        }
    }
}
