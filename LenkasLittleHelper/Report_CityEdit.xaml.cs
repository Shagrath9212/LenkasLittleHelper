using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for Report_CityEdit.xaml
    /// </summary>
    public partial class Report_CityEdit : Window
    {
        public ObservableCollection<City> Cities { get; } = new();
        private int IdDayReport { get; }
        public Report_CityEdit(int idDayReport, string date)
        {
            IdDayReport = idDayReport;
            InitializeComponent();
            Title = $"Додавання міста на {date}";
            ListCities.ItemsSource = Cities;
            InitCities();
        }

        private void InitCities()
        {
            Cities.Clear();
            string sql = "SELECT ID_CITY, TITLE CITY_NAME FROM CITIES ORDER BY TITLE";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idCity = e.GetValueOrDefault<int>("ID_CITY");
                    string? city = e.GetValueOrDefault<string>("CITY_NAME");

                    Cities.Add(new City(idCity, city));
                }
            });
        }

        private void City_Save_Click(object _, RoutedEventArgs __)
        {
            var selectedCity = (City)ListCities.SelectedItem;

            if (selectedCity == null)
            {
                return;
            }

            string sql = $@"INSERT INTO REPORT_CITIES (ID_REPORT_DAY, ID_CITY)
                    VALUES ({IdDayReport}, {selectedCity.Id})";

            DBHelper.DoCommand(sql);
            Close();
        }
    }
}