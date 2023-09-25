using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using LenkasLittleHelper.Windows.Directories;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace LenkasLittleHelper.Directories
{
    /// <summary>
    /// Interaction logic for Pharmacies.xaml
    /// </summary>
    public partial class WindowPharmacies : Window
    {
        private ObservableCollection<Models.City_Pharmacies> Cities { get; set; } = new();
        private ObservableCollection<Models.Pharmacy> Pharmacies { get; set; } = new();
        public WindowPharmacies()
        {
            InitializeComponent();

            ListCities.ItemsSource = Cities;
            ListPharmacies.ItemsSource = Pharmacies;

            LoadCities();
        }

        private void LoadCities()
        {
            Cities.Clear();

            string sql = @"SELECT
                  C.ID_CITY,
                  C.TITLE,
                  (SELECT
                      COUNT(1)
                    FROM PHARMACIES
                    WHERE ID_CITY = C.ID_CITY
                    AND IS_ARCHIVED = 0) CNT_PHARMACIES
                FROM CITIES C
                ORDER BY C.TITLE";

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idCity = e.GetValueOrDefault<int>("ID_CITY");
                    string? titleCity = e.GetValueOrDefault<string>("TITLE");
                    int cntPharmacies = e.GetValueOrDefault<int>("CNT_PHARMACIES");

                    var city = new City_Pharmacies(idCity, titleCity);

                    city.UpdateCounter(cntPharmacies);

                    Cities.Add(city);
                }
            });
        }

        private void LoadPharmacies(int idCity)
        {
            Pharmacies.Clear();

            StringBuilder sql = new($@"SELECT
                  P.ID_PHARMACY,
                  C.TITLE NAME_CITY,
                  P.NAME_PHARMACY,
                  P.STREET,
                  P.BUILD_NUM,
                  P.IS_ARCHIVED
                FROM PHARMACIES P
	             LEFT JOIN CITIES C ON C.ID_CITY=P.ID_CITY
                    WHERE P.ID_CITY={idCity}");

            if (!ShowArch_Pharmacies.IsChecked.HasValue || !ShowArch_Pharmacies.IsChecked.Value)
            {
                sql.Append(" AND P.IS_ARCHIVED=0");
            }

            sql.Append(" ORDER BY NAME_PHARMACY");

            DBHelper.ExecuteReader(sql.ToString(), e =>
            {
                while (e.Read())
                {
                    int idPharmacy = e.GetValueOrDefault<int>("ID_PHARMACY");
                    string? nameCity = e.GetValueOrDefault<string>("NAME_CITY");
                    string? namePharmacy = e.GetValueOrDefault<string>("NAME_PHARMACY");
                    string? street = e.GetValueOrDefault<string>("STREET");
                    string? build = e.GetValueOrDefault<string>("BUILD_NUM");
                    bool isArchived = e.GetValueOrDefault<bool>("IS_ARCHIVED");

                    Pharmacies.Add(new Pharmacy(idPharmacy, nameCity, namePharmacy, street, build, isArchived));
                }
            });
        }

        private void ListCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListCities.SelectedItem is not Models.City_Pharmacies city)
            {
                Btn_Add_Pharmacy.IsEnabled = false;
                return;
            }

            Btn_Add_Pharmacy.IsEnabled = true;

            LoadPharmacies(city.Id);
        }

        private void SearchPharmacy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchPharmacy.Text) || SearchPharmacy.Text.Length < 2)
            {
                ListPharmacies.ItemsSource = Pharmacies;
                return;
            }

            var filterText = SearchPharmacy.Text.ToLower();

            var filtered = Pharmacies.Where(p =>
            !string.IsNullOrEmpty(p.FullName) && p.FullName.ToLower().IndexOf(filterText) != -1);

            ListPharmacies.ItemsSource = filtered;
        }

        private void ListPharmacies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchPharmacy.Text = string.Empty;

            if (ListPharmacies.SelectedItem is not Pharmacy pharmacy)
            {
                Btn_EditPharmacy.IsEnabled = false;
                Btn_ArchivePharmacy.IsEnabled = false;
                return;
            }

            Btn_EditPharmacy.IsEnabled = true;
            Btn_ArchivePharmacy.IsEnabled = true;

            Btn_ArchivePharmacy.Content = pharmacy.IsArchived ? "Повернути із архіву" : "У архів";
        }

        private void Btn_Add_Pharmacy_Click(object sender, RoutedEventArgs e)
        {
            if (ListCities.SelectedItem is not City_Pharmacies city)
            {
                return;
            }

            var addPharmacy = new AddEditPharmacy(city.Id);
            addPharmacy.Show();
            addPharmacy.Closed += (s, e) =>
            {
                ReinitCountPharmacies(city);
            };
        }

        /// <summary>
        /// Оновлює кількість аптек по містах
        /// </summary>
        /// <param name="cityCurrent"></param>
        private void ReinitCountPharmacies(City_Pharmacies cityCurrent)
        {
            string sql = @"SELECT
                  ID_CITY,
                  COUNT(1) CNT
                FROM PHARMACIES
                WHERE IS_ARCHIVED = 0
                GROUP BY ID_CITY";

            Dictionary<int, int> citiesAndPharmaciesCount = new();

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idCty = e.GetValueOrDefault<int>("ID_CITY");
                    int count = e.GetValueOrDefault<int>("CNT");

                    citiesAndPharmaciesCount.TryAdd(idCty, count);
                }
            });

            foreach (var city in Cities)
            {
                if (!citiesAndPharmaciesCount.TryGetValue(city.Id, out int count))
                {
                    city.UpdateCounter(0);
                    continue;
                }

                city.UpdateCounter(count);
            }

            LoadPharmacies(cityCurrent.Id);
        }

        private void Btn_EditPharmacy_Click(object sender, RoutedEventArgs e)
        {
            if (ListPharmacies.SelectedItem is not Pharmacy pharmacy || ListCities.SelectedItem is not Models.City_Pharmacies city)
            {
                return;
            }

            var editPharmacy = new AddEditPharmacy(city.Id, pharmacy);
            editPharmacy.Show();
            editPharmacy.Closed += (s, e) =>
            {
                ReinitCountPharmacies(city);
            };
        }

        private void Btn_ArchivePharmacy_Click(object sender, RoutedEventArgs e)
        {
            if (ListPharmacies.SelectedItem is not Pharmacy pharmacy || ListCities.SelectedItem is not Models.City_Pharmacies city)
            {
                return;
            }

            int signArchived = Convert.ToInt32(!pharmacy.IsArchived);

            string sql = @$"UPDATE PHARMACIES
                    SET IS_ARCHIVED = {signArchived}
                    WHERE ID_PHARMACY = {pharmacy.IdPharmacy}";

            DBHelper.DoCommand(sql);
            LoadPharmacies(city.Id);
            ReinitCountPharmacies(city);
        }

        private void ShowArch_Pharmacies_Checked(object sender, RoutedEventArgs e)
        {
            if (ListCities.SelectedItem is not Models.City_Pharmacies city)
            {
                return;
            }

            LoadPharmacies(city.Id);
        }
    }
}
