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
    /// Logika interakcji dla klasy StatisticAverageRentTime.xaml
    /// </summary>
    public partial class StatisticAverageRentTime : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public StatisticAverageRentTime()
        {
            InitializeComponent();
            showColumnChart();
        }

        private void showColumnChart()
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);
            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();

            SqlCommand command = new SqlCommand("SELECT P.name, R.product_id, AVG(DATEDIFF(d, R.rent_date, R.Return_date)) as DIFF FROM Rentals as R JOIN Products as P ON P.product_id = R.product_id WHERE R.status = 'Finished' GROUP BY P.name, R.product_id", sqlConnection);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    valueList.Add(new KeyValuePair<string, int>(reader["name"].ToString(), (int)reader["DIFF"]));
                }
            }

            reader.Close();

            // Setting data for pie chart
            barChart.DataContext = valueList;

            connection.CloseConnection(sqlConnection);
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }
    }
}
