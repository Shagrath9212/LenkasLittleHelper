using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for Directories.xaml
    /// </summary>
    public partial class Directories : Window
    {
        public ObservableCollection<Hospital> Hospitals { get; } = new ObservableCollection<Hospital>();
        public ObservableCollection<Doctor> Doctors { get; } = new ObservableCollection<Doctor>();

        public Directories()
        {
            InitializeComponent();

            ListHospitals.ItemsSource = Hospitals;
            ListDoctors.ItemsSource = Doctors;
            InitHospitals();
        }

        /// <summary>
        /// Метод для ініціалізації переліку лікарень
        /// </summary>
        private void InitHospitals()
        {
            Hospitals.Clear();

            string sqlHospitals = @"SELECT H.ID_HOSPITAL, 
                    H.TITLE, 
                    C.TITLE CITY,
                    C.ID_CITY 
                    FROM HOSPITALS H LEFT JOIN CITIES C ON H.ID_CITY=C.ID_CITY ORDER BY C.TITLE";

            var error = DBHelper.ExecuteReader(sqlHospitals, e =>
            {
                while (e.Read())
                {
                    int idHospital = e.IsDBNull("ID_HOSPITAL") ? 0 : e.GetInt32("ID_HOSPITAL");
                    string hospital = e.IsDBNull("TITLE") ? string.Empty : e.GetString("TITLE");
                    string city = e.IsDBNull("CITY") ? string.Empty : e.GetString("CITY");
                    int idCity = e.IsDBNull("ID_CITY") ? 0 : e.GetInt32("ID_CITY");

                    Hospitals.Add(new Hospital(idHospital, city, idCity, hospital));
                }
            });
        }

        /// <summary>
        /// Метод для ініціалізації переліку лікарів (у контексті лікарні) вказаного у <see cref="idHospital"/>
        /// </summary>
        /// <param name="idHospital"></param>
        private void InitDoctors(int idHospital)
        {
            Doctors.Clear();

            string sqlDoctors = $@"SELECT
                  D.ID_DOCTOR,
                  S.NAME_SPECIALITY,
                  D.PHONE_NUM,
                  D.FULL_NAME,
                  AD.STREET,
                  AD.BUILD_NUMBER,
                  CT.TITLE CATEGORY,
                  D.VISITABLE
                FROM DOCTORS D
                  LEFT JOIN ADDRESSES AD
                    ON D.ID_ADDRESS = AD.ID_ADDRESS
                  LEFT JOIN SPECIALITIES S
                    ON D.SPECIALITY = S.ID_SPECIALITY
                  LEFT JOIN CATEGORIES CT
                    ON D.ID_CATEGORY = CT.ID_CATEGORY
                WHERE AD.ID_HOSPITAL = {idHospital}";

            var error = DBHelper.ExecuteReader(sqlDoctors, e =>
            {
                while (e.Read())
                {
                    int idDoctor = e.IsDBNull("ID_DOCTOR") ? 0 : e.GetInt32("ID_DOCTOR");
                    string fullName = e.IsDBNull("FULL_NAME") ? string.Empty : e.GetString("FULL_NAME");
                    string speciality = e.IsDBNull("NAME_SPECIALITY") ? string.Empty : e.GetString("NAME_SPECIALITY");
                    string phoneNum = e.IsDBNull("PHONE_NUM") ? string.Empty : e.GetString("PHONE_NUM");
                    string street = e.IsDBNull("STREET") ? string.Empty : e.GetString("STREET");
                    string buildNumber = e.IsDBNull("BUILD_NUMBER") ? string.Empty : e.GetString("BUILD_NUMBER");
                    string category = e.IsDBNull("CATEGORY") ? string.Empty : e.GetString("CATEGORY");
                    bool visitable = e.IsDBNull("VISITABLE") ? false : e.GetBoolean("VISITABLE");

                    Doctors.Add(new Doctor(idDoctor, fullName, speciality, phoneNum, street, buildNumber, category, visitable));
                }
            });
        }

        private void Btn_AddHospital_Click(object sender, RoutedEventArgs e)
        {
            var pageHospital = new EditHospital();
            pageHospital.Show();
        }

        private void ListHospitals_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Btn_EditHospital.IsEnabled = ListHospitals.SelectedValue != null;
            Btn_DeleteHospital.IsEnabled = ListHospitals.SelectedValue != null;

            if (ListHospitals.SelectedValue == null)
            {
                return;
            }

            var hospital = (Hospital)ListHospitals.SelectedValue;

            InitDoctors(hospital.IdHospital);
        }

        private void Btn_EditHospital_Click(object sender, RoutedEventArgs e)
        {
            if (ListHospitals.SelectedValue == null)
            {
                return;
            }

            var hospital = (Hospital)ListHospitals.SelectedValue;

            var pageEdit = new EditHospital(hospital);
            pageEdit.Show();

            pageEdit.Closed += (s, e) =>
            {
                InitHospitals();
            };
        }
    }
}