using LenkasLittleHelper.Database;
using LenkasLittleHelper.Helpers;
using System;
using System.Collections.Generic;
using NPOI.SS.Util;
using System.IO;
using System.Linq;

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

                    sheet.CreateRow(rowStart += 1).CreateCell(0).SetCellValue(day.Value.ToDBFormat_DateOnly());

                    var cities = LoadCities(day.Key);
                    if ((cities != null && !cities.Any()) || cities == null)
                    {
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

                        if ((hospitals != null && !hospitals.Any()) || hospitals == null)
                        {
                            continue;
                        }

                        bool isHospitalFirstIteration = true;

                        foreach (var hospital in hospitals)
                        {
                            var rowHospitalStart = rowStart + 1;
                            if (isHospitalFirstIteration)
                            {
                                rowHospitalStart -= 1;
                                sheet.GetRow(rowStart).CreateCell(2).SetCellValue(hospital.Value);
                                isHospitalFirstIteration = false;
                            }
                            else
                            {
                                sheet.CreateRow(rowStart += 1).CreateCell(2).SetCellValue(hospital.Value);
                            }

                            Dictionary<int, List<(string?, string?)>> buildings = LoadBuildings(hospital.Key);
                            if (buildings == null || !buildings.Any())
                            {
                                continue;
                            }

                            bool isBuildingFirstIteration = true;

                            foreach (KeyValuePair<int, List<(string?, string?)>> building in buildings)
                            {
                                var rowBuildStart = rowStart + 1;
                                if (isBuildingFirstIteration)
                                {
                                    rowBuildStart -= 1;
                                    sheet.GetRow(rowStart).CreateCell(3).SetCellValue(building.Key);
                                    sheet.GetRow(rowStart).CreateCell(4).SetCellValue(building.Key);
                                    isBuildingFirstIteration = false;
                                }
                                else
                                {
                                    sheet.CreateRow(rowStart += 1).CreateCell(3).SetCellValue(building.Key);
                                    sheet.CreateRow(rowStart += 1).CreateCell(4).SetCellValue(building.Key);
                                }
                                if (building.Value == null || !building.Value.Any())
                                {
                                    continue;
                                }

                                bool isDoctorFirstIteration = true;

                                foreach (var doctor in building.Value)
                                {
                                    if (isDoctorFirstIteration)
                                    {
                                        sheet.GetRow(rowStart).CreateCell(5).SetCellValue(doctor.Item1);
                                        sheet.GetRow(rowStart).CreateCell(6).SetCellValue(doctor.Item2);
                                        isDoctorFirstIteration = false;
                                    }
                                    else
                                    {
                                        var row = sheet.CreateRow(rowStart += 1);
                                        row.CreateCell(5).SetCellValue(doctor.Item1);
                                        row.CreateCell(6).SetCellValue(doctor.Item2);
                                    }
                                }

                                if (sheet.LastRowNum - rowBuildStart < 1)
                                {
                                    continue;
                                }

                                CellRangeAddress craBuild = new(rowBuildStart, sheet.LastRowNum, 3, 3);
                                sheet.AddMergedRegion(craBuild);
                                
                                CellRangeAddress craBuild2 = new(rowBuildStart, sheet.LastRowNum, 4, 4);
                                sheet.AddMergedRegion(craBuild2);
                            }

                            if (sheet.LastRowNum - rowHospitalStart < 1)
                            {
                                continue;
                            }

                            CellRangeAddress craHospital = new(rowHospitalStart, sheet.LastRowNum, 2, 2);

                            sheet.AddMergedRegion(craHospital);
                        }

                        if (sheet.LastRowNum - rowCityStart < 1)
                        {
                            continue;
                        }

                        CellRangeAddress craCity = new(rowCityStart, sheet.LastRowNum, 1, 1); ;

                        sheet.AddMergedRegion(craCity);
                    }

                    var rowSpanDay = sheet.LastRowNum - rowDayStart;

                    if (sheet.LastRowNum - rowDayStart < 1)
                    {
                        continue;
                    }

                    CellRangeAddress craDay = new(rowDayStart, sheet.LastRowNum, 0, 0); ;

                    sheet.AddMergedRegion(craDay);
                }

                document.SaveAs(Path.Combine(Pathh, "Reports", $"new{DateTime.Now.Ticks}.xlsx"));
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