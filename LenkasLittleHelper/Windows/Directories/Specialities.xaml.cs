using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LenkasLittleHelper.Windows.Directories
{
    /// <summary>
    /// Interaction logic for Specialities.xaml
    /// </summary>
    public partial class Specialities : Window
    {
        private ObservableCollection<Speciality_Directory> Specialities_Src { get; set; } = new();

        public Specialities()
        {
            InitializeComponent();
            ListSpecialities.ItemsSource = Specialities_Src;
            LoadSpecialities();
        }

        private void LoadSpecialities()
        {
            Specialities_Src.Clear();

            string sql = $@"SELECT ID_SPECIALITY, 
                            NAME_SPECIALITY, 
                            IS_ARCHIVED FROM SPECIALITIES ";

            if (!ShowArchSpecialities.IsChecked.HasValue || !ShowArchSpecialities.IsChecked.Value)
            {
                sql += "WHERE IS_ARCHIVED=0 ";
            }

            sql += "ORDER BY NAME_SPECIALITY";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idSpeciality = e.GetValueOrDefault<int>("ID_SPECIALITY");
                    string? title = e.GetValueOrDefault<string>("NAME_SPECIALITY");
                    bool isArchived = e.GetValueOrDefault<bool>("IS_ARCHIVED");

                    Specialities_Src.Add(new Speciality_Directory(idSpeciality, title, isArchived));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }
        }

        private void ListSpecialities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListSpecialities.SelectedItem is not Speciality_Directory speciality)
            {
                Btn_EditSpeciality.IsEnabled = false;
                Btn_ArchiveSpeciality.IsEnabled = false;
                return;
            }

            Btn_EditSpeciality.IsEnabled = true;
            Btn_ArchiveSpeciality.IsEnabled = true;

            Btn_ArchiveSpeciality.Content = speciality.IsArchived ? "Із архіву" : "У архів";
        }

        private void Btn_Add_EditSpeciality_Click(object sender, RoutedEventArgs e)
        {
            if (ListSpecialities.SelectedItem is not Speciality_Directory speciality)
            {
                return;
            }

            var wndSpeciality = new AddEditSpeciality(speciality);
            wndSpeciality.Show();

            wndSpeciality.Closed += (s, e) =>
            {
                LoadSpecialities();
            };
        }

        private void Btn_Add_Speciality_Click(object sender, RoutedEventArgs e)
        {
            var wndSpeciality = new AddEditSpeciality();

            wndSpeciality.Show();

            wndSpeciality.Closed += (s, e) =>
            {
                LoadSpecialities();
            };
        }

        private void Btn_ArchiveSpeciality_Click(object sender, RoutedEventArgs e)
        {
            if (ListSpecialities.SelectedItem is not Speciality_Directory speciality)
            {
                return;
            }

            string sql = $@"UPDATE SPECIALITIES SET IS_ARCHIVED={Convert.ToInt32(!speciality.IsArchived)} 
                            WHERE ID_SPECIALITY={speciality.IdSpeciality}";

            var error = DBHelper.DoCommand(sql);

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }

            LoadSpecialities();
        }

        private void SearchSpeciality_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchSpeciality.Text) || SearchSpeciality.Text.Length < 2)
            {
                ListSpecialities.ItemsSource = Specialities_Src;
                return;
            }

            var filterText = SearchSpeciality.Text.ToLower();

            var filtered = Specialities_Src.Where(p =>
            !string.IsNullOrEmpty(p.Name) && p.Name.ToLower().IndexOf(filterText) != -1);

            ListSpecialities.ItemsSource = filtered;
        }

        private void ShowArchSpecialities_Checked(object sender, RoutedEventArgs e)
        {
            LoadSpecialities();
        }
    }
}