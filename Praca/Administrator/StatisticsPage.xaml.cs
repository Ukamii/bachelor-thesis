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
    /// Logika interakcji dla klasy StatisticsPage.xaml
    /// </summary>
    /// 
    public class Product
    {
        public string name { get; set; }
        public int times_rented { get; set; }
        public int times_requested { get; set; }
    }

    public partial class StatisticsPage : Page
    {
        private Session session = new Session();

        public StatisticsPage()
        {
            InitializeComponent();
        }

        private void TimesRentedButton_Click(object sender, RoutedEventArgs e)
        {
            StatisticsPage statisticsPage = new StatisticsPage();
            session.addPreviousWindow(statisticsPage);

            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = new StatisticTimesRented();
        }

        private void RequestWaitButton_Click(object sender, RoutedEventArgs e)
        {
            StatisticsPage statisticsPage = new StatisticsPage();
            session.addPreviousWindow(statisticsPage);

            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = new StatisticRequestWaitingTime();
        }

        private void TimesRequestedButton_Click(object sender, RoutedEventArgs e)
        {
            StatisticsPage statisticsPage = new StatisticsPage();
            session.addPreviousWindow(statisticsPage);

            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = new StatisticTimesRequested();
        }

        private void RentTimeButton_Click(object sender, RoutedEventArgs e)
        {
            StatisticsPage statisticsPage = new StatisticsPage();
            session.addPreviousWindow(statisticsPage);

            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = new StatisticAverageRentTime();
        }
    }
}
