using LenkasLittleHelper.Database;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LenkasLittleHelper
{
    public partial class ReportEdit : Window
    {
        public ReportEdit()
        {
            InitializeComponent();
        }

        private void Report_Add_Click(object sender, RoutedEventArgs e)
        {
            string sql = "INSERT INTO REPORTS (REPORT_TITLE,DT_CREATE) VALUES (@title,@dt)";

            Dictionary<string, object> cmdParams = new()
            {
                {"title",ReportName.Text},
                {"dt",DateTime.Now.Ticks}
            };

            var error = DBHelper.DoCommand(sql, cmdParams);

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }

            Close();
        }
    }
}