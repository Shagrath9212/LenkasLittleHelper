using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for Report_HospitalEdit.xaml
    /// </summary>
    public partial class Report_HospitalEdit : Window
    {
        private int IDCity { get; }
        private int IDReportCity { get; }
        public ObservableCollection<Hospital> Hospitals { get; } = new();
        public Report_HospitalEdit(int idCity, int idReportCity, string? nameCity, IEnumerable<int> alreadyAdded)
        {
            InitializeComponent();
            IDCity = idCity;
            IDReportCity = idReportCity;
            Title = $"Додавання лікарні для міста {nameCity}";

            ListHospitals.ItemsSource = Hospitals;
            LoadHospitals(alreadyAdded);
        }

        private void LoadHospitals(IEnumerable<int> alreadyAdded)
        {
            Hospitals.Clear();
            string sql = $"SELECT ID_HOSPITAL, TITLE FROM HOSPITALS WHERE ID_CITY={IDCity} " +
                $"AND ID_HOSPITAL NOT IN ({string.Join(',', alreadyAdded)})";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idHospital = e.GetValueOrDefault<int>("ID_HOSPITAL");
                    string? hospitalName = e.GetValueOrDefault<string>("TITLE");
                    Hospitals.Add(new Hospital(idHospital, hospitalName));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }
        }

        private void HospitalSave_Click(object sender, RoutedEventArgs e)
        {
            var selectedHospital = (Hospital)ListHospitals.SelectedItem;
            if (selectedHospital == null)
            {
                return;
            }
            string sql = @$"INSERT INTO REPORT_HOSPITALS (ID_REPORT_CITY,ID_HOSPITAL) 
                            VALUES ({IDReportCity},{selectedHospital.IdHospital})";

            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
                return;
            }

            Close();
        }
    }
}
