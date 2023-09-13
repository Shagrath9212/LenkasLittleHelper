using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for Directories.xaml
    /// </summary>
    public partial class Directories : Window
    {
        public ObservableCollection<Hospital> Hospitals { get; } = new ObservableCollection<Hospital>();

        public Directories()
        {
            InitializeComponent();

            ListHospitals.ItemsSource = Hospitals;
            InitHospitals();
        }

        private void InitHospitals()
        {
            Hospitals.Clear();

            string sqlHospitals = @"SELECT H.ID_HOSPITAL, 
                    H.TITLE, 
                    C.TITLE CITY,
                    C.ID_CITY 
                    FROM HOSPITALS H LEFT JOIN CITIES C ON H.ID_CITY=C.ID_CITY ORDER BY C.TITLE";

            var error = DBHelper.ExecuteReader(sqlHospitals, e =>
            {
                while (e.Read())
                {
                    int idHospital = e.IsDBNull("ID_HOSPITAL") ? 0 : e.GetInt32("ID_HOSPITAL");
                    string hospital = e.IsDBNull("TITLE") ? string.Empty : e.GetString("TITLE");
                    string city = e.IsDBNull("CITY") ? string.Empty : e.GetString("CITY");
                    int idCity = e.IsDBNull("ID_CITY") ? 0 : e.GetInt32("ID_CITY");

                    Hospitals.Add(new Hospital(idHospital, city, idCity, hospital));
                }
            });
        }

        private void Btn_AddHospital_Click(object sender, RoutedEventArgs e)
        {
            var pageHospital = new EditHospital();
            pageHospital.Show();
        }

        private void ListHospitals_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Btn_EditHospital.IsEnabled = ListHospitals.SelectedValue != null;
            Btn_DeleteHospital.IsEnabled = ListHospitals.SelectedValue != null;
        }

        private void Btn_EditHospital_Click(object sender, RoutedEventArgs e)
        {
            if (ListHospitals.SelectedValue == null)
            {
                return;
            }

            var hospital = (Hospital)ListHospitals.SelectedValue;

            var pageEdit = new EditHospital(hospital);
            pageEdit.Show();

            pageEdit.Closed += (s, e) =>
            {
                InitHospitals();
            };
        }
    }
}