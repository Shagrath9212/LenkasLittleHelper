using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for ReportHelper.xaml
    /// </summary>
    public partial class ReportHelper : Window
    {
        private ObservableCollection<Report> Reports { get; set; } = new ObservableCollection<Report>();
        private ObservableCollection<Models.ReportDay> ReportsDay { get; set; } = new ObservableCollection<Models.ReportDay>();
        public ReportHelper()
        {
            InitializeComponent();
            ListReports.ItemsSource = Reports;
            ListDailyReports.ItemsSource = ReportsDay;
            LoadReports();
        }

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

        private void LoadDayReports(int idReport)
        {
            ReportsDay.Clear();

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
                    ReportsDay.Add(new(idReportDay, day));
                }
            });
        }

        /// <summary>
        /// Завантаження переліку лікарень (по днях)
        /// </summary>
        /// <param name="idReportDay"></param>
        private void LoadCties(int idReportDay)
        {
            string sql = "";
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

        #region Додавання/редагування/видалення дат звіту

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
        #endregion

        #region Міста
        private void Btn_AddCity_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private void ListReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var report = (Report)ListReports.SelectedItem;

            if (report == null)
            {
                Btn_AddDate.IsEnabled = false;
                return;
            }

            Btn_AddDate.IsEnabled = true;

            LoadDayReports(report.IdReport);
        }

        private void Btn_AddHospital_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListDailyReports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var day = (Models.ReportDay)ListDailyReports.SelectedItem;

            if (day == null)
            {
                Btn_AddCity.IsEnabled = false;
                return;
            }

            Btn_AddDate.IsEnabled = true;

            LoadDayReports(day.IdReportDay);
        }
    }
}