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
    /// Logika interakcji dla klasy StatisticTimesRented.xaml
    /// </summary>
    public partial class StatisticTimesRented : Page
    {
        private ConnectionToDatabase connection = new ConnectionToDatabase();
        private Session session = new Session();

        public StatisticTimesRented()
        {
            InitializeComponent();
            showColumnChart();
        }

        private void showColumnChart()
        {
            SqlConnection sqlConnection = connection.Connection;
            connection.OpenConnection(sqlConnection);
            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();

            SqlCommand command = new SqlCommand("SELECT name, times_rented FROM Products ORDER BY times_rented DESC", sqlConnection);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                while(reader.Read())
                {
                    valueList.Add(new KeyValuePair<string, int>(reader["name"].ToString(), (int)reader["times_rented"]));
                }
            }

            reader.Close();

            // Setting data for pie chart
            pieChart.DataContext = valueList;

            connection.CloseConnection(sqlConnection);
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = session.getPreviousWindow();
        }
    }
}
