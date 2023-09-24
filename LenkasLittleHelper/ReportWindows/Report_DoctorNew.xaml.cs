using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for Report_DoctorNew.xaml
    /// </summary>
    public partial class Report_DoctorNew : Window
    {
        private int IdHospital { get; }
        private int IdReportHospital { get; }
        public ObservableCollection<Doctor> Doctors { get; } = new();
        public Report_DoctorNew(int idHospital, int idReportHospital, string? nameHospital, IEnumerable<int> existingDoctors)
        {
            IdHospital = idHospital;
            IdReportHospital = idReportHospital;
            InitializeComponent();
            ListDoctors.ItemsSource = Doctors;
            Title = $"Вибір лікаря для {nameHospital}";
            LoadDoctors(existingDoctors);
        }
        private void LoadDoctors(IEnumerable<int> existingDoctors)
        {
            Doctors.Clear();
            string sql = @$"SELECT
                  D.ID_DOCTOR,
                  D.FULL_NAME,
                  S.NAME_SPECIALITY,
                  A.STREET,
                  A.BUILD_NUMBER
                FROM DOCTORS D
                  LEFT JOIN ADDRESSES A
                    ON D.ID_ADDRESS = A.ID_ADDRESS
                  LEFT JOIN SPECIALITIES S
                    ON D.SPECIALITY = S.ID_SPECIALITY
                WHERE A.ID_HOSPITAL = {IdHospital}
                AND D.ID_DOCTOR NOT IN ({string.Join(',', existingDoctors)})
                ORDER BY D.FULL_NAME";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idDoctor = e.GetValueOrDefault<int>("ID_DOCTOR");
                    string? fullName = e.GetValueOrDefault<string>("FULL_NAME");
                    string? speciality = e.GetValueOrDefault<string>("NAME_SPECIALITY");
                    string? street = e.GetValueOrDefault<string>("STREET");
                    string? builNumber = e.GetValueOrDefault<string>("BUILD_NUMBER");

                    Doctors.Add(new Doctor(idDoctor, fullName, speciality, street, builNumber));
                }
            });
        }

        private void ListDoctors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListDoctors.SelectedItem is not Doctor doctor)
            {
                return;
            }
            System.Console.WriteLine(doctor.FullName);
        }

        private void SearchDoctor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchDoctor.Text) || SearchDoctor.Text.Length < 2)
            {
                ListDoctors.ItemsSource = Doctors;
                return;
            }

            var filterText = SearchDoctor.Text.ToLower();

            var filtered = Doctors.Where(d =>
            !string.IsNullOrEmpty(d.FullName) && d.FullName.ToLower().IndexOf(filterText) != -1);

            ListDoctors.ItemsSource = filtered;
        }

        private void Doctor_Save_Click(object sender, RoutedEventArgs e)
        {

            var selectedDoctors = Doctors.Where(d => d.IsChecked);

            if (!selectedDoctors.Any())
            {
                Close();
                return;
            }

            foreach (var doctor in selectedDoctors)
            {
                string sql = $@"INSERT INTO REPORT_DOCTORS (ID_REPORT_HOSPITAL,ID_DOCTOR) 
                            VALUES ({IdReportHospital},{doctor.IdDoctor})";
                
                DBHelper.DoCommand(sql);
            }

            Close();
        }
    }
}