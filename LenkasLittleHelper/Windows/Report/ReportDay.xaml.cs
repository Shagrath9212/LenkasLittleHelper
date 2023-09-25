using LenkasLittleHelper.Database;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for ReportDay.xaml
    /// </summary>
    public partial class ReportDay : Window
    {
        private int IdReport { get; set; }
        private Models.ReportDay? DayReport { get; }
        private MainEnv.ReportType ReportType { get; }

        public ReportDay(int idReport, MainEnv.ReportType reportType)
        {
            InitializeComponent();
            ReportType = reportType;
            IdReport = idReport;
        }

        public ReportDay(Models.ReportDay day)
        {
            InitializeComponent();
            DayReport = day;
            ReportDay_Field.SelectedDate = day.Day;
        }

        private void Report_Dt_Add_Click(object sender, RoutedEventArgs e)
        {
            long dt = DateTime.Now.Ticks + 1;
            if (ReportDay_Field.SelectedDate.HasValue)
            {
                dt = ReportDay_Field.SelectedDate.Value.Ticks + 1;
            }

            Dictionary<string, object> cmdParams = new()
            {
                {"dt",dt}
            };

            string? sql;
            if (DayReport == null)
            {
                cmdParams.Add("idReport", IdReport);
                cmdParams.Add("reportType", (int)ReportType);
                sql = "INSERT INTO REPORT_DAYS (ID_REPORT,DAY,REPORT_TYPE) VALUES (@idReport,@dt,@reportType)";
            }
            else
            {
                cmdParams.Add("idReportDay", DayReport.IdReportDay);
                sql = @"UPDATE REPORT_DAYS
                    SET DAY = @dt
                    WHERE ID_REPORT_DAY = @idReportDay";
            }

            var error = DBHelper.DoCommand(sql, cmdParams);

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Помилка!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Close();
        }
    }
}