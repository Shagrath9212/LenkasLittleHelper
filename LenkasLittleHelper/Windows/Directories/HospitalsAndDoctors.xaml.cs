using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace LenkasLittleHelper.Directories
{
    /// <summary>
    /// Interaction logic for HospitalsAndDoctors.xaml
    /// </summary>
    public partial class HospitalsAndDoctors : Window
    {
        public HospitalsAndDoctors()
        {
            InitializeComponent();

            ListHospitals.ItemsSource = Hospitals;
            ListDoctors.ItemsSource = Doctors;
            InitHospitals();
        }

        private ObservableCollection<Hospital_Directories> Hospitals { get; } = new ObservableCollection<Hospital_Directories>();
        private ObservableCollection<Doctor_Directory> Doctors { get; } = new ObservableCollection<Doctor_Directory>();

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
                    FROM HOSPITALS H 
                        LEFT JOIN CITIES C 
                    ON H.ID_CITY=C.ID_CITY ");

            if (!ShowArch.IsChecked ?? false)
            {
                sqlHospitals.Append(" WHERE H.IS_ARCHIVED=0 AND C.IS_ARCHIVED=0 ");
            }

            sqlHospitals.Append("ORDER BY H.IS_ARCHIVED, C.TITLE");

            var error = DBHelper.ExecuteReader(sqlHospitals.ToString(), e =>
            {
                while (e.Read())
                {
                    int idHospital = e.GetValueOrDefault<int>("ID_HOSPITAL");
                    var hospital = e.GetValueOrDefault<string>("TITLE");
                    var city = e.GetValueOrDefault<string>("CITY");
                    int idCity = e.GetValueOrDefault<int>("ID_CITY");

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
                                  D.VISITABLE,
                                  D.IS_ARCHIVED
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
                                   WHERE AD.ID_HOSPITAL = {hospital.IdHospital} ";

                if (!ShowArch_Doctors.IsChecked.HasValue || !ShowArch_Doctors.IsChecked.Value)
                {
                    sqlDoctors += @"AND D.IS_ARCHIVED=0 ";
                }

                sqlDoctors += "ORDER BY C.TITLE";
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
                                  D.VISITABLE,
                                  D.IS_ARCHIVED
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
                                    ON H.ID_CITY = C.ID_CITY ";

                if (!ShowArch_Doctors.IsChecked.HasValue || !ShowArch_Doctors.IsChecked.Value)
                {
                    sqlDoctors += @"AND D.IS_ARCHIVED=0 ";
                }

                sqlDoctors += "ORDER BY C.TITLE";
            }

            var error = DBHelper.ExecuteReader(sqlDoctors, e =>
            {
                while (e.Read())
                {
                    int idDoctor = e.GetValueOrDefault<int>("ID_DOCTOR");
                    var fullName = e.GetValueOrDefault<string>("FULL_NAME");
                    var speciality = e.GetValueOrDefault<string>("NAME_SPECIALITY");
                    var phoneNum = e.GetValueOrDefault<string>("PHONE_NUM");
                    var street = e.GetValueOrDefault<string>("STREET");
                    string? city = e.GetValueOrDefault<string>("CITY");
                    string? nameHospital = e.GetValueOrDefault<string>("NAME_HOSPITAL");
                    var buildNumber = e.GetValueOrDefault<string>("BUILD_NUMBER");
                    var category = e.GetValueOrDefault<string>("CATEGORY");
                    bool visitable = e.GetValueOrDefault<bool>("VISITABLE");

                    int idSpecialiity = e.GetValueOrDefault<int>("SPECIALITY");
                    int idAddress = e.GetValueOrDefault<int>("ID_ADDRESS");
                    int idCategory = e.GetValueOrDefault<int>("ID_CATEGORY");
                    bool isArchived = e.GetValueOrDefault<bool>("IS_ARCHIVED");

                    Doctors.Add(new Doctor_Directory(idDoctor, fullName, speciality, phoneNum, street, buildNumber, category, visitable, idSpecialiity, idCategory, idAddress, city, nameHospital, isArchived));
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
                Btn_Add_Doctor.IsEnabled = false;
                Doctors.Clear();
                return;
            }

            var hospital = (Hospital_Directories)ListHospitals.SelectedValue;

            Btn_EditHospital.IsEnabled = !hospital.DoLoadAll;
            Btn_DeleteHospital.IsEnabled = !hospital.DoLoadAll;
            Btn_Add_Doctor.IsEnabled = !hospital.DoLoadAll;

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

            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }

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
            if (ListDoctors.SelectedItem is not Doctor_Directory doctor)
            {
                return;
            }

            string sql = $"UPDATE DOCTORS SET IS_ARCHIVED={Convert.ToInt32(!doctor.IsArchived)} WHERE ID_DOCTOR={doctor.IdDoctor}";

            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }

            InitDoctors();
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
