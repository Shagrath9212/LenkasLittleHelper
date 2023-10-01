using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LenkasLittleHelper.Windows.Directories
{
    /// <summary>
    /// Interaction logic for Cities.xaml
    /// </summary>
    public partial class Cities : Window
    {
        private ObservableCollection<City_Directory> Cities_Src { get; set; } = new();
        public Cities()
        {
            InitializeComponent();

            ListCities.ItemsSource = Cities_Src;

            LoadCities();
        }

        private void LoadCities()
        {
            Cities_Src.Clear();
            string sql = $@"SELECT ID_CITY, 
                            TITLE, 
                            IS_ARCHIVED FROM CITIES ";

            if (!ShowArch_Cities.IsChecked.HasValue || !ShowArch_Cities.IsChecked.Value)
            {
                sql += "WHERE IS_ARCHIVED=0";
            }

            sql += " ORDER BY TITLE";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idCity = e.GetValueOrDefault<int>("ID_CITY");
                    string? title = e.GetValueOrDefault<string>("TITLE");
                    bool isArchived = e.GetValueOrDefault<bool>("IS_ARCHIVED");

                    Cities_Src.Add(new City_Directory(idCity, title, isArchived));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }
        }

        private void ListCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListCities.SelectedItem is not City_Directory city)
            {
                Btn_EditCity.IsEnabled = false;
                Btn_ArchiveCity.IsEnabled = false;
                return;
            }

            Btn_EditCity.IsEnabled = true;
            Btn_ArchiveCity.IsEnabled = true;

            Btn_ArchiveCity.Content = city.IsArchived ? "Із архіву" : "У архів";
        }

        private void Btn_Add_EditCity_Click(object sender, RoutedEventArgs e)
        {
            if (ListCities.SelectedItem is not City_Directory city)
            {
                return;
            }

            var wndCity = new AddEditCity(city);

            wndCity.Show();

            wndCity.Closed += (s, e) =>
            {
                LoadCities();
            };
        }

        private void Btn_ArchiveCity_Click(object sender, RoutedEventArgs e)
        {
            if (ListCities.SelectedItem is not City_Directory city)
            {
                return;
            }

            string sql = $@"UPDATE CITIES SET IS_ARCHIVED={Convert.ToInt32(!city.IsArchived)} 
                            WHERE ID_CITY={city.Id}";

            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }

            LoadCities();
        }

        private void SearchCity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchCity.Text) || SearchCity.Text.Length < 2)
            {
                ListCities.ItemsSource = Cities_Src;
                return;
            }

            var filterText = SearchCity.Text.ToLower();

            var filtered = Cities_Src.Where(p =>
            !string.IsNullOrEmpty(p.CityName) && p.CityName.ToLower().IndexOf(filterText) != -1);

            ListCities.ItemsSource = filtered;
        }

        private void ShowArch_Cities_Checked(object sender, RoutedEventArgs e)
        {
            LoadCities();
        }

        private void Btn_Add_City_Click(object sender, RoutedEventArgs e)
        {
            var wndCity = new AddEditCity();

            wndCity.Show();

            wndCity.Closed += (s, e) =>
            {
                LoadCities();
            };
        }
    }
}
