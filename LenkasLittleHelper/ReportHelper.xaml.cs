using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for ReportHelper.xaml
    /// </summary>
    public partial class ReportHelper : Window
    {
        private ObservableCollection<Report> Reports { get; set; } = new();
        private ObservableCollection<Models.ReportDay> ReportsDays { get; set; } = new();
        private ObservableCollection<ReportCity> ReportsCities { get; set; } = new();
        private ObservableCollection<ReportHospital> ReportHospitals { get; set; } = new();
        private ObservableCollection<Doctor_Report> ReportDoctors { get; set; } = new();

        private readonly Dictionary<int, Action> Controls = new();
        #region Функції для очистки даних та вимкнення кнопок
        private void LevelOne()
        {
            ReportsDays.Clear();
            Btn_MakeExcel.IsEnabled = false;
            Btn_AddDate.IsEnabled = false;
            Btn_EditDay.IsEnabled = false;
            Btn_EditDay.IsEnabled = false;
        }

        private void LevelTwo()
        {
            ReportsCities.Clear();
            Btn_AddCity.IsEnabled = false;
        }
        private void LevelThree()
        {
            ReportHospitals.Clear();
            Btn_AddHospital.IsEnabled = false;
        }

        private void LevelFour()
        {
            ReportDoctors.Clear();
            Btn_AddDoctor.IsEnabled = false;
        }

        /// <summary>
        /// Функція для очистки списку та вимкнення кнопок по рівнях вкладеності
        /// </summary>
        private void DisableNestedControls(int level)
        {
            var actions = Controls.Where(k => k.Key >= level).Select(e => e.Value);

            foreach (var action in actions)
            {
                action();
            }
        }

        #endregion

        public ReportHelper()
        {
            InitializeComponent();
            ListReports.ItemsSource = Reports;
            ListDailyReports.ItemsSource = ReportsDays;
            ListCitiesReport.ItemsSource = ReportsCities;
            ListHospitalsReport.ItemsSource = ReportHospitals;
            ListDoctorsReport.ItemsSource = ReportDoctors;
            LoadReports();

            Controls.Add(1, LevelOne);
            Controls.Add(2, LevelTwo);
            Controls.Add(3, LevelThree);
            Controls.Add(4, LevelFour);
        }

        #region Звіти

        /// <summary>
        /// Завантаження усіх звітів
        /// </summary>
        private void LoadReports()
        {
            Reports.Clear();

            string sql = @"SELECT
                  ID_REPORT,
                  REPORT_TITLE,
                  DT_CREATE
                FROM REPORTS
                ORDER BY ID_REPORT";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReport = e.GetValueOrDefault<int>("ID_REPORT");
                    string? reportTitle = e.GetValueOrDefault<string>("REPORT_TITLE");
                    DateTime dtCreate = e.GetValueOrDefault<DateTime>("DT_CREATE");

                    Reports.Add(new Report(idReport, reportTitle, dtCreate));
                }
            });
        }

        private void Btn_AddReport_Click(object sender, RoutedEventArgs e)
        {
            var addNew = new ReportEdit();
            addNew.Show();
            addNew.Closed += (s, e) =>
            {
                LoadReports();
            };
        }

        private void ListReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var report = (Report)ListReports.SelectedItem;

            if (report == null)
            {
                Btn_AddDate.IsEnabled = false;
                DisableNestedControls(1);
                return;
            }

            Btn_AddDate.IsEnabled = true;
            Btn_MakeExcel.IsEnabled = true;

            LoadDayReports(report.IdReport);
        }
        #endregion

        #region Дати

        private void LoadDayReports(int idReport)
        {
            ReportsDays.Clear();

            string sql = $@"SELECT
              ID_REPORT_DAY,
              day
            FROM REPORT_DAYS
            WHERE ID_REPORT = {idReport} ORDER BY DAY";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportDay = e.GetValueOrDefault<int>("ID_REPORT_DAY");
                    DateTime day = e.GetValueOrDefault<DateTime>("DAY");
                    ReportsDays.Add(new(idReportDay, day));
                }
            });
        }

        private void Btn_EditDay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_AddDate_Click(object sender, RoutedEventArgs e)
        {
            var reportCurrent = (Report)ListReports.SelectedItem;
            var dt = new ReportDay(reportCurrent.IdReport);
            dt.Show();

            dt.Closed += (s, e) =>
            {
                LoadDayReports(reportCurrent.IdReport);
            };
        }

        private void Btn_DeleteDay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListDailyReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var day = (Models.ReportDay)ListDailyReports.SelectedItem;

            if (day == null)
            {
                Btn_AddCity.IsEnabled = false;
                DisableNestedControls(2);
                return;
            }

            Btn_AddCity.IsEnabled = true;
            LoadCities(day.IdReportDay);
        }
        #endregion

        #region Міста
        private void Btn_AddCity_Click(object sender, RoutedEventArgs e)
        {
            var day = (Models.ReportDay)ListDailyReports.SelectedItem;

            if (day == null)
            {
                return;
            }

            var addCityDlg = new Report_CityEdit(day.IdReportDay, day.ReportDate_Str);
            addCityDlg.Show();

            addCityDlg.Closed += (s, e) =>
            {
                LoadCities(day.IdReportDay);
            };
        }

        /// <summary>
        /// Завантаження переліку міст (по вказаному ID дня)/>
        /// </summary>
        /// <param name="idReportDay"></param>
        private void LoadCities(int idReportDay)
        {
            ReportsCities.Clear();

            string sql = $@"SELECT
                  RC.ID_REPORT_CITY,
                  C.TITLE,
                  RC.ID_CITY
                FROM REPORT_CITIES RC
                  LEFT JOIN CITIES C
                    ON RC.ID_CITY = C.ID_CITY
                WHERE RC.ID_REPORT_DAY = {idReportDay} 
                    ORDER BY RC.ID_REPORT_CITY DESC";

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportCity = e.GetValueOrDefault<int>("ID_REPORT_CITY");
                    string? cityName = e.GetValueOrDefault<string>("TITLE");
                    int idCity = e.GetValueOrDefault<int>("ID_CITY");
                    ReportsCities.Add(new ReportCity(idCity, cityName, idReportCity));
                }
            });
        }

        private void ListCitiesReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var city = (ReportCity)ListCitiesReport.SelectedItem;
            if (city == null)
            {
                Btn_AddHospital.IsEnabled = false;
                DisableNestedControls(3);
                return;
            }
            Btn_AddHospital.IsEnabled = true;
            LoadHospitals(city.IdReportCity);
        }
        #endregion

        #region Лікарні
        private void LoadHospitals(int idReportCity)
        {
            ReportHospitals.Clear();

            string sql = $@"SELECT
                          RH.ID_REPORT_HOSPITAL,
                          RH.ID_HOSPITAL,
                          H.TITLE
                        FROM REPORT_HOSPITALS RH
                          LEFT JOIN HOSPITALS H
                            ON RH.ID_HOSPITAL = H.ID_HOSPITAL
                        WHERE RH.ID_REPORT_CITY = {idReportCity}
                        ORDER BY RH.ID_REPORT_HOSPITAL DESC";

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportHospital = e.GetValueOrDefault<int>("ID_REPORT_HOSPITAL");
                    int idHospital = e.GetValueOrDefault<int>("ID_HOSPITAL");
                    string? title = e.GetValueOrDefault<string>("TITLE");

                    ReportHospitals.Add(new ReportHospital(idReportHospital, idHospital, title));
                }
            });
        }

        private void ListHospitalsReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var hospital = (ReportHospital)ListHospitalsReport.SelectedItem;
            if (hospital == null)
            {
                Btn_AddDoctor.IsEnabled = false;
                DisableNestedControls(4);
                return;
            }

            Btn_AddDoctor.IsEnabled = true;
            LoadDoctors(hospital.IdReportHospital);
        }

        private void Btn_AddHospital_Click(object sender, RoutedEventArgs e)
        {
            var city = (ReportCity)ListCitiesReport.SelectedItem;
            if (city == null)
            {
                return;
            }
            var hospitalEdit = new Report_HospitalEdit(city.Id, city.IdReportCity, city.CityName);
            hospitalEdit.Show();

            hospitalEdit.Closed += (s, e) =>
            {
                LoadHospitals(city.IdReportCity);
            };
        }
        #endregion

        #region Лікарі
        private void LoadDoctors(int idReportHospital)
        {
            ReportDoctors.Clear();

            string sql = $@"SELECT
                              D.ID_DOCTOR,
                              D.FULL_NAME,
                              S.NAME_SPECIALITY,
                              A.STREET,
                              A.BUILD_NUMBER,
                              RD.ID_REPORT_DOCTOR
                            FROM REPORT_DOCTORS RD
                              LEFT JOIN DOCTORS D
                                ON RD.ID_DOCTOR = D.ID_DOCTOR
                              LEFT JOIN ADDRESSES A
                                ON D.ID_ADDRESS = A.ID_ADDRESS
                              LEFT JOIN SPECIALITIES S
                                ON D.SPECIALITY = S.ID_SPECIALITY
                            WHERE RD.ID_REPORT_HOSPITAL = {idReportHospital}
                            ORDER BY ID_REPORT_DOCTOR DESC";

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    var idDoctor = e.GetValueOrDefault<int>("ID_DOCTOR");
                    var idReportDoctor = e.GetValueOrDefault<int>("ID_REPORT_DOCTOR");
                    var fullName = e.GetValueOrDefault<string>("FULL_NAME");
                    var nameSpeciality = e.GetValueOrDefault<string>("NAME_SPECIALITY");
                    var street = e.GetValueOrDefault<string>("STREET");
                    var buildNumber = e.GetValueOrDefault<string>("BUILD_NUMBER");

                    ReportDoctors.Add(new Doctor_Report(idReportDoctor, idDoctor, fullName, nameSpeciality, street, buildNumber));
                }
            });
        }

        private void Btn_AddDoctor_Click(object sender, RoutedEventArgs e)
        {
            var hospital = (ReportHospital)ListHospitalsReport.SelectedItem;
            if (hospital == null)
            {
                return;
            }
            var reportEditDoctor = new Report_EditDoctor(hospital.IdHospital, hospital.IdReportHospital, hospital.Title, ReportDoctors.Select(e => e.IdDoctor));
            reportEditDoctor.Show();

            reportEditDoctor.Closed += (s, e) =>
            {
                LoadDoctors(hospital.IdReportHospital);
            };
        }
        #endregion

        private void Btn_MakeExcel_Click(object sender, RoutedEventArgs e)
        {
            var report = (Report)ListReports.SelectedItem;

            if (report == null)
            {
                return;
            }
            MakeReport.CreateReport_Fact(report.IdReport);
        }
    }
}