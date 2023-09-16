using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for EditHospital.xaml
    /// </summary>
    public partial class EditHospital : Window
    {
        public ObservableCollection<City> Cities { get; } = new();

        private Hospital_Directories? hospitalCurr { get; set; }

        private void Init()
        {
            InitializeComponent();

            ListCities.ItemsSource = Cities;

            InitCities();
        }

        public EditHospital()
        {
            Init();
        }

        public EditHospital(Hospital_Directories hospital)
        {
            hospitalCurr = hospital;
            Init();

            foreach (var city in ListCities.Items)
            {
                var _city = (City)city;
                if (_city == null)
                {
                    continue;
                }

                if (_city.Id == hospital.IdCity)
                {
                    ListCities.SelectedItem = city;
                    break;
                }
            }
            HospitalName.Text = hospital.Title;
        }

        private void InitCities()
        {
            string sql = "SELECT ID_CITY, TITLE CITY_NAME FROM CITIES ORDER BY TITLE";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idCity = e.IsDBNull("ID_CITY") ? 0 : e.GetInt32("ID_CITY");
                    string city = e.IsDBNull("CITY_NAME") ? string.Empty : e.GetString("CITY_NAME");

                    Cities.Add(new City(idCity, city));
                }
            });
        }

        private void ListCities_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //var selectedCity = (City)ListCities.SelectedValue;

            //string sql=$@"UPDATE HOSPITALS SET ID_CITY={selectedCity.Id} 
            //            WHERE ID_HOSPITAL={selectedCity}"
        }

        private void Hospital_Save_Click(object sender, RoutedEventArgs e)
        {
            string? sql = null;

            var selectedCity = (City)ListCities.SelectedItem;

            if (hospitalCurr != null)
            {
                sql = $@"UPDATE HOSPITALS SET ID_CITY={selectedCity.Id} 
                            WHERE ID_HOSPITAL={hospitalCurr.IdHospital}";
            }

            DBHelper.DoCommand(sql);

            this.Close();
        }
    }
}
