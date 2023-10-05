using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using LenkasLittleHelper.Windows.Report;
using static LenkasLittleHelper.MainEnv;
using System.Text;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for ReportHelper.xaml
    /// </summary>
    public partial class ReportHelper : Window
    {
        private ObservableCollection<Report> Reports { get; set; } = new();
        private ObservableCollection<Models.ReportDay> ReportsDays { get; set; } = new();
        private ObservableCollection<Models.ReportDay> ReportsDays_Plan { get; set; } = new();
        private ObservableCollection<ReportCity> ReportsCities { get; set; } = new();
        private ObservableCollection<ReportHospital> ReportHospitals { get; set; } = new();
        private ObservableCollection<PharmacyReport> ReportPharmacies { get; set; } = new();
        private ObservableCollection<Doctor_Report> ReportDoctors { get; set; } = new();

        private readonly Dictionary<int, Action> Controls = new();
        #region Функції для очистки даних та вимкнення кнопок
        private void LevelOne()
        {
            ReportsDays.Clear();
            ReportsDays_Plan.Clear();
            Btn_MakeExcel.IsEnabled = false;
            Btn_AddDate.IsEnabled = false;
            Btn_AddDatePlan.IsEnabled = false;
        }

        private void LevelTwo()
        {
            ReportsCities.Clear();
            Btn_AddCity.IsEnabled = false;
            Btn_EditDay.IsEnabled = false;
            Btn_EditDayPlan.IsEnabled = false;
            Btn_DeleteDay.IsEnabled = false;
            Btn_DeleteDayPlan.IsEnabled = false;
        }
        private void LevelThree()
        {
            ReportHospitals.Clear();
            ReportPharmacies.Clear();
            Btn_AddHospital.IsEnabled = false;
            Btn_AddPharmacy.IsEnabled = false;
            Btn_DeleteCity.IsEnabled = false;
        }

        private void LevelFour()
        {
            ReportDoctors.Clear();
            Btn_AddDoctor.IsEnabled = false;
            Btn_DeleteHospital.IsEnabled = false;
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
            ListDailyReportsPlan.ItemsSource = ReportsDays_Plan;
            ListCitiesReport.ItemsSource = ReportsCities;
            ListHospitalsReport.ItemsSource = ReportHospitals;
            ListPharmaciesReport.ItemsSource = ReportPharmacies;
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

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }
        }

        private void RefreshDailyCounters()
        {
            if (ListReports.SelectedItem is not Report report)
            {
                return;
            }
            string sql = @$"SELECT 
                      RD.ID_REPORT_DAY, 
                      (
                        SELECT 
                          COUNT(1) 
                        FROM 
                          REPORT_DOCTORS RDO 
                          LEFT JOIN REPORT_HOSPITALS RH ON RDO.ID_REPORT_HOSPITAL = RH.ID_REPORT_HOSPITAL 
                          LEFT JOIN REPORT_CITIES RC ON RH.ID_REPORT_CITY = RC.ID_REPORT_CITY 
                        WHERE 
                          RC.ID_REPORT_DAY = RD.ID_REPORT_DAY
                      ) CNT_DOCTORS, 
                      (
                        SELECT 
                          COUNT(1) 
                        FROM 
                          REPORT_PHARMACIES RP 
                          LEFT JOIN REPORT_CITIES RC1 ON RP.ID_REPORT_CITY = RC1.ID_REPORT_CITY 
                        WHERE 
                          RC1.ID_REPORT_DAY = RD.ID_REPORT_DAY
                      ) CNT_PHARMACIES 
                    FROM 
                      REPORT_DAYS RD 
                    WHERE 
                      RD.ID_REPORT = {report.IdReport}";

            //Key-IdReport. Value (кортеж) 1-кількість лікарів, 2-кількість аптек
            Dictionary<int, (int, int)> counts = new();

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportCity = e.GetValueOrDefault<int>("ID_REPORT_DAY");
                    int countDoctors = e.GetValueOrDefault<int>("CNT_DOCTORS");
                    int countPharmacies = e.GetValueOrDefault<int>("CNT_PHARMACIES");

                    counts.TryAdd(idReportCity, (countDoctors, countPharmacies));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                ShowErrorDlg(error);
                return;
            }

            foreach (var day in ReportsDays)
            {
                if (!counts.TryGetValue(day.IdReportDay, out var counts_))
                {
                    day.UpdateCounter(0, 0);
                    continue;
                }

                day.UpdateCounter(counts_.Item1, counts_.Item2);
            }

            foreach (var day in ReportsDays_Plan)
            {
                if (!counts.TryGetValue(day.IdReportDay, out var counts_))
                {
                    day.UpdateCounter(0, 0);
                    continue;
                }

                day.UpdateCounter(counts_.Item1, counts_.Item2);
            }
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
                Btn_AddDatePlan.IsEnabled = false;
                Btn_MakeExcel.IsEnabled = false;
                DisableNestedControls(1);
                return;
            }

            Btn_AddDate.IsEnabled = true;
            Btn_AddDatePlan.IsEnabled = true;
            Btn_MakeExcel.IsEnabled = true;

            LoadDayReports_Fact(report.IdReport);
            LoadDayReports_Plan(report.IdReport);
            RefreshDailyCounters();
        }

        private void Btn_MakeExcel_Click(object sender, RoutedEventArgs e)
        {
            var report = (Report)ListReports.SelectedItem;

            if (report == null)
            {
                return;
            }


            Microsoft.Win32.SaveFileDialog saveFileDialog = new()
            {
                FileName = MakeReportFileName(report.IdReport),
                DefaultExt = ".xlsx"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
            {
                return;
            }

            MakeReport.CreateReport(report.IdReport, saveFileDialog.FileName);
        }

        /// <summary>
        /// Створює назву файлу для звіту по шаблону Прізвище_Ім'я_Звіт_dd(start)_MM(start)_dd(end)_MM(end)_План_dd(start)_MM(start)_dd(end)_MM(end)
        /// </summary>
        /// <param name="idReport"></param>
        private static string MakeReportFileName(int idReport)
        {
            string sql = $@"SELECT 
                  * 
                FROM 
                  (
                    SELECT 
                      MIN(RD.DAY) FACT_MIN, 
                      MAX(RD.DAY) FACT_MAX 
                    FROM 
                      REPORT_DAYS RD 
                    WHERE 
                      ID_REPORT = {idReport} 
                      AND REPORT_TYPE = {(int)ReportType.Fact}
                  ), 
                  (
                    SELECT 
                      MIN(RD.DAY) PLAN_MIN, 
                      MAX(RD.DAY) PLAN_MAX 
                    FROM 
                      REPORT_DAYS RD 
                    WHERE 
                      ID_REPORT = {idReport} 
                      AND REPORT_TYPE = {(int)ReportType.Plan}
                  )
                ";

            StringBuilder fileName = new("Станіславчук Олена_Звіт_");

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                if (e.Read())
                {
                    #region ФАКТ

                    DateTime factMin = e.GetValueOrDefault("FACT_MIN", DateTime.MinValue);
                    DateTime factMax = e.GetValueOrDefault("FACT_MAX", DateTime.MinValue);

                    if (factMin == DateTime.MinValue || factMax == DateTime.MinValue)
                    {
                        MessageBox.Show("Не вказано дат для фактичних звітів, поправиш ім'я файлу.");
                    }
                    else
                    {
                        if (factMin.Day < 10)
                        {
                            fileName.Append($"0{factMin.Day}");
                        }
                        else
                        {
                            fileName.Append(factMin.Day);
                        }

                        fileName.Append('_');

                        if (factMin.Month < 10)
                        {
                            fileName.Append($"0{factMin.Month}");
                        }
                        else
                        {
                            fileName.Append(factMin.Month);
                        }

                        fileName.Append('_');

                        if (factMax.Day < 10)
                        {
                            fileName.Append($"0{factMax.Day}");
                        }
                        else
                        {
                            fileName.Append(factMax.Day);
                        }

                        fileName.Append('_');

                        if (factMax.Month < 10)
                        {
                            fileName.Append($"0{factMax.Month}");
                        }
                        else
                        {
                            fileName.Append(factMax.Month);
                        }

                        fileName.Append('_');
                    }
                    #endregion

                    #region ПЛАН
                    DateTime planMin = e.GetValueOrDefault("PLAN_MIN", DateTime.MinValue);
                    DateTime planMax = e.GetValueOrDefault("PLAN_MAX", DateTime.MinValue);

                    if (planMin == DateTime.MinValue || planMax == DateTime.MinValue)
                    {
                        MessageBox.Show("Не вказано дат для планових звітів, поправиш ім'я файлу.");
                    }
                    else
                    {
                        fileName.Append("План_");
                        if (planMin.Day < 10)
                        {
                            fileName.Append($"0{planMin.Day}");
                        }
                        else
                        {
                            fileName.Append(planMin.Day);
                        }

                        fileName.Append('_');

                        if (planMin.Month < 10)
                        {
                            fileName.Append($"0{planMin.Month}");
                        }
                        else
                        {
                            fileName.Append(planMin.Month);
                        }

                        fileName.Append('_');

                        if (planMax.Day < 10)
                        {
                            fileName.Append($"0{planMax.Day}");
                        }
                        else
                        {
                            fileName.Append(planMax.Day);
                        }

                        fileName.Append('_');

                        if (planMax.Month < 10)
                        {
                            fileName.Append($"0{planMax.Month}");
                        }
                        else
                        {
                            fileName.Append(planMax.Month);
                        }
                    }
                    #endregion
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                ShowErrorDlg(error);
            }

            return fileName.ToString();
        }

        private void Btn_deleteReport_Click(object sender, RoutedEventArgs e)
        {
            if (ListReports.SelectedItem is not Report report)
            {
                return;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show($"Видалити звіт {report.ReportName}?", "Попередження", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
            string sql = $"DELETE FROM REPORTS WHERE ID_REPORT={report.IdReport}";

            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }

            LoadReports();
        }
        #endregion

        #region Дати
        #region Факт

        private bool TryAutofillDate(int idReport, ReportType reportType)
        {
            string getMaxDateSql = @$"SELECT
                  MAX(day) MAX_DAY
                FROM REPORT_DAYS
                WHERE ID_REPORT = {idReport}
                AND REPORT_TYPE = {(int)reportType}";

            //при створенні нового дня-автоматично пропонується наступний день

            var proposedDate = DateTime.MinValue;

            var error = DBHelper.ExecuteReader(getMaxDateSql, e =>
            {
                if (e.Read())
                {
                    proposedDate = e.GetValueOrDefault("MAX_DAY", DateTime.MinValue);
                    if (proposedDate == DateTime.MinValue)
                    {
                        return;
                    }

                    proposedDate = proposedDate.Date.AddDays(1);
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }

            if (proposedDate != DateTime.MinValue)
            {
                var sql = $"INSERT INTO REPORT_DAYS (ID_REPORT,DAY,REPORT_TYPE) VALUES ({idReport},{proposedDate.Ticks + 1},{(int)reportType})";

                error = DBHelper.DoCommand(sql);

                if (!string.IsNullOrEmpty(error))
                {
                    MainEnv.ShowErrorDlg(error);
                    return false;
                }
                if (reportType == ReportType.Fact)
                {
                    LoadDayReports_Fact(idReport);
                }
                else
                {
                    LoadDayReports_Plan(idReport);
                }
                return true;
            }

            return false;
        }

        private void LoadDayReports_Fact(int idReport)
        {
            ReportsDays.Clear();

            string sql = $@"SELECT
              ID_REPORT_DAY,
              day,
              REPORT_TYPE
            FROM REPORT_DAYS
            WHERE ID_REPORT = {idReport} AND REPORT_TYPE={(int)MainEnv.ReportType.Fact} ORDER BY DAY";

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
            if (ListDailyReports.SelectedItem is not Models.ReportDay day
                || ListReports.SelectedItem is not Report report)
            {
                return;
            }

            var dt = new ReportDay(day);
            dt.Show();

            dt.Closed += (s, e) =>
            {
                LoadDayReports_Fact(report.IdReport);
            };
        }

        private void Btn_AddDate_Click(object sender, RoutedEventArgs e)
        {
            if (ListReports.SelectedItem is not Report reportCurrent)
            {
                return;
            }

            if (TryAutofillDate(reportCurrent.IdReport, ReportType.Fact))
            {
                return;
            }

            ReportDay dt = new(reportCurrent.IdReport, ReportType.Fact);
            dt.Show();

            dt.Closed += (s, e) =>
            {
                LoadDayReports_Fact(reportCurrent.IdReport);
            };
        }

        private void Btn_DeleteDay_Click(object sender, RoutedEventArgs e)
        {
            if (ListDailyReports.SelectedItem is not Models.ReportDay day)
            {
                return;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show($"Видалити усі записи на дату {day.ReportDate_Str}?", "Попередження", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
            string sql = $"DELETE FROM REPORT_DAYS WHERE ID_REPORT_DAY={day.IdReportDay}";

            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Помилка при видаленні!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (ListReports.SelectedItem is Report report)
            {
                LoadDayReports_Fact(report.IdReport);
            }
        }

        private void ListDailyReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListDailyReports.SelectedItem is not Models.ReportDay day)
            {
                if (ListDailyReportsPlan.SelectedItem == null)
                {
                    ReportTypeLbl.Content = null;
                    Btn_AddCity.IsEnabled = false;
                    DisableNestedControls(2);
                    return;
                }
                return;
            }

            ReportTypeLbl.Content = "!!!ФАКТ!!!";

            Btn_AddCity.IsEnabled = true;
            Btn_DeleteDay.IsEnabled = true;
            Btn_DeleteDayPlan.IsEnabled = false;
            Btn_EditDay.IsEnabled = true;
            Btn_EditDayPlan.IsEnabled = false;

            ListDailyReportsPlan.UnselectAll();

            LoadCities(day.IdReportDay);
        }

        #endregion

        #region План

        private void ListDailyReportsPlan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListDailyReportsPlan.SelectedItem is not Models.ReportDay day)
            {
                if (ListDailyReports.SelectedItem == null)
                {
                    ReportTypeLbl.Content = null;
                    Btn_AddCity.IsEnabled = false;
                    DisableNestedControls(2);
                    return;
                }
                return;
            }

            Btn_AddCity.IsEnabled = true;
            Btn_DeleteDay.IsEnabled = false;
            Btn_DeleteDayPlan.IsEnabled = true;
            Btn_EditDay.IsEnabled = false;
            Btn_EditDayPlan.IsEnabled = true;

            ReportTypeLbl.Content = "!!!ПЛАН!!!";

            ListDailyReports.UnselectAll();

            LoadCities(day.IdReportDay);
        }

        private void LoadDayReports_Plan(int idReport)
        {
            ReportsDays_Plan.Clear();

            string sql = $@"SELECT
              ID_REPORT_DAY,
              day,
              REPORT_TYPE
            FROM REPORT_DAYS
            WHERE ID_REPORT = {idReport} AND REPORT_TYPE={(int)ReportType.Plan} ORDER BY DAY";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportDay = e.GetValueOrDefault<int>("ID_REPORT_DAY");
                    DateTime day = e.GetValueOrDefault<DateTime>("DAY");

                    ReportsDays_Plan.Add(new(idReportDay, day));
                }
            });
        }

        private void Btn_AddDatePlan_Click(object sender, RoutedEventArgs e)
        {
            if (ListReports.SelectedItem is not Report reportCurrent)
            {
                return;
            }

            if (TryAutofillDate(reportCurrent.IdReport, ReportType.Plan))
            {
                return;
            }

            ReportDay dt = new(reportCurrent.IdReport, ReportType.Plan);
            dt.Show();

            dt.Closed += (s, e) =>
            {
                LoadDayReports_Plan(reportCurrent.IdReport);
            };
        }

        private void Btn_EditDayPlan_Click(object sender, RoutedEventArgs e)
        {
            if (ListDailyReportsPlan.SelectedItem is not Models.ReportDay day
                || ListReports.SelectedItem is not Report report)
            {
                return;
            }

            var dt = new ReportDay(day);
            dt.Show();

            dt.Closed += (s, e) =>
            {
                LoadDayReports_Plan(report.IdReport);
            };
        }

        private void Btn_DeleteDayPlan_Click(object sender, RoutedEventArgs e)
        {
            if (ListDailyReportsPlan.SelectedItem is not Models.ReportDay day)
            {
                return;
            }
            MessageBoxResult messageBoxResult = MessageBox.Show($"Видалити усі записи на дату {day.ReportDate_Str}?", "Попередження", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
            string sql = $"DELETE FROM REPORT_DAYS WHERE ID_REPORT_DAY={day.IdReportDay}";

            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Помилка при видаленні!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (ListDailyReportsPlan.SelectedItem is Report report)
            {
                LoadDayReports_Plan(report.IdReport);
            }
        }

        #endregion

        #endregion

        #region Міста

        /// <summary>
        /// Оновлення кількості лікарів, аптек та лікарень напроти кожного міста зі звіту
        /// </summary>
        /// <param name="idReportDay">Ідентифікатор дня звіту</param>
        private void RefreshCounts()
        {
            RefreshCitiesCount();
            RefreshDailyCounters();
        }

        private void RefreshCitiesCount()
        {
            int idReportDay = -1;
            if (ListDailyReports.SelectedItem is Models.ReportDay day)
            {
                idReportDay = day.IdReportDay;
            }

            if (idReportDay < 0)
            {
                if (ListDailyReportsPlan.SelectedItem is Models.ReportDay day_)
                {
                    idReportDay = day_.IdReportDay;
                }
            }

            if (idReportDay < 0)
            {
                return;
            }

            string sql = @$"SELECT
              ID_REPORT_CITY,
              (SELECT
                  COUNT(1)
                FROM REPORT_HOSPITALS RH
                  LEFT JOIN REPORT_DOCTORS RD
                    ON RH.ID_REPORT_HOSPITAL = RD.ID_REPORT_HOSPITAL
                WHERE RH.ID_REPORT_CITY = RC.ID_REPORT_CITY AND RD.ID_REPORT_DOCTOR IS NOT NULL) CNT_DOCTORS,
              (SELECT
                  COUNT(1)
                FROM REPORT_PHARMACIES RP
                WHERE RP.ID_REPORT_CITY = RC.ID_REPORT_CITY) CNT_PHARMACIES,
              (SELECT
                  COUNT(1)
                FROM REPORT_HOSPITALS RH
                WHERE RH.ID_REPORT_CITY = RC.ID_REPORT_CITY) CNT_HOSPITALS
            FROM REPORT_CITIES RC
            WHERE RC.ID_REPORT_DAY = {idReportDay}";

            //Key-IdReportCity. Value (кортеж) 1-кількість лікарів, 2-кількість аптек, 3-кількість лікарень
            Dictionary<int, (int, int, int)> counts = new();

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportCity = e.GetValueOrDefault<int>("ID_REPORT_CITY");
                    int countDoctors = e.GetValueOrDefault<int>("CNT_DOCTORS");
                    int countPharmacies = e.GetValueOrDefault<int>("CNT_PHARMACIES");
                    int countHospitals = e.GetValueOrDefault<int>("CNT_HOSPITALS");

                    counts.TryAdd(idReportCity, (countDoctors, countPharmacies, countHospitals));
                }
            });

            foreach (var city in ReportsCities)
            {
                if (!counts.TryGetValue(city.IdReportCity, out var counts_))
                {
                    city.UpdateCounter(0, 0, 0);
                    continue;
                }

                city.UpdateCounter(counts_.Item1, counts_.Item2, counts_.Item3);
            }
        }

        private void Btn_AddCity_Click(object sender, RoutedEventArgs e)
        {
            if (ListDailyReports.SelectedItem is not Models.ReportDay day)
            {
                if (ListDailyReportsPlan.SelectedItem is not Models.ReportDay day_)
                {
                    return;
                }

                day = day_;
            }

            var addCityDlg = new Report_CityEdit(day.IdReportDay, day.ReportDate_Str, ReportsCities.Select(e => e.Id));
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
                    AND C.IS_ARCHIVED=0 
                    ORDER BY RC.ID_REPORT_CITY DESC";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportCity = e.GetValueOrDefault<int>("ID_REPORT_CITY");
                    string? cityName = e.GetValueOrDefault<string>("TITLE");
                    int idCity = e.GetValueOrDefault<int>("ID_CITY");
                    ReportsCities.Add(new ReportCity(idCity, cityName, idReportCity));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }

            RefreshCounts();
        }

        private void ListCitiesReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var city = (ReportCity)ListCitiesReport.SelectedItem;
            if (city == null)
            {
                Btn_AddHospital.IsEnabled = false;
                Btn_AddPharmacy.IsEnabled = false;
                Btn_DeleteCity.IsEnabled = false;
                DisableNestedControls(3);
                return;
            }

            Btn_AddHospital.IsEnabled = true;
            Btn_AddPharmacy.IsEnabled = true;
            Btn_DeleteCity.IsEnabled = true;
            LoadHospitals(city.IdReportCity);
            LoadPharmacies(city.IdReportCity);
        }

        private void Btn_DeleteCity_Click(object sender, RoutedEventArgs e)
        {
            if (ListCitiesReport.SelectedItem is not ReportCity city)
            {
                return;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show($"Видалити усі записи по місту {city.CityName}?", "Попередження", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }
            string sql = $@"DELETE
                  FROM REPORT_CITIES
                WHERE ID_REPORT_CITY = {city.IdReportCity}";

            if (!string.IsNullOrEmpty(DBHelper.DoCommand(sql)))
            {
                return;
            }

            ReportsCities.Remove(city);
        }
        #endregion

        #region Лікарні
        private void LoadHospitals(int idReportCity)
        {
            ReportHospitals.Clear();

            string sql = $@"SELECT
                          RH.ID_REPORT_HOSPITAL,
                          RH.ID_HOSPITAL,
                          H.TITLE,
                          (SELECT COUNT(1) FROM REPORT_DOCTORS WHERE ID_REPORT_HOSPITAL=RH.ID_REPORT_HOSPITAL) CNT_DOCTORS
                        FROM REPORT_HOSPITALS RH
                          LEFT JOIN HOSPITALS H
                            ON RH.ID_HOSPITAL = H.ID_HOSPITAL
                        WHERE RH.ID_REPORT_CITY = {idReportCity}
                        ORDER BY RH.ID_REPORT_HOSPITAL DESC";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportHospital = e.GetValueOrDefault<int>("ID_REPORT_HOSPITAL");
                    int idHospital = e.GetValueOrDefault<int>("ID_HOSPITAL");
                    string? title = e.GetValueOrDefault<string>("TITLE");
                    int countDoctors = e.GetValueOrDefault<int>("CNT_DOCTORS");

                    var hospital = new ReportHospital(idReportHospital, idHospital, title);

                    hospital.UpdateCounter(countDoctors);

                    ReportHospitals.Add(hospital);
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }
        }

        private void ListHospitalsReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var hospital = (ReportHospital)ListHospitalsReport.SelectedItem;
            if (hospital == null)
            {
                Btn_AddDoctor.IsEnabled = false;
                Btn_DeleteHospital.IsEnabled = false;
                DisableNestedControls(4);
                return;
            }

            Btn_AddDoctor.IsEnabled = true;
            Btn_DeleteHospital.IsEnabled = true;
            LoadDoctors(hospital);
        }

        private void Btn_AddHospital_Click(object sender, RoutedEventArgs e)
        {
            if (ListCitiesReport.SelectedItem is not ReportCity city)
            {
                return;
            }

            var hospitalEdit = new Report_HospitalEdit(city.Id, city.IdReportCity, city.CityName, ReportHospitals.Select(e => e.IdHospital));
            hospitalEdit.Show();

            hospitalEdit.Closed += (s, e) =>
            {
                LoadHospitals(city.IdReportCity);
                RefreshCounts();
            };
        }

        private void Btn_DeleteHospital_Click(object sender, RoutedEventArgs e)
        {
            if (ListHospitalsReport.SelectedItem is not ReportHospital hospital)
            {
                return;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show($"Видалити усі записи по лікарні {hospital.Title}?", "Попередження", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            string sql = @$"DELETE
                      FROM REPORT_HOSPITALS
                    WHERE ID_REPORT_HOSPITAL = {hospital.IdReportHospital}";
            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                ShowErrorDlg(error);
                return;
            }

            ReportHospitals.Remove(hospital);
            RefreshCounts();
        }
        #endregion

        #region Лікарі
        private void LoadDoctors(ReportHospital hospital)
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
                            WHERE RD.ID_REPORT_HOSPITAL = {hospital.IdReportHospital}
                            ORDER BY ID_REPORT_DOCTOR DESC";

            var error = DBHelper.ExecuteReader(sql, e =>
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

            if (!string.IsNullOrEmpty(error))
            {
                ShowErrorDlg(error);
            }

            hospital.UpdateCounter(ReportDoctors.Count);

            RefreshCounts();
        }

        private void Btn_AddDoctor_Click(object sender, RoutedEventArgs e)
        {
            if (ListHospitalsReport.SelectedItem is not ReportHospital hospital)
            {
                return;
            }

            var reportEditDoctor = new Report_DoctorNew(hospital.IdHospital, hospital.IdReportHospital, hospital.Title, ReportDoctors.Select(e => e.IdDoctor));
            reportEditDoctor.Show();

            reportEditDoctor.Closed += (s, e) =>
            {
                LoadDoctors(hospital);
                RefreshCounts();
            };
        }

        private void Btn_DeleteDoctor_Click(object sender, RoutedEventArgs e)
        {
            if (ListDoctorsReport.SelectedItem is not Doctor_Report doctor
                || ListHospitalsReport.SelectedItem is not ReportHospital hospital)
            {
                return;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show($"Видалити лікаря {doctor.FullName}?", "Попередження", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            string sql = @$"DELETE
                      FROM REPORT_DOCTORS
                    WHERE ID_REPORT_DOCTOR = {doctor.IdReportDoctor}";
            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                return;
            }

            LoadDoctors(hospital);

            RefreshCounts();
        }

        private void ListDoctorsReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Btn_DeleteDoctor.IsEnabled = ListDoctorsReport.SelectedItem != null;
        }
        #endregion

        #region Аптеки

        private void ListPharmaciesReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Btn_DeletePharmacy.IsEnabled = ListPharmaciesReport.SelectedItem != null;
        }

        private void LoadPharmacies(int idReportCity)
        {
            ReportPharmacies.Clear();

            string sql = @$"SELECT
              RP.ID_REPORT_PHARMACY,
              RP.ID_PHARMACY,
              P.NAME_PHARMACY,
              P.STREET,
              P.BUILD_NUM
            FROM REPORT_PHARMACIES RP
              LEFT JOIN PHARMACIES P
                ON RP.ID_PHARMACY = P.ID_PHARMACY
            WHERE RP.ID_REPORT_CITY = {idReportCity}";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportPharmacy = e.GetValueOrDefault<int>("ID_REPORT_PHARMACY");
                    int idPharmacy = e.GetValueOrDefault<int>("ID_PHARMACY");
                    string? namePharmacy = e.GetValueOrDefault<string>("NAME_PHARMACY");
                    string? street = e.GetValueOrDefault<string>("STREET");
                    string? build = e.GetValueOrDefault<string>("BUILD_NUM");

                    ReportPharmacies.Add(new PharmacyReport(idReportPharmacy, idPharmacy, namePharmacy, street, build));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                ShowErrorDlg(error);
            }
        }

        private void Btn_AddPharmacy_Click(object sender, RoutedEventArgs e)
        {
            if (ListCitiesReport.SelectedItem is not ReportCity city)
            {
                return;
            }

            var addPharmacy = new Report_AddEditPharmacy(city.Id, city.CityName, city.IdReportCity, ReportPharmacies.Select(e => e.IdPharmacy));

            addPharmacy.Show();
            addPharmacy.Closed += (s, e) =>
            {
                LoadPharmacies(city.IdReportCity);
                RefreshCounts();
            };
        }

        private void Btn_DeletePharmacy_Click(object sender, RoutedEventArgs e)
        {
            if (ListPharmaciesReport.SelectedItem is not PharmacyReport pharmacyReport || ListCitiesReport.SelectedItem is not ReportCity city)
            {
                return;
            }

            string sql = $"DELETE FROM REPORT_PHARMACIES WHERE ID_REPORT_PHARMACY={pharmacyReport.IdReportPharmacy}";

            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                ShowErrorDlg(error);
            }

            LoadPharmacies(city.IdReportCity);

            RefreshCounts();
        }
        #endregion
    }
}