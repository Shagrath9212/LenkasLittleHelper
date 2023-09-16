using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for Report_EditDoctor.xaml
    /// </summary>
    public partial class Report_EditDoctor : Window
    {
        public ObservableCollection<Doctor> Doctors { get; } = new();
        private int IdHospital { get; }
        private int IdReportHospital { get; }
        public Report_EditDoctor(int idHospital, int idReportHospital, string? nameHospital)
        {
            IdHospital = idHospital;
            IdReportHospital = idReportHospital;
            InitializeComponent();
            ListDoctors.ItemsSource = Doctors;
            Title = $"Вибір лікаря для {nameHospital}";
            LoadDoctors();
        }

        private void LoadDoctors()
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
                WHERE A.ID_HOSPITAL = {IdHospital}";

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

        private void Doctor_Save_Click(object sender, RoutedEventArgs e)
        {
            var selectedDoctor = (Doctor)ListDoctors.SelectedItem;

            if (selectedDoctor == null)
            {
                return;
            }

            string sql = $@"INSERT INTO REPORT_DOCTORS (ID_REPORT_HOSPITAL,ID_DOCTOR) 
                            VALUES ({IdReportHospital},{selectedDoctor.IdDoctor})";

            var error = DBHelper.DoCommand(sql);

            Close();
        }
    }
}