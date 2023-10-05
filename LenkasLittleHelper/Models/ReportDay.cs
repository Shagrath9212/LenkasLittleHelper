using LenkasLittleHelper.Helpers;
using System;
using System.Collections.Generic;

namespace LenkasLittleHelper.Models
{
    public class ReportDay : VM_WithNotify
    {
        private static readonly Dictionary<DayOfWeek, string> DaysOfWeek = new()
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

        public int CntDoctors { get => cntDoctors; private set => SetProperty(ref cntDoctors, value); }
        public int cntDoctors;

        public int CntPharmacies { get => cntPharmacies; private set => SetProperty(ref cntPharmacies, value); }
        public int cntPharmacies;

        public void UpdateCounter(int countDoctors, int countPharmacies)
        {
            CntDoctors = countDoctors;
            CntPharmacies = countPharmacies;
        }

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