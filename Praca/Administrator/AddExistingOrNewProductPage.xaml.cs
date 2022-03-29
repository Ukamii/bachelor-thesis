using System;
using System.Collections.Generic;
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
    /// Logika interakcji dla klasy AddExistingOrNewProduct.xaml
    /// </summary>
    public partial class AddExistingOrNewProductPage : Page
    {
        private Session session = new Session();
        public AddExistingOrNewProductPage()
        {
            InitializeComponent();
        }

        private void NewProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddExistingOrNewProductPage addExistingOrNewProduct = new AddExistingOrNewProductPage();
            session.addPreviousWindow(addExistingOrNewProduct);

            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = new AddProductPage();
        }

        private void CopyOfProductButton_Click(object sender, RoutedEventArgs e)
        {
            AddExistingOrNewProductPage addExistingOrNewProduct = new AddExistingOrNewProductPage();
            session.addPreviousWindow(addExistingOrNewProduct);

            AdministratorWindow administratorWindow = (AdministratorWindow)Window.GetWindow(this);
            administratorWindow.ShowingFrame.Content = new AddCopyOfProductPage();
        }
    }
}
