using System.Windows;
using System.Windows.Navigation;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Виклик вікна довідників
        /// </summary>
        private void Btn_Directories_Click(object _sender, RoutedEventArgs _e)
        {
            var directoriesPage = new Directories();
            directoriesPage.Show();
        }
    }
}