using LenkasLittleHelper.Windows.Directories;
using System.Windows;

namespace LenkasLittleHelper.Directories
{
    /// <summary>
    /// Interaction logic for Directory.xaml
    /// </summary>
    public partial class Directory : Window
    {
        public Directory()
        {
            InitializeComponent();
        }

        #region Button event listeners
        private void ShowHospitalsAndDoctors(object sender, RoutedEventArgs e)
        {
            var pageHospitalsAndDoctors = new HospitalsAndDoctors();
            pageHospitalsAndDoctors.Show();
        }
        private void ShowPharmacies(object sender, RoutedEventArgs e)
        {
            var pagePharmacies = new WindowPharmacies();
            pagePharmacies.Show();
        }

        private void Btn_Cities_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new Cities();
            wnd.Show();
        }

        private void Btn_Specialities_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new Specialities();
            wnd.Show();
        }
        #endregion
    }
}