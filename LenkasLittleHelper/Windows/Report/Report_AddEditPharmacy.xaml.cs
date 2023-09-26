using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LenkasLittleHelper.Windows.Report
{
    /// <summary>
    /// Interaction logic for Report_AddEditPharmacy.xaml
    /// </summary>
    public partial class Report_AddEditPharmacy : Window
    {
        private ObservableCollection<Pharmacy> Pharmacies { get; set; } = new();

        private readonly int IdReportCity;

        public Report_AddEditPharmacy(int idCity, int idReportCity, IEnumerable<int> existing)
        {
            InitializeComponent();
            ListPharmacies.ItemsSource = Pharmacies;
            IdReportCity = idReportCity;
            LoadPharmacies(idCity, existing);
        }

        private void LoadPharmacies(int idCity, IEnumerable<int> existing)
        {
            Pharmacies.Clear();

            string sql = $@"SELECT
                      ID_PHARMACY,
                      NAME_PHARMACY,
                      STREET,
                      BUILD_NUM
                    FROM PHARMACIES
                    WHERE ID_CITY = {idCity}
                    AND IS_ARCHIVED = 0
                    AND ID_PHARMACY NOT IN({string.Join(',', existing)})";

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idPharmacy = e.GetValueOrDefault<int>("ID_PHARMACY");
                    string? namePharmacy = e.GetValueOrDefault<string>("NAME_PHARMACY");
                    string? street = e.GetValueOrDefault<string>("STREET");
                    string? build = e.GetValueOrDefault<string>("BUILD_NUM");

                    Pharmacies.Add(new Pharmacy(idPharmacy, namePharmacy, street, build));
                }
            });
        }

        private void Pharmacies_Save_Click(object sender, RoutedEventArgs e)
        {
            var selected = Pharmacies.Where(p => p.IsChecked);

            if (!selected.Any())
            {
                Close();
                return;
            }

            foreach (var pharmacy in selected)
            {
                string sql = $@"INSERT INTO REPORT_PHARMACIES (ID_REPORT_CITY, ID_PHARMACY)
                    VALUES ({IdReportCity}, {pharmacy.IdPharmacy})";

                var error = DBHelper.DoCommand(sql);

                if (!string.IsNullOrEmpty(error))
                {

                }
            }

            Close();
        }

        private void ListPharmacies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SearchPharmacy_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchPharmacy.Text) || SearchPharmacy.Text.Length < 2)
            {
                ListPharmacies.ItemsSource = Pharmacies;
                return;
            }

            var filterText = SearchPharmacy.Text.ToLower();

            var filtered = Pharmacies.Where(d =>
            !string.IsNullOrEmpty(d.FullName) && d.FullName.ToLower().IndexOf(filterText) != -1);

            ListPharmacies.ItemsSource = filtered;
        }
    }
}