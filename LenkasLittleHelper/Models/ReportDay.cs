using LenkasLittleHelper.Helpers;
using System;
using System.Collections.Generic;

namespace LenkasLittleHelper.Models
{
    public class ReportDay
    {
        private static Dictionary<DayOfWeek, string> DaysOfWeek = new()
        {
            {DayOfWeek.Sunday,"Неділя" },
            {DayOfWeek.Monday,"Понеділок" },
            {DayOfWeek.Tuesday,"Вівторок" },
            {DayOfWeek.Wednesday,"Середа" },
            {DayOfWeek.Thursday,"Четвер" },
            {DayOfWeek.Friday,"П'ятниця" },
            {DayOfWeek.Saturday,"Субота" }
        };

        public int IdReportDay { get; }
        public DateTime Day { get; }
        public string ReportDate_Str
        {
            get
            {
                return Day.ToDBFormat_DateOnly();
            }
        }
        public string ReportDate_DayOfWeek
        {
            get
            {
                return DaysOfWeek[Day.DayOfWeek];
            }
        }
        public ReportDay(int idReportDay, DateTime day)
        {
            IdReportDay = idReportDay;
            Day = day;
        }
    }
}