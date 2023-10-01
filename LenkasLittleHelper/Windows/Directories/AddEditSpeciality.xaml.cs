using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LenkasLittleHelper.Windows.Directories
{
    /// <summary>
    /// Interaction logic for AddEditSpeciality.xaml
    /// </summary>
    public partial class AddEditSpeciality : Window
    {
        private readonly Speciality_Directory? SpecialityCurrent;
        public AddEditSpeciality(Speciality_Directory specialityCurrent)
        {
            InitializeComponent();
            SpecialityCurrent = specialityCurrent;
            NameSpeciality.Text = specialityCurrent.Name;
            Title = specialityCurrent.Name;
        }

        public AddEditSpeciality()
        {
            InitializeComponent();
        }

        private void SaveSpeciality_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameSpeciality.Text))
            {
                MessageBox.Show("Назва спеціальності не може бути порожньою!");
                return;
            }

            Dictionary<string, object> cmdParams = new()
            {
                {"title", NameSpeciality.Text }
            };

            if (SpecialityCurrent != null)
            {
                string sql = $"UPDATE SPECIALITIES SET NAME_SPECIALITY=@title WHERE ID_SPECIALITY={SpecialityCurrent.IdSpeciality}";
                var err = DBHelper.DoCommand(sql, cmdParams);

                if (!string.IsNullOrEmpty(err))
                {
                    MainEnv.ShowErrorDlg(err);
                    return;
                }

                Close();
                return;
            }

            var specialitiesAllSql = "SELECT * FROM SPECIALITIES";

            List<Speciality_Directory> existingSpecialities = new();

            var error = DBHelper.ExecuteReader(specialitiesAllSql, e =>
            {
                while (e.Read())
                {
                    int idSpeciality = e.GetValueOrDefault<int>("ID_SPECIALITY");
                    string? nameSpeciality = e.GetValueOrDefault<string>("NAME_SPECIALITY");
                    bool isArchived = e.GetValueOrDefault<bool>("IS_ARCHIVED");

                    existingSpecialities.Add(new Speciality_Directory(idSpeciality, nameSpeciality, isArchived));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
                return;
            }

            string addSpecialitySql = "INSERT INTO SPECIALITIES(NAME_SPECIALITY) VALUES (@title)";

            foreach (var speciality in existingSpecialities)
            {
                //Якщо архівне-виводимо із архіву
                if (!string.IsNullOrEmpty(speciality.Name) && speciality.Name.Trim() == NameSpeciality.Text)
                {
                    if (speciality.IsArchived)
                    {
                        addSpecialitySql = $"UPDATE SPECIALITIES SET IS_ARCHIVED=0 WHERE ID_SPECIALITY={speciality.IdSpeciality}";
                    }
                    else
                    {
                        MessageBox.Show($"Спеціальність {speciality.Name} уже існує!", "Неможливо додати спеціальність!");
                        return;
                    }
                }
            }

            error = DBHelper.DoCommand(addSpecialitySql, cmdParams);

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
                return;
            }

            Close();
        }
    }
}