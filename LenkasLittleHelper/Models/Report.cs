using LenkasLittleHelper.Helpers;
using System;

namespace LenkasLittleHelper.Models
{
    internal class Report
    {
        public int IdReport { get; }
        public string? ReportName { get; }
        public DateTime DtReport { get; }

        public MainEnv.ReportType ReportType { get; }

        public string ReportDate
        {
            get
            {
                return DtReport.ToDBFormat();
            }
        }

        public Report(int idReport, string? reportName, DateTime dtReport)
        {
            IdReport = idReport;
            ReportName = reportName;
            DtReport = dtReport;
        }
    }
}