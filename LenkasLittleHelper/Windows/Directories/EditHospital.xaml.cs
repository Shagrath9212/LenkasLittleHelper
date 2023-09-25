using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
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

            SetCitySelected(hospital.IdCity);

            HospitalName.Text = hospital.Title;
        }

        private void SetCitySelected(int idCity)
        {
            foreach (var city in ListCities.Items)
            {
                var _city = (City)city;
                if (_city == null)
                {
                    continue;
                }

                if (_city.Id == idCity)
                {
                    ListCities.SelectedItem = city;
                    break;
                }
            }
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

            int idCity = selectedCity?.Id ?? -1;

            if (!string.IsNullOrEmpty(CityNameCustom.Text))
            {
                if (Cities.FirstOrDefault
                    (e => !string.IsNullOrEmpty(e.CityName) && e.CityName.ToLower() == CityNameCustom.Text.ToLower()) != null)
                {
                    MessageBox.Show($"Місто {CityNameCustom.Text} уже існує у колекції міст!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string addCitySql = "INSERT INTO CITIES (TITLE) VALUES (@nameCity)";

                Dictionary<string, object>? cmdParams = new()
                {
                    {"nameCity",CityNameCustom.Text }
                };

                var error = DBHelper.DoCommand(addCitySql, cmdParams);

                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(error, "Помилка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string getLastId = "SELECT ID_CITY FROM CITIES ORDER BY ID_CITY DESC";

                DBHelper.ExecuteReader(getLastId, e =>
                {
                    if (e.Read())
                    {
                        idCity = e.GetValueOrDefault("ID_CITY", idCity);
                    }
                });
            }

            var hospitalsCmdParams = new Dictionary<string, object>
            {
                { "title", HospitalName.Text },
                { "idCity", idCity }
            };

            if (hospitalCurr != null)
            {
                sql = $@"UPDATE HOSPITALS SET ID_CITY=@idCity, TITLE=@title
                            WHERE ID_HOSPITAL={hospitalCurr.IdHospital}";
            }
            else
            {
                sql = "INSERT INTO HOSPITALS (TITLE,ID_CITY) VALUES (@title,@idCity)";
            }

            DBHelper.DoCommand(sql, hospitalsCmdParams);

            this.Close();
        }
    }
}
