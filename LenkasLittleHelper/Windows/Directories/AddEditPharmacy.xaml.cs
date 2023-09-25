using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace LenkasLittleHelper.Windows.Directories
{
    /// <summary>
    /// Interaction logic for AddEditPharmacy.xaml
    /// </summary>
    public partial class AddEditPharmacy : Window
    {
        private readonly Pharmacy? PharmacyCurrent;
        private ObservableCollection<City> Cities { get; set; } = new();
        public AddEditPharmacy(int idCity, Pharmacy pharmacyCurrent)
        {
            InitializeComponent();

            ListCities.ItemsSource = Cities;

            PharmacyCurrent = pharmacyCurrent;

            LoadCities();

            SetCitySelected(idCity);

            Street.Text = pharmacyCurrent.Street;
            Build.Text = pharmacyCurrent.BuildNum;
            FullName.Text = pharmacyCurrent.FullName;
        }

        public AddEditPharmacy(int idCity)
        {
            InitializeComponent();

            ListCities.ItemsSource = Cities;

            LoadCities();
            SetCitySelected(idCity);
        }

        private void SetCitySelected(int idCity)
        {
            foreach (var city in Cities)
            {
                if (city.Id == idCity)
                {
                    ListCities.SelectedItem = city;
                    return;
                }
            }
        }

        private void LoadCities()
        {
            string sql = "SELECT ID_CITY, TITLE CITY_NAME FROM CITIES ORDER BY TITLE";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idCity = e.GetValueOrDefault<int>("ID_CITY");
                    var city = e.GetValueOrDefault<string>("CITY_NAME");

                    Cities.Add(new City(idCity, city));
                }
            });
        }

        private void PharmacyAdd_Click(object sender, RoutedEventArgs e)
        {
            if (ListCities.SelectedItem is not City city)
            {
                MessageBox.Show($"Не вказано місто!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(FullName.Text))
            {
                MessageBox.Show($"Не вказано назву аптеки!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(Street.Text))
            {
                MessageBox.Show($"Не вказано вулицю!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(Build.Text))
            {
                MessageBox.Show($"Не вказано номер будинку!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Dictionary<string, object> cmdParams = new()
            {
                {"namePharmacy", FullName.Text},
                {"idCity", city.Id},
                {"street", Street.Text},
                {"build", Build.Text}
            };

            string sql;

            if (PharmacyCurrent != null)
            {
                sql = $@"UPDATE PHARMACIES
                        SET NAME_PHARMACY = @namePharmacy,
                            ID_CITY = @idCity,
                            STREET = @street,
                            BUILD_NUM = @build WHERE ID_PHARMACY={PharmacyCurrent.IdPharmacy}";
            }
            else
            {
                sql = @"INSERT INTO PHARMACIES (NAME_PHARMACY, ID_CITY, STREET, BUILD_NUM)
                    VALUES (@namePharmacy, @idCity, @street, @build)";
            }

            var error = DBHelper.DoCommand(sql, cmdParams);

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Close();
        }
    }
}
