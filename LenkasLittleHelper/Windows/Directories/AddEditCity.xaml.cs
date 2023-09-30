using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.Generic;
using System.Windows;

namespace LenkasLittleHelper.Windows.Directories
{
    /// <summary>
    /// Interaction logic for AddEditCity.xaml
    /// </summary>
    public partial class AddEditCity : Window
    {
        private readonly City_Directory? CityCurrent;
        public AddEditCity()
        {
            InitializeComponent();
        }

        public AddEditCity(City_Directory cityCurrent)
        {
            InitializeComponent();
            CityCurrent = cityCurrent;
            CityName.Text = cityCurrent.CityName;
            Title = cityCurrent.CityName;
        }

        private void SaveCity_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CityName.Text))
            {
                MessageBox.Show("Назва міста не може бути порожньою!");
                return;
            }

            Dictionary<string, object> cmdParams = new()
            {
                {"title", CityName.Text }
            };

            if (CityCurrent != null)
            {
                string sql = $"UPDATE CITIES SET TITLE=@title WHERE ID_CITY={CityCurrent.Id}";
                DBHelper.DoCommand(sql);
                Close();
                return;
            }

            var citiesAllSql = "SELECT ID_CITY, TITLE, IS_ARCHIVED FROM CITIES";

            List<City_Directory> existingCities = new();

            DBHelper.ExecuteReader(citiesAllSql, e =>
            {
                while (e.Read())
                {
                    int idCity = e.GetValueOrDefault<int>("ID_CITY");
                    string? title = e.GetValueOrDefault<string>("TITLE");
                    bool isArchived = e.GetValueOrDefault<bool>("IS_ARCHIVED");

                    existingCities.Add(new City_Directory(idCity, title, isArchived));
                }
            });

            string addCitySql = "INSERT INTO CITIES(TITLE) VALUES (@title)";

            foreach (var city in existingCities)
            {
                //Якщо архівне-виводимо із архіву
                if (!string.IsNullOrEmpty(city.CityName) && city.CityName.Trim() == CityName.Text)
                {
                    if (city.IsArchived)
                    {
                        addCitySql = $"UPDATE CITIES SET IS_ARCHIVED=0 WHERE ID_CITY={city.Id}";
                    }
                    else
                    {
                        MessageBox.Show($"Місто {city.CityName} уже існує!", "Неможливо додати місто!");
                        return;
                    }
                }
            }

            var error = DBHelper.DoCommand(addCitySql, cmdParams);
            Close();
        }
    }
}
