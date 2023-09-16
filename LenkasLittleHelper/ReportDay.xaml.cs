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
        public ReportDay(int idReport)
        {
            InitializeComponent();
            IdReport = idReport;
        }

        private void Report_Dt_Add_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object?>? cmdParams = new()
            {
                {"idReport",IdReport},
                {"dt",ReportDay_Field.SelectedDate?.Ticks}
            };

            string sql = "INSERT INTO REPORT_DAYS (ID_REPORT,DAY) VALUES (@idReport,@dt)";

            var error = DBHelper.DoCommand(sql, cmdParams);

            Close();
        }
    }
}