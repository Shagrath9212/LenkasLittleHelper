using LenkasLittleHelper.Database;
using LenkasLittleHelper.Helpers;
using System;
using System.Collections.Generic;
using NPOI.SS.Util;
using System.Linq;
using LenkasLittleHelper.Models;
using NPOI.SS.UserModel;
using System.Windows;
using static LenkasLittleHelper.MainEnv;

namespace LenkasLittleHelper.Windows.Report
{
    internal static class MakeReport
    {
        public static string Pathh = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public const string Template = "template.xlsx";


        public static void CreateReport(int idReport, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("Не вказано ім'я файлу!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (ExcelDocument? document = ExcelDocument.OpenFromTemplate(Template))
            {
                if (document == null)
                {
                    return;
                }

                var cellFont = document.CreateFont();

                cellFont.FontName = "Arial";
                cellFont.FontHeightInPoints = 9;

                var cellStyle = document.CreateCellStyle();
                cellStyle.SetFont(cellFont);

                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.WrapText = true;
                cellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;

                CreateReport_Doctors(document, cellStyle, idReport, MainEnv.ReportType.Fact);
                CreateReport_Doctors(document, cellStyle, idReport, MainEnv.ReportType.Plan);

                document.SaveAs(fileName);
            }
        }

        public static void CreateReport_Doctors(ExcelDocument document, ICellStyle cellStyle, int idReport, MainEnv.ReportType reportType)
        {
            var days = LoadDays(idReport, reportType);

            var rowStart = 0;

            string sheetName = reportType == MainEnv.ReportType.Plan ? "План врачи" : "Факт врачи";

            var sheet = document.GetSheet(sheetName);

            if (sheet == null)
            {
                return;
            }

            foreach (var day in days)
            {
                var rowDayStart = rowStart + 1;

                var rowCity = sheet.CreateRow(rowStart += 1);

                rowCity.CreateCell(0).SetCellValue(day.Value.ToDBFormat_DateOnly());

                rowCity.CreateCell(2).SetCellValue("Станіславчук Олена");

                var cities = LoadCities(day.Key);
                if (cities != null && !cities.Any() || cities == null)
                {
                    sheet.GetRow(rowStart).CreateCell(1).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(3).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(4).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(5).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                    continue;
                }

                bool isCityFirstIteration = true;

                foreach (var city in cities)
                {
                    var rowCityStart = rowStart + 1;
                    if (isCityFirstIteration)
                    {
                        rowCityStart -= 1;
                        sheet.GetRow(rowStart).CreateCell(1).SetCellValue(city.Value);
                        isCityFirstIteration = false;
                    }
                    else
                    {
                        sheet.CreateRow(rowStart += 1).CreateCell(1).SetCellValue(city.Value);
                    }

                    var hospitals = LoadHospitals(city.Key);

                    if (hospitals != null && !hospitals.Any() || hospitals == null)
                    {
                        sheet.GetRow(rowStart).CreateCell(3).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(4).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(5).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                        continue;
                    }

                    bool isHospitalFirstIteration = true;

                    foreach (var hospital in hospitals)
                    {
                        var rowHospitalStart = rowStart + 1;
                        if (isHospitalFirstIteration)
                        {
                            rowHospitalStart -= 1;
                            sheet.GetRow(rowStart).CreateCell(3).SetCellValue(hospital.Value);
                            isHospitalFirstIteration = false;
                        }
                        else
                        {
                            sheet.CreateRow(rowStart += 1).CreateCell(3).SetCellValue(hospital.Value);
                        }

                        var buildingsNDoctors = LoadBuildingsAndDoctors(hospital.Key);

                        if (buildingsNDoctors == null || !buildingsNDoctors.Any())
                        {
                            sheet.GetRow(rowStart).CreateCell(4).SetBlank();
                            sheet.GetRow(rowStart).CreateCell(5).SetBlank();
                            sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                            sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                            continue;
                        }

                        bool isBuildingFirstIteration = true;

                        foreach (var building in buildingsNDoctors)
                        {
                            var rowBuildStart = rowStart + 1;
                            if (isBuildingFirstIteration)
                            {
                                rowBuildStart -= 1;
                                sheet.GetRow(rowStart).CreateCell(4).SetCellValue(building.Street);
                                sheet.GetRow(rowStart).CreateCell(5).SetCellValue(building.NumBuilding);
                                isBuildingFirstIteration = false;
                            }
                            else
                            {
                                var row = sheet.CreateRow(rowStart += 1);
                                row.CreateCell(4).SetCellValue(building.Street);
                                row.CreateCell(5).SetCellValue(building.NumBuilding);
                            }

                            bool isDoctorFirstIteration = true;

                            if (building.Doctors == null || !building.Doctors.Any())
                            {
                                sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                                sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                                continue;
                            }

                            foreach (var doctor in building.Doctors)
                            {
                                if (isDoctorFirstIteration)
                                {
                                    sheet.GetRow(rowStart).CreateCell(6).SetCellValue(doctor.Item1);
                                    sheet.GetRow(rowStart).CreateCell(7).SetCellValue(doctor.Item2);
                                    isDoctorFirstIteration = false;
                                }
                                else
                                {
                                    var row = sheet.CreateRow(rowStart += 1);
                                    row.CreateCell(6).SetCellValue(doctor.Item1);
                                    row.CreateCell(7).SetCellValue(doctor.Item2);
                                }
                            }

                            if (sheet.LastRowNum - rowBuildStart < 1)
                            {
                                continue;
                            }

                            CellRangeAddress craBuild = new(rowBuildStart, sheet.LastRowNum, 4, 4);
                            sheet.AddMergedRegion(craBuild);
                            sheet.SetCellRangeBorders(craBuild);

                            CellRangeAddress craBuild2 = new(rowBuildStart, sheet.LastRowNum, 5, 5);
                            sheet.AddMergedRegion(craBuild2);
                            sheet.SetCellRangeBorders(craBuild2);
                        }

                        if (sheet.LastRowNum - rowHospitalStart < 1)
                        {
                            continue;
                        }

                        CellRangeAddress craHospital = new(rowHospitalStart, sheet.LastRowNum, 3, 3);

                        sheet.AddMergedRegion(craHospital);
                        sheet.SetCellRangeBorders(craHospital);
                    }

                    if (sheet.LastRowNum - rowCityStart < 1)
                    {
                        continue;
                    }

                    CellRangeAddress craCity = new(rowCityStart, sheet.LastRowNum, 1, 1);

                    sheet.AddMergedRegion(craCity);
                    sheet.SetCellRangeBorders(craCity);
                }

                var rowSpanDay = sheet.LastRowNum - rowDayStart;

                if (sheet.LastRowNum - rowDayStart < 1)
                {
                    continue;
                }

                CellRangeAddress craDay = new(rowDayStart, sheet.LastRowNum, 0, 0);

                sheet.AddMergedRegion(craDay);
                sheet.SetCellRangeBorders(craDay);

                CellRangeAddress craLenka = new(rowDayStart, sheet.LastRowNum, 2, 2);

                sheet.AddMergedRegion(craLenka);
                sheet.SetCellRangeBorders(craLenka);
            }

            for (int i = 1; i < sheet.LastRowNum + 1; i++)
            {
                var row = sheet.GetRow(i);

                for (int cellStart = 8; cellStart <= 10; cellStart++)
                {
                    row.CreateCell(cellStart).SetBlank();
                }

                foreach (var cell in row.Cells)
                {
                    cell.CellStyle = cellStyle;
                }
            }
        }

        private static void CreateReport_PharmaciesPlan(ExcelDocument document, ICellStyle cellStyle, int idReport)
        {
            int rowStart = 0;
            var days = LoadDays(idReport, MainEnv.ReportType.Plan);

            var sheet = document.GetSheet("План аптеки");

            foreach (var day in days)
            {
                var rowDayStart = rowStart + 1;

                var rowCity = sheet.CreateRow(rowStart += 1);

                rowCity.CreateCell(0).SetCellValue(day.Value.ToDBFormat_DateOnly());

                rowCity.CreateCell(1).SetCellValue("Станіславчук Олена");

                var cities = LoadPharmaciesCities(day.Key);

                if (cities != null && !cities.Any() || cities == null)
                {
                    sheet.GetRow(rowStart).CreateCell(1).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(3).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(4).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(5).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                    sheet.GetRow(rowStart).CreateCell(8).SetBlank();
                    continue;
                }

                bool isCityFirstIteration = true;

                foreach (var city in cities)
                {
                    var rowCityStart = rowStart + 1;
                    if (isCityFirstIteration)
                    {
                        rowCityStart -= 1;
                        sheet.GetRow(rowStart).CreateCell(2).SetCellValue(city.Value);
                        isCityFirstIteration = false;
                    }
                    else
                    {
                        sheet.CreateRow(rowStart += 1).CreateCell(2).SetCellValue(city.Value);
                    }

                    var pharmacies = LoadPharmacies(city.Key);

                    if (pharmacies != null && !pharmacies.Any() || pharmacies == null)
                    {
                        sheet.GetRow(rowStart).CreateCell(1).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(3).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(4).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(5).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(8).SetBlank();
                        continue;
                    }

                    bool isPharmacyFirstIteration = true;

                    foreach (var pharmacy in pharmacies)
                    {
                        var rowPharmacyStart = rowStart + 1;
                        if (isPharmacyFirstIteration)
                        {
                            rowPharmacyStart -= 1;

                            var row = sheet.GetRow(rowStart);
                            row.CreateCell(3).SetCellValue(pharmacy.Item1);
                            row.CreateCell(4).SetCellValue(pharmacy.Item2);
                            row.CreateCell(5).SetCellValue(pharmacy.Item3);
                            isPharmacyFirstIteration = false;
                        }
                        else
                        {
                            var row = sheet.CreateRow(rowStart += 1);
                            row.CreateCell(3).SetCellValue(pharmacy.Item1);
                            row.CreateCell(4).SetCellValue(pharmacy.Item2);
                            row.CreateCell(5).SetCellValue(pharmacy.Item3);
                        }

                        //var buildingsNDoctors = LoadBuildingsAndDoctors(hospital.Key);

                        //if (buildingsNDoctors == null || !buildingsNDoctors.Any())
                        //{
                        //    sheet.GetRow(rowStart).CreateCell(4).SetBlank();
                        //    sheet.GetRow(rowStart).CreateCell(5).SetBlank();
                        //    sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                        //    sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                        //    continue;
                        //}

                        //bool isBuildingFirstIteration = true;

                        //foreach (var building in buildingsNDoctors)
                        //{
                        //    var rowBuildStart = rowStart + 1;
                        //    if (isBuildingFirstIteration)
                        //    {
                        //        rowBuildStart -= 1;
                        //        sheet.GetRow(rowStart).CreateCell(4).SetCellValue(building.Street);
                        //        sheet.GetRow(rowStart).CreateCell(5).SetCellValue(building.NumBuilding);
                        //        isBuildingFirstIteration = false;
                        //    }
                        //    else
                        //    {
                        //        var row = sheet.CreateRow(rowStart += 1);
                        //        row.CreateCell(4).SetCellValue(building.Street);
                        //        row.CreateCell(5).SetCellValue(building.NumBuilding);
                        //    }

                        //    bool isDoctorFirstIteration = true;

                        //    if (building.Doctors == null || !building.Doctors.Any())
                        //    {
                        //        sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                        //        sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                        //        continue;
                        //    }

                        //    foreach (var doctor in building.Doctors)
                        //    {
                        //        if (isDoctorFirstIteration)
                        //        {
                        //            sheet.GetRow(rowStart).CreateCell(6).SetCellValue(doctor.Item1);
                        //            sheet.GetRow(rowStart).CreateCell(7).SetCellValue(doctor.Item2);
                        //            isDoctorFirstIteration = false;
                        //        }
                        //        else
                        //        {
                        //            var row = sheet.CreateRow(rowStart += 1);
                        //            row.CreateCell(6).SetCellValue(doctor.Item1);
                        //            row.CreateCell(7).SetCellValue(doctor.Item2);
                        //        }
                        //    }

                        //    if (sheet.LastRowNum - rowBuildStart < 1)
                        //    {
                        //        continue;
                        //    }

                        //    CellRangeAddress craBuild = new(rowBuildStart, sheet.LastRowNum, 4, 4);
                        //    sheet.AddMergedRegion(craBuild);
                        //    sheet.SetCellRangeBorders(craBuild);

                        //    CellRangeAddress craBuild2 = new(rowBuildStart, sheet.LastRowNum, 5, 5);
                        //    sheet.AddMergedRegion(craBuild2);
                        //    sheet.SetCellRangeBorders(craBuild2);
                        //}

                        //if (sheet.LastRowNum - rowHospitalStart < 1)
                        //{
                        //    continue;
                        //}

                        //CellRangeAddress craHospital = new(rowHospitalStart, sheet.LastRowNum, 3, 3);

                        //sheet.AddMergedRegion(craHospital);
                        //sheet.SetCellRangeBorders(craHospital);
                    }

                    if (sheet.LastRowNum - rowCityStart < 1)
                    {
                        continue;
                    }

                    CellRangeAddress craCity = new(rowCityStart, sheet.LastRowNum, 1, 1);

                    sheet.AddMergedRegion(craCity);
                    sheet.SetCellRangeBorders(craCity);
                }

                var rowSpanDay = sheet.LastRowNum - rowDayStart;

                if (sheet.LastRowNum - rowDayStart < 1)
                {
                    continue;
                }

                CellRangeAddress craDay = new(rowDayStart, sheet.LastRowNum, 0, 0);

                sheet.AddMergedRegion(craDay);
                sheet.SetCellRangeBorders(craDay);

                CellRangeAddress craLenka = new(rowDayStart, sheet.LastRowNum, 2, 2);

                sheet.AddMergedRegion(craLenka);
                sheet.SetCellRangeBorders(craLenka);
            }
        }

        /// <summary>
        /// Лікарі (факт)
        /// </summary>
        /// <param name="idReport"></param>
        /// <param name="fileName"></param>
        public static void CreateReport_Fact_old(int idReport, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                MessageBox.Show("Не вказано ім'я файлу!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var days = LoadDays(idReport, MainEnv.ReportType.Fact);

            using (var document = ExcelDocument.OpenFromTemplate(Template))
            {
                if (document == null)
                {
                    return;
                }

                var cellFont = document.CreateFont();

                cellFont.FontName = "Arial";
                cellFont.FontHeightInPoints = 9;

                var cellStyle = document.CreateCellStyle();
                cellStyle.SetFont(cellFont);

                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.WrapText = true;
                cellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;

                var rowStart = 0;
                var sheet = document.GetSheetAt(0);

                foreach (var day in days)
                {
                    var rowDayStart = rowStart + 1;

                    var rowCity = sheet.CreateRow(rowStart += 1);

                    rowCity.CreateCell(0).SetCellValue(day.Value.ToDBFormat_DateOnly());

                    rowCity.CreateCell(2).SetCellValue("Станіславчук Олена");

                    var cities = LoadCities(day.Key);
                    if (cities != null && !cities.Any() || cities == null)
                    {
                        sheet.GetRow(rowStart).CreateCell(1).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(3).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(4).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(5).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                        sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                        continue;
                    }

                    bool isCityFirstIteration = true;

                    foreach (var city in cities)
                    {
                        var rowCityStart = rowStart + 1;
                        if (isCityFirstIteration)
                        {
                            rowCityStart -= 1;
                            sheet.GetRow(rowStart).CreateCell(1).SetCellValue(city.Value);
                            isCityFirstIteration = false;
                        }
                        else
                        {
                            sheet.CreateRow(rowStart += 1).CreateCell(1).SetCellValue(city.Value);
                        }

                        var hospitals = LoadHospitals(city.Key);

                        if (hospitals != null && !hospitals.Any() || hospitals == null)
                        {
                            sheet.GetRow(rowStart).CreateCell(3).SetBlank();
                            sheet.GetRow(rowStart).CreateCell(4).SetBlank();
                            sheet.GetRow(rowStart).CreateCell(5).SetBlank();
                            sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                            sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                            continue;
                        }

                        bool isHospitalFirstIteration = true;

                        foreach (var hospital in hospitals)
                        {
                            var rowHospitalStart = rowStart + 1;
                            if (isHospitalFirstIteration)
                            {
                                rowHospitalStart -= 1;
                                sheet.GetRow(rowStart).CreateCell(3).SetCellValue(hospital.Value);
                                isHospitalFirstIteration = false;
                            }
                            else
                            {
                                sheet.CreateRow(rowStart += 1).CreateCell(3).SetCellValue(hospital.Value);
                            }

                            var buildingsNDoctors = LoadBuildingsAndDoctors(hospital.Key);

                            if (buildingsNDoctors == null || !buildingsNDoctors.Any())
                            {
                                sheet.GetRow(rowStart).CreateCell(4).SetBlank();
                                sheet.GetRow(rowStart).CreateCell(5).SetBlank();
                                sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                                sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                                continue;
                            }

                            bool isBuildingFirstIteration = true;

                            foreach (var building in buildingsNDoctors)
                            {
                                var rowBuildStart = rowStart + 1;
                                if (isBuildingFirstIteration)
                                {
                                    rowBuildStart -= 1;
                                    sheet.GetRow(rowStart).CreateCell(4).SetCellValue(building.Street);
                                    sheet.GetRow(rowStart).CreateCell(5).SetCellValue(building.NumBuilding);
                                    isBuildingFirstIteration = false;
                                }
                                else
                                {
                                    var row = sheet.CreateRow(rowStart += 1);
                                    row.CreateCell(4).SetCellValue(building.Street);
                                    row.CreateCell(5).SetCellValue(building.NumBuilding);
                                }

                                bool isDoctorFirstIteration = true;

                                if (building.Doctors == null || !building.Doctors.Any())
                                {
                                    sheet.GetRow(rowStart).CreateCell(6).SetBlank();
                                    sheet.GetRow(rowStart).CreateCell(7).SetBlank();
                                    continue;
                                }

                                foreach (var doctor in building.Doctors)
                                {
                                    if (isDoctorFirstIteration)
                                    {
                                        sheet.GetRow(rowStart).CreateCell(6).SetCellValue(doctor.Item1);
                                        sheet.GetRow(rowStart).CreateCell(7).SetCellValue(doctor.Item2);
                                        isDoctorFirstIteration = false;
                                    }
                                    else
                                    {
                                        var row = sheet.CreateRow(rowStart += 1);
                                        row.CreateCell(6).SetCellValue(doctor.Item1);
                                        row.CreateCell(7).SetCellValue(doctor.Item2);
                                    }
                                }

                                if (sheet.LastRowNum - rowBuildStart < 1)
                                {
                                    continue;
                                }

                                CellRangeAddress craBuild = new(rowBuildStart, sheet.LastRowNum, 4, 4);
                                sheet.AddMergedRegion(craBuild);
                                sheet.SetCellRangeBorders(craBuild);

                                CellRangeAddress craBuild2 = new(rowBuildStart, sheet.LastRowNum, 5, 5);
                                sheet.AddMergedRegion(craBuild2);
                                sheet.SetCellRangeBorders(craBuild2);
                            }

                            if (sheet.LastRowNum - rowHospitalStart < 1)
                            {
                                continue;
                            }

                            CellRangeAddress craHospital = new(rowHospitalStart, sheet.LastRowNum, 3, 3);

                            sheet.AddMergedRegion(craHospital);
                            sheet.SetCellRangeBorders(craHospital);
                        }

                        if (sheet.LastRowNum - rowCityStart < 1)
                        {
                            continue;
                        }

                        CellRangeAddress craCity = new(rowCityStart, sheet.LastRowNum, 1, 1);

                        sheet.AddMergedRegion(craCity);
                        sheet.SetCellRangeBorders(craCity);
                    }

                    var rowSpanDay = sheet.LastRowNum - rowDayStart;

                    if (sheet.LastRowNum - rowDayStart < 1)
                    {
                        continue;
                    }

                    CellRangeAddress craDay = new(rowDayStart, sheet.LastRowNum, 0, 0);

                    sheet.AddMergedRegion(craDay);
                    sheet.SetCellRangeBorders(craDay);

                    CellRangeAddress craLenka = new(rowDayStart, sheet.LastRowNum, 2, 2);

                    sheet.AddMergedRegion(craLenka);
                    sheet.SetCellRangeBorders(craLenka);
                }

                for (int i = 1; i < sheet.LastRowNum + 1; i++)
                {
                    var row = sheet.GetRow(i);

                    for (int cellStart = 8; cellStart <= 10; cellStart++)
                    {
                        row.CreateCell(cellStart).SetBlank();
                    }

                    foreach (var cell in row.Cells)
                    {
                        cell.CellStyle = cellStyle;
                    }
                }

                document.SaveAs(fileName);
            }
        }

        private static Dictionary<int, string?>? LoadCities(int idReportDay)
        {
            string sql = $@"SELECT
                  RS.ID_REPORT_CITY,
                  C.TITLE
                FROM REPORT_CITIES RS
                  LEFT JOIN CITIES C
                    ON RS.ID_CITY = C.ID_CITY
                WHERE RS.ID_REPORT_DAY = {idReportDay}";

            Dictionary<int, string?>? cities = new();

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    cities.Add(e.GetValueOrDefault<int>("ID_REPORT_CITY"), e.GetValueOrDefault<string>("TITLE"));
                }
            });

            return cities;
        }
        private static Dictionary<int, string?>? LoadHospitals(int idReportCity)
        {
            string sql = $@"SELECT
                      RH.ID_REPORT_HOSPITAL,
                      H.TITLE
                    FROM REPORT_HOSPITALS RH
                      LEFT JOIN HOSPITALS H
                        ON RH.ID_HOSPITAL = H.ID_HOSPITAL
                    WHERE RH.ID_REPORT_CITY = {idReportCity}";

            Dictionary<int, string?>? hospitals = new();

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    hospitals.Add(e.GetValueOrDefault<int>("ID_REPORT_HOSPITAL"), e.GetValueOrDefault<string>("TITLE"));
                }
            });

            return hospitals;
        }

        private static Dictionary<int, DateTime> LoadDays(int idReport, MainEnv.ReportType reportType)
        {
            string sql = $"SELECT ID_REPORT_DAY, DAY FROM REPORT_DAYS WHERE ID_REPORT={idReport} AND REPORT_TYPE={(int)reportType} ORDER BY DAY";

            var days = new Dictionary<int, DateTime>();

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportDay = e.GetValueOrDefault<int>("ID_REPORT_DAY");
                    var day = e.GetValueOrDefault<DateTime>("DAY");
                    days.Add(idReportDay, day);
                }
            });
            return days;
        }

        private static Dictionary<int, string?> LoadPharmaciesCities(int idReportDay)
        {
            string sql = $@"SELECT
                  RC.ID_REPORT_CITY,
                  C.TITLE
                FROM REPORT_CITIES RC
                  LEFT JOIN REPORT_PHARMACIES RP
                    ON RC.ID_REPORT_CITY = RP.ID_REPORT_CITY
                    WHERE RC.ID_REPORT_DAY={idReportDay}
                  LEFT JOIN CITIES C
                    ON RC.ID_CITY = C.ID_CITY
                GROUP BY RC.ID_REPORT_CITY
                HAVING COUNT(RP.ID_REPORT_CITY) > 0";

            var ret = new Dictionary<int, string?>();

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportCity = e.GetValueOrDefault<int>("ID_REPORT_CITY");
                    string? nameCity = e.GetValueOrDefault<string>("TITLE");
                    _ = ret.TryAdd(idReportCity, nameCity);
                }
            });

            return ret;
        }

        private static IReadOnlyList<(string?, string?, string?)> LoadPharmacies(int idReportCity)
        {
            List<(string?, string?, string?)> ret = new();

            string sql = $@"SELECT
              P.NAME_PHARMACY,
              P.STREET,
              P.BUILD_NUM
            FROM REPORT_PHARMACIES RP
              LEFT JOIN PHARMACIES P
                ON RP.ID_PHARMACY = P.ID_PHARMACY
            WHERE RP.ID_REPORT_CITY = {idReportCity}";

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    string? namePharmacy = e.GetValueOrDefault<string>("NAME_PHARMACY");
                    string? street = e.GetValueOrDefault<string>("STREET");
                    string? buildNum = e.GetValueOrDefault<string>("BUILD_NUM");

                    ret.Add((namePharmacy, street, buildNum));
                }
            });

            return ret;
        }

        public static Dictionary<int, List<(string?, string?)>> LoadBuildings(int idReportHospital)
        {
            string sql = @$"SELECT
                  D.ID_ADDRESS,
                  D.FULL_NAME,
                  S.NAME_SPECIALITY
                FROM REPORT_DOCTORS RD
                  LEFT JOIN DOCTORS D
                    ON RD.ID_DOCTOR = D.ID_DOCTOR
                  LEFT JOIN SPECIALITIES S
                    ON D.SPECIALITY = S.ID_SPECIALITY
                WHERE RD.ID_REPORT_HOSPITAL = {idReportHospital}";

            Dictionary<int, List<(string?, string?)>> buildings = new();

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idAddress = e.GetValueOrDefault<int>("ID_ADDRESS");
                    string? fullName = e.GetValueOrDefault<string>("FULL_NAME");
                    string? speciality = e.GetValueOrDefault<string>("NAME_SPECIALITY");

                    if (!buildings.TryGetValue(idAddress, out var doctors))
                    {
                        doctors = new List<(string?, string?)>();
                        buildings.Add(idAddress, doctors);
                    }

                    doctors.Add((fullName, speciality));
                }
            });

            return buildings;
        }

        public static IEnumerable<ReportBuilding> LoadBuildingsAndDoctors(int idReportHospital)
        {
            string sql = @$"SELECT
				  A.STREET,
				  A.BUILD_NUMBER,
                  D.ID_ADDRESS,
                  D.FULL_NAME,
                  S.NAME_SPECIALITY
                FROM REPORT_DOCTORS RD
                  LEFT JOIN DOCTORS D
                    ON RD.ID_DOCTOR = D.ID_DOCTOR
                  LEFT JOIN SPECIALITIES S
                    ON D.SPECIALITY = S.ID_SPECIALITY
					LEFT JOIN ADDRESSES A ON D.ID_ADDRESS=A.ID_ADDRESS
                WHERE RD.ID_REPORT_HOSPITAL = {idReportHospital}";

            Dictionary<int, ReportBuilding> buildings = new();

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idAddress = e.GetValueOrDefault<int>("ID_ADDRESS");
                    string? street = e.GetValueOrDefault<string>("STREET");
                    string? buildNumber = e.GetValueOrDefault<string>("BUILD_NUMBER");

                    string? fullName = e.GetValueOrDefault<string>("FULL_NAME");
                    string? speciality = e.GetValueOrDefault<string>("NAME_SPECIALITY");

                    if (!buildings.TryGetValue(idAddress, out var building))
                    {
                        building = new ReportBuilding(street, buildNumber);
                        buildings.Add(idAddress, building);
                        //doctors = new List<(string?, string?)>();
                        //buildings.Add(idAddress, doctors);
                    }

                    building.AddDoctor(fullName, speciality);
                    //doctors.Add((fullName, speciality));
                }
            });

            return buildings.Values;
        }

        public static void SetCellRangeBorders(this ISheet worksheet, CellRangeAddress cra)
        {
            RegionUtil.SetBorderTop((int)BorderStyle.Thin, cra, worksheet);
            RegionUtil.SetBorderRight((int)BorderStyle.Thin, cra, worksheet);
            RegionUtil.SetBorderBottom((int)BorderStyle.Thin, cra, worksheet);
            RegionUtil.SetBorderLeft((int)BorderStyle.Thin, cra, worksheet);
        }
    }
}