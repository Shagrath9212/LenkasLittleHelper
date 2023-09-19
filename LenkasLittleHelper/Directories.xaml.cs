using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for Directories.xaml
    /// </summary>
    public partial class Directories : Window
    {
        public ObservableCollection<Hospital_Directories> Hospitals { get; } = new ObservableCollection<Hospital_Directories>();
        private ObservableCollection<Doctor_Directory> Doctors { get; set; } = new ObservableCollection<Doctor_Directory>();

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

            StringBuilder sqlHospitals = new(@"SELECT H.ID_HOSPITAL, 
                    H.TITLE, 
                    H.IS_ARCHIVED, 
                    C.TITLE CITY,
                    C.ID_CITY 
                    FROM HOSPITALS H LEFT JOIN CITIES C ON H.ID_CITY=C.ID_CITY ");

            if (!ShowArch.IsChecked ?? false)
            {
                sqlHospitals.Append(" WHERE H.IS_ARCHIVED=0 ");
            }

            sqlHospitals.Append("ORDER BY H.IS_ARCHIVED, C.TITLE");

            var error = DBHelper.ExecuteReader(sqlHospitals.ToString(), e =>
            {
                while (e.Read())
                {
                    int idHospital = e.IsDBNull("ID_HOSPITAL") ? 0 : e.GetInt32("ID_HOSPITAL");
                    string hospital = e.IsDBNull("TITLE") ? string.Empty : e.GetString("TITLE");
                    string city = e.IsDBNull("CITY") ? string.Empty : e.GetString("CITY");
                    int idCity = e.IsDBNull("ID_CITY") ? 0 : e.GetInt32("ID_CITY");

                    bool isArchived = e.GetValueOrDefault<bool>("IS_ARCHIVED");

                    Hospitals.Add(new Hospital_Directories(idHospital, city, idCity, hospital, isArchived));
                }
            });

            if (error != null)
            {
                MessageBox.Show(error, "Помилка при завантаженні!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Hospitals.Add(new Hospital_Directories(-1, "Всі міста", -1, "Всі лікарні", false, true));
        }

        /// <summary>
        /// Метод для ініціалізації переліку лікарів (у контексті лікарні) вказаного у <see cref="idHospital"/>
        /// </summary>
        /// <param name="idHospital"></param>
        private void InitDoctors()
        {
            var hospital = (Hospital_Directories)ListHospitals.SelectedValue;

            Btn_DeleteHospital.IsEnabled = hospital != null && hospital.IdHospital != -1;

            Btn_Add_Doctor.IsEnabled = hospital != null && hospital.IdHospital != -1;

            if (hospital == null)
            {
                return;
            }

            if (hospital.IsArchived)
            {
                Btn_DeleteHospital.Content = "Повернути із архіву";
            }
            else
            {
                Btn_DeleteHospital.Content = "У архів";
            }

            Doctors.Clear();

            string? sqlDoctors = null;

            if (!hospital.DoLoadAll)
            {
                ColCity.Width = 0;
                ColHospital.Width = 0;

                sqlDoctors = $@"SELECT
                                  D.ID_DOCTOR,
                                  D.SPECIALITY,
                                  D.ID_ADDRESS,
                                  C.TITLE CITY,
                                  H.TITLE NAME_HOSPITAL,
                                  D.ID_CATEGORY,
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
                                  LEFT JOIN HOSPITALS H
                                    ON AD.ID_HOSPITAL = H.ID_HOSPITAL
                                  LEFT JOIN CITIES C
                                    ON H.ID_CITY = C.ID_CITY
                                   WHERE AD.ID_HOSPITAL = {hospital.IdHospital} 
                                  ORDER BY C.TITLE";

                //sqlDoctors = $@"SELECT
                //  D.ID_DOCTOR,
                //  D.SPECIALITY,
                //  D.ID_ADDRESS,
                //  NULL CITY,
                //  NULL NAME_HOSPITAL,
                //  D.ID_CATEGORY,
                //  S.NAME_SPECIALITY,
                //  D.PHONE_NUM,
                //  D.FULL_NAME,
                //  AD.STREET,
                //  AD.BUILD_NUMBER,
                //  CT.TITLE CATEGORY,
                //  D.VISITABLE
                //FROM DOCTORS D
                //  LEFT JOIN ADDRESSES AD
                //    ON D.ID_ADDRESS = AD.ID_ADDRESS
                //  LEFT JOIN SPECIALITIES S
                //    ON D.SPECIALITY = S.ID_SPECIALITY
                //  LEFT JOIN CATEGORIES CT
                //    ON D.ID_CATEGORY = CT.ID_CATEGORY
                //WHERE AD.ID_HOSPITAL = {hospital.IdHospital}";
            }
            else
            {
                ColCity.Width = 100;
                ColHospital.Width = 300;

                sqlDoctors = $@"SELECT
                                  D.ID_DOCTOR,
                                  D.SPECIALITY,
                                  D.ID_ADDRESS,
                                  C.TITLE CITY,
                                  H.TITLE NAME_HOSPITAL,
                                  D.ID_CATEGORY,
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
                                  LEFT JOIN HOSPITALS H
                                    ON AD.ID_HOSPITAL = H.ID_HOSPITAL
                                  LEFT JOIN CITIES C
                                    ON H.ID_CITY = C.ID_CITY
                                  ORDER BY C.TITLE";
            }

            var error = DBHelper.ExecuteReader(sqlDoctors, e =>
            {
                while (e.Read())
                {
                    int idDoctor = e.IsDBNull("ID_DOCTOR") ? 0 : e.GetInt32("ID_DOCTOR");
                    string fullName = e.IsDBNull("FULL_NAME") ? string.Empty : e.GetString("FULL_NAME");
                    string speciality = e.IsDBNull("NAME_SPECIALITY") ? string.Empty : e.GetString("NAME_SPECIALITY");
                    string phoneNum = e.IsDBNull("PHONE_NUM") ? string.Empty : e.GetString("PHONE_NUM");
                    string street = e.IsDBNull("STREET") ? string.Empty : e.GetString("STREET");
                    string? city = e.GetValueOrDefault<string>("CITY");
                    string? nameHospital = e.GetValueOrDefault<string>("NAME_HOSPITAL");
                    string buildNumber = e.IsDBNull("BUILD_NUMBER") ? string.Empty : e.GetString("BUILD_NUMBER");
                    string category = e.IsDBNull("CATEGORY") ? string.Empty : e.GetString("CATEGORY");
                    bool visitable = e.IsDBNull("VISITABLE") ? false : e.GetBoolean("VISITABLE");

                    int idSpecialiity = e.GetValueOrDefault<int>("SPECIALITY");
                    int idAddress = e.GetValueOrDefault<int>("ID_ADDRESS");
                    int idCategory = e.GetValueOrDefault<int>("ID_CATEGORY");

                    Doctors.Add(new Doctor_Directory(idDoctor, fullName, speciality, phoneNum, street, buildNumber, category, visitable, idSpecialiity, idCategory, idAddress, city, nameHospital));
                }
            });
        }

        private void Btn_AddHospital_Click(object sender, RoutedEventArgs e)
        {
            var pageHospital = new EditHospital();
            pageHospital.Show();

            pageHospital.Closed += (s, e) =>
            {
                InitHospitals();
            };
        }

        private void ListHospitals_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListHospitals.SelectedValue == null)
            {
                Btn_EditHospital.IsEnabled = false;
                Btn_DeleteHospital.IsEnabled = false;
                return;
            }

            var hospital = (Hospital_Directories)ListHospitals.SelectedValue;

            Btn_EditHospital.IsEnabled = !hospital.DoLoadAll;
            Btn_DeleteHospital.IsEnabled = !hospital.DoLoadAll;

            SearchDoctor.Text = string.Empty;
            InitDoctors();
        }

        private void Btn_EditHospital_Click(object sender, RoutedEventArgs e)
        {
            if (ListHospitals.SelectedValue == null)
            {
                return;
            }

            var hospital = (Hospital_Directories)ListHospitals.SelectedValue;

            var pageEdit = new EditHospital(hospital);
            pageEdit.Show();

            pageEdit.Closed += (s, e) =>
            {
                InitHospitals();
            };
        }

        private void Btn_DeleteHospital_Click(object sender, RoutedEventArgs e)
        {
            if (ListHospitals.SelectedValue == null)
            {
                return;
            }

            var hospital = (Hospital_Directories)ListHospitals.SelectedValue;

            if (hospital == null)
            {
                return;
            }

            string sql = $"UPDATE HOSPITALS SET IS_ARCHIVED={Convert.ToInt32(!hospital.IsArchived)} WHERE ID_HOSPITAL={hospital.IdHospital}";

            DBHelper.DoCommand(sql);

            InitHospitals();
        }

        private void ShowArch_Checked(object sender, RoutedEventArgs e)
        {
            InitHospitals();
        }

        private void ShowArch_Doctors_Checked(object sender, RoutedEventArgs e)
        {
            InitDoctors();
        }

        private void DoArchiveDoctor_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Add_Doctor_Click(object sender, RoutedEventArgs e)
        {
            if (ListHospitals.SelectedValue == null)
            {
                return;
            }

            var hospital = (Hospital_Directories)ListHospitals.SelectedValue;

            Directory_AddEditDoctor? window = null;

            var doctor = (Doctor_Directory)ListDoctors.SelectedValue;

            if (ListDoctors.SelectedValue == null)
            {
                window = new Directory_AddEditDoctor(hospital.Title, hospital.IdHospital);
            }
            else
            {
                window = new Directory_AddEditDoctor(doctor.GetIDHospital(), doctor);
            }

            window.Show();

            window.Closed += (s, e) =>
            {
                InitDoctors();
            };
        }

        private void ListDoctors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Btn_EditDoctor.IsEnabled = ListDoctors.SelectedValue != null;
            Btn_ArchiveDoctor.IsEnabled = ListDoctors.SelectedValue != null;
        }

        private void SearchDoctor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchDoctor.Text) || SearchDoctor.Text.Length < 4)
            {
                ListDoctors.ItemsSource = Doctors;
                return;
            }

            var filterText = SearchDoctor.Text.ToLower();

            var filtered = Doctors.Where(d =>
            (!string.IsNullOrEmpty(d.FullName) && d.FullName.ToLower().IndexOf(filterText) != -1)
            || (!string.IsNullOrEmpty(d.Speciality) && d.Speciality.ToLower().IndexOf(filterText) != -1)
            || (!string.IsNullOrEmpty(d.Speciality) && d.Speciality.ToLower().IndexOf(filterText) != -1)
            || (!string.IsNullOrEmpty(d.City) && d.City.ToLower().IndexOf(filterText) != -1)
            || (!string.IsNullOrEmpty(d.Address) && d.Address.ToLower().IndexOf(filterText) != -1)
            );

            ListDoctors.ItemsSource = filtered;
        }
    }
}