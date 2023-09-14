using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for Report_CityEdit.xaml
    /// </summary>
    public partial class Report_CityEdit : Window
    {
        public Report_CityEdit(int idReport, string date)
        {
            InitializeComponent();
            Title = $"Додавання міста на {date}";
        }

        private void City_Save_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
