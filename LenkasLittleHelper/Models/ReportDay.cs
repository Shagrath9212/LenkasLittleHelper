using LenkasLittleHelper.Helpers;
using System;

namespace LenkasLittleHelper.Models
{
    public class ReportDay
    {
        public int IdReportDay { get; }
        public DateTime Day { get; }
        public string ReportDate_Str
        {
            get
            {
                return Day.ToDBFormat_DateOnly();
            }
        }
        public ReportDay(int idReportDay, DateTime day)
        {
            IdReportDay = idReportDay;
            Day = day;
        }
    }
}