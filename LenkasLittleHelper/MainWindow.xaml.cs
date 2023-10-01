using System.Windows;

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
            var directory = new Directories.Directory();
            directory.Show();
        }

        private void Btn_Reports_Click(object sender, RoutedEventArgs e)
        {
            var reportsPage = new ReportHelper();
            reportsPage.Show();
        }
    }
}