using LenkasLittleHelper.Database;
using LenkasLittleHelper.Helpers;
using System;
using System.Collections.Generic;
using NPOI.SS.Util;
using System.IO;

namespace LenkasLittleHelper
{
    internal class MakeReport
    {
        public static string Pathh = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static void CreateReport_Fact(int idReport)
        {
            var days = LoadDays(idReport);

            using (var document = ExcelDocument.CreateNew())
            {
                var rowStart = -1;
                var sheet = document.CreateSheet();
                foreach (var day in days)
                {
                    var rowDayStart = rowStart + 1;

                    var dayRowSpan = 0;
                    sheet.CreateRow(rowStart += 1).CreateCell(0).SetCellValue(day.Value.ToDBFormat_DateOnly());

                    var cities = LoadCities(day.Key);

                    if (cities == null || cities.Count < 1)
                    {
                        continue;
                    }

                    if (cities.Count - 1 > 0)
                    {
                        dayRowSpan = cities.Count - 1;
                    }

                    bool isCityFirstIteration = true;

                    foreach (var city in cities)
                    {
                        var cityRowStart = rowStart + 1;

                        var cityRowSpan = 0;
                        if (isCityFirstIteration)
                        {
                            sheet.GetRow(rowStart).CreateCell(1).SetCellValue(city.Value);
                            isCityFirstIteration = false;
                        }
                        else
                        {
                            sheet.CreateRow(rowStart += 1).CreateCell(1).SetCellValue(city.Value);
                        }

                        var hospitals = LoadHospitals(city.Key);

                        if (hospitals == null || hospitals.Count < 1)
                        {
                            continue;
                        }
                        if (hospitals.Count - 1 > cityRowSpan)
                        {
                            cityRowSpan = hospitals.Count - 1;
                        }
                        if (hospitals.Count - 1 > dayRowSpan)
                        {
                            dayRowSpan = hospitals.Count - 1;
                        }

                        bool isHospitalFirstIteration = true;

                        foreach (var hospital in hospitals)
                        {
                            var hospitalRowStart = rowStart + 1;

                            var hospitalRowSpan = 0;

                            if (isHospitalFirstIteration)
                            {
                                sheet.GetRow(rowStart).CreateCell(2).SetCellValue(hospital.Value);
                                isHospitalFirstIteration = false;
                            }
                            else
                            {
                                sheet.CreateRow(rowStart += 1).CreateCell(2).SetCellValue(hospital.Value);
                            }

                            continue;

                            Dictionary<int, List<(string?, string?)>> buildings = LoadBuildings(hospital.Key);

                            if (buildings.Count < 1)
                            {
                                continue;
                            }

                            if (buildings.Count - 1 > hospitalRowSpan)
                            {
                                hospitalRowSpan = buildings.Count - 1;
                            }

                            if (buildings.Count - 1 > cityRowSpan)
                            {
                                cityRowSpan = buildings.Count - 1;
                            }

                            if (buildings.Count - 1 > dayRowSpan)
                            {
                                dayRowSpan = buildings.Count - 1;
                            }

                            bool isBuildFirstIteration = true;

                            foreach (KeyValuePair<int, List<(string?, string?)>> build in buildings)
                            {
                                var buildRowStart = rowStart + 1;

                                var buildRowSpan = 0;

                                if (build.Value.Count - 1 > buildRowSpan)
                                {
                                    buildRowSpan = build.Value.Count - 1;
                                }
                                //if (build.Value.Count < 1)
                                //{

                                //}

                                if (isBuildFirstIteration)
                                {
                                    sheet.GetRow(rowStart).CreateCell(3).SetCellValue(build.Key);
                                    isBuildFirstIteration = false;
                                }
                                else
                                {
                                    sheet.CreateRow(rowStart += 1).CreateCell(3).SetCellValue(build.Key);
                                }

                                bool doctorsFirstIteration = true;

                                //foreach ((string?, string?) doctor in build.Value)
                                //{
                                //    if (doctorsFirstIteration)
                                //    {
                                //        sheet.GetRow(rowStart).CreateCell(4).SetCellValue(doctor.Item1);
                                //        sheet.GetRow(rowStart).CreateCell(5).SetCellValue(doctor.Item2);
                                //        doctorsFirstIteration = false;
                                //    }
                                //    else
                                //    {
                                //        sheet.CreateRow(rowStart += 1).CreateCell(4).SetCellValue(doctor.Item1);
                                //        sheet.CreateRow(rowStart += 1).CreateCell(5).SetCellValue(doctor.Item2);
                                //    }
                                //}

                                //sheet.GetRow(buildRowStart).CreateCell(6).SetCellValue(build.Key);
                                //sheet.GetRow(buildRowStart).CreateCell(5).SetCellValue(build.Key);

                                //if (build.Value.Count > 1)
                                //{
                                //    var craNumBuild = new CellRangeAddress(buildRowStart, rowTotalStart, 6, 6);
                                //    sheet.AddMergedRegion(craNumBuild);

                                //    var craStreet = new CellRangeAddress(buildRowStart, rowTotalStart, 5, 5);
                                //    sheet.AddMergedRegion(craStreet);
                                //}
                            }
                        }

                        if (cityRowSpan > 0)
                        {
                            //dayRowSpan += cityRowSpan;

                            if (dayRowSpan > rowStart)
                            {
                                rowStart = dayRowSpan;
                            }

                            var craCity = new CellRangeAddress(cityRowSpan, cityRowStart + cityRowSpan, 1, 1);
                            sheet.AddMergedRegion(craCity);
                        }
                    }

                    if (dayRowSpan > 0)
                    {
                        var craDay = new CellRangeAddress(rowDayStart, rowDayStart + dayRowSpan, 0, 0);
                        sheet.AddMergedRegion(craDay);
                    }
                }

                document.SaveAs(Path.Combine(Pathh, "new.xlsx"));
            }
        }

        private static Dictionary<int, string>? LoadCities(int idReportDay)
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
        private static Dictionary<int, string>? LoadHospitals(int idReportCity)
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

        //public static void CreateReport_Fact(int idReport)
        //{
        //    //LoadDays(idReport);

        //    using (var document = ExcelDocument.CreateNew())
        //    {
        //        var sheet = document.CreateSheet();

        //        var days = LoadDays(idReport);

        //        int rowTotalStart = -1;

        //        foreach (var day in days)
        //        {
        //            int rowStart = rowTotalStart;

        //            var buildings = LoadBuildings(day.Key);

        //            foreach (KeyValuePair<int, List<(string?, string?)>> build in buildings)
        //            {
        //                var buildRowStart = rowTotalStart + 1;

        //                foreach ((string?, string?) doctor in build.Value)
        //                {
        //                    rowTotalStart += 1;
        //                    var row = sheet.CreateRow(rowTotalStart);
        //                    row.CreateCell(7).SetCellValue(doctor.Item1);
        //                    row.CreateCell(8).SetCellValue(doctor.Item2);
        //                }

        //                sheet.GetRow(buildRowStart).CreateCell(6).SetCellValue(build.Key);
        //                sheet.GetRow(buildRowStart).CreateCell(5).SetCellValue(build.Key);

        //                if (build.Value.Count > 1)
        //                {
        //                    var craNumBuild = new CellRangeAddress(buildRowStart, rowTotalStart, 6, 6);
        //                    sheet.AddMergedRegion(craNumBuild);

        //                    var craStreet = new CellRangeAddress(buildRowStart, rowTotalStart, 5, 5);
        //                    sheet.AddMergedRegion(craStreet);
        //                }
        //            }
        //        }

        //        document.SaveAs(Path.Combine(Pathh, "new.xlsx"));
        //    }
        //}

        private static Dictionary<int, DateTime> LoadDays(int idReport)
        {
            string sql = $"SELECT ID_REPORT_DAY, DAY FROM REPORT_DAYS WHERE ID_REPORT={idReport} ORDER BY DAY";

            var days = new Dictionary<int, DateTime>();

            DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idReportDay = e.GetValueOrDefault<int>("ID_REPORT_DAY");
                    DateTime day = e.GetValueOrDefault<DateTime>("DAY");
                    days.Add(idReportDay, day);
                }
            });
            return days;
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
    }
}