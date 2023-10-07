using LenkasLittleHelper.Database;
using LenkasLittleHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace LenkasLittleHelper
{
    /// <summary>
    /// Interaction logic for Directory_AddEditDoctor.xaml
    /// </summary>
    public partial class Directory_AddEditDoctor : Window
    {
        private readonly ObservableCollection<Address> Addresses = new();
        private readonly ObservableCollection<Speciality> Specialities = new();
        private readonly ObservableCollection<ReportCategory> Categories = new();

        private readonly int IdHospital;
        private readonly Doctor_Directory? Doctor;
        public Directory_AddEditDoctor(string? titleHospital, int idHospital)
        {
            InitializeComponent();
            IdHospital = idHospital;
            LoadStaff();
            Title = $"Додавання лікаря для {titleHospital}";
        }
        public Directory_AddEditDoctor(int idHospital, Doctor_Directory doctor)
        {
            InitializeComponent();
            IdHospital = idHospital;
            Doctor = doctor;

            Title = $"{doctor.FullName}, {doctor.City}, {doctor.NameHospital}";

            DoctorName.Text = doctor.FullName;
            PhoneNum.Text = doctor.PhoneNum;
            IsVisitable.IsChecked = doctor.Visitable;

            LoadStaff();
            InitPickers();
        }

        private void LoadStaff()
        {
            ListAddreses.ItemsSource = Addresses;
            ListSpecialities.ItemsSource = Specialities;
            ListCategories.ItemsSource = Categories;

            InitAddresses();
            InitSpecialities();
            InitCategories();
        }

        private void InitAddresses()
        {
            Addresses.Clear();

            string sql = @$"SELECT
                  ID_ADDRESS,
                  STREET,
                  BUILD_NUMBER
                FROM ADDRESSES
                WHERE ID_HOSPITAL = {IdHospital}";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idAddress = e.GetValueOrDefault<int>("ID_ADDRESS");
                    string? street = e.GetValueOrDefault<string>("STREET");
                    string? build = e.GetValueOrDefault<string>("BUILD_NUMBER");

                    Addresses.Add(new Address(idAddress, street, build));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }
        }

        private void InitSpecialities()
        {
            Specialities.Clear();

            string sql = @"SELECT
                  ID_SPECIALITY,
                  NAME_SPECIALITY
                FROM SPECIALITIES
                  WHERE IS_ARCHIVED=0 
                    ORDER BY NAME_SPECIALITY";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idSpeciality = e.GetValueOrDefault<int>("ID_SPECIALITY");
                    string? name = e.GetValueOrDefault<string>("NAME_SPECIALITY");

                    Specialities.Add(new Speciality(idSpeciality, name));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }
        }

        private void InitCategories()
        {
            Categories.Clear();

            string sql = @"SELECT
                          ID_CATEGORY,
                          TITLE
                        FROM CATEGORIES
                        ORDER BY TITLE";

            var error = DBHelper.ExecuteReader(sql, e =>
            {
                while (e.Read())
                {
                    int idCategory = e.GetValueOrDefault<int>("ID_CATEGORY");
                    string? title = e.GetValueOrDefault<string?>("TITLE");

                    Categories.Add(new ReportCategory(idCategory, title));
                }
            });

            if (!string.IsNullOrEmpty(error))
            {
                MainEnv.ShowErrorDlg(error);
            }
        }

        private void InitPickers()
        {
            if (Doctor == null)
            {
                return;
            }

            if (Addresses.Count == 1)
            {
                ListAddreses.SelectedIndex = 0;
            }

            foreach (var address in ListAddreses.ItemsSource)
            {
                if (address is not Address _address)
                {
                    continue;
                }
                if (_address.IdAddress == Doctor.IdAddress)
                {
                    ListAddreses.SelectedItem = address;
                    break;
                }
            }

            foreach (var speciality in ListSpecialities.ItemsSource)
            {
                if (speciality is not Speciality _speciality)
                {
                    continue;
                }
                if (_speciality.IdSpeciality == Doctor.IdSpeciality)
                {
                    ListSpecialities.SelectedItem = speciality;
                    break;
                }
            }

            foreach (var category in ListCategories.ItemsSource)
            {
                if (category is not ReportCategory _category)
                {
                    continue;
                }
                if (_category.IdCategory == Doctor.IdCategory)
                {
                    ListCategories.SelectedItem = category;
                    break;
                }
            }
        }

        private void Doctor_Save_Click(object sender, RoutedEventArgs e)
        {
            int idAddress = -1;
            int idSpeciality = -1;
            int idCategory = 1;
            string? street = null;
            string? buildNumber = null;

            if (ListAddreses.SelectedItem is Address a)
            {
                idAddress = a.IdAddress;
                street = a.Street;
                buildNumber = a.BuildNumber;
            }

            if (ListCategories.SelectedItem is ReportCategory rc)
            {
                idCategory = rc.IdCategory;
            }

            if (ListSpecialities.SelectedItem is Speciality rs)
            {
                idSpeciality = rs.IdSpeciality;
            }

            #region Введена вручну адреса
            if (!string.IsNullOrEmpty(AddressCustom.Text) || !string.IsNullOrEmpty(BuildCustom.Text))
            {
                if (!string.IsNullOrEmpty(AddressCustom.Text) && !string.IsNullOrEmpty(BuildCustom.Text))
                {
                    var addr = Addresses.FirstOrDefault
                    (e => !string.IsNullOrEmpty(e.Street) && e.Street.ToLower().Trim() == AddressCustom.Text.ToLower().Trim()
                    && !string.IsNullOrEmpty(e.BuildNumber) && e.BuildNumber.ToLower().Trim() == BuildCustom.Text.ToLower().Trim()
                    );

                    if (addr != null)
                    {
                        idAddress = addr.IdAddress;
                    }
                    else
                    {
                        string addAddressSql = "INSERT INTO ADDRESSES (ID_HOSPITAL, STREET, BUILD_NUMBER) VALUES (@idHospital, @street, @buildnum)";

                        var cmdParams_AddAddress = new Dictionary<string, object>()
                        {
                            {"idHospital",IdHospital},
                            {"street",AddressCustom.Text},
                            {"buildnum",BuildCustom.Text}
                        };

                        var error = DBHelper.DoCommand(addAddressSql, cmdParams_AddAddress);

                        if (!string.IsNullOrEmpty(error))
                        {
                            MainEnv.ShowErrorDlg(error);
                            return;
                        }

                        string getLastAddrIdSql = "SELECT ID_ADDRESS FROM ADDRESSES ORDER BY ID_ADDRESS DESC";
                        error = DBHelper.ExecuteReader(getLastAddrIdSql, e =>
                        {
                            if (e.Read())
                            {
                                idAddress = e.GetValueOrDefault<int>("ID_ADDRESS");
                            }
                        });

                        if (!string.IsNullOrEmpty(error))
                        {
                            MainEnv.ShowErrorDlg(error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Не вказано адресу або номер споруди!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            #endregion

            #region Введена вручну спеціальність
            if (!string.IsNullOrEmpty(SpecialityCustom.Text))
            {
                var spec = Specialities.FirstOrDefault
                    (e => !string.IsNullOrEmpty(e.Name) && e.Name.ToLower() == SpecialityCustom.Text.ToLower());
                if (spec != null)
                {
                    idSpeciality = spec.IdSpeciality;
                }
                else
                {
                    string addSpecSql = "INSERT INTO SPECIALITIES (NAME_SPECIALITY) VALUES (@nameSpeciality)";

                    var cmdParams_AddSpec = new Dictionary<string, object>()
                        {
                            {"nameSpeciality",SpecialityCustom.Text}
                        };

                    var error = DBHelper.DoCommand(addSpecSql, cmdParams_AddSpec);

                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show(error, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    string getLastAddrIdSql = "SELECT ID_SPECIALITY FROM SPECIALITIES ORDER BY ID_SPECIALITY DESC";
                    error = DBHelper.ExecuteReader(getLastAddrIdSql, e =>
                    {
                        if (e.Read())
                        {
                            idSpeciality = e.GetValueOrDefault<int>("ID_SPECIALITY");
                        }
                    });

                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show(error, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            #endregion

            if (idSpeciality == -1)
            {
                MessageBox.Show("Не вказано спеціальність лікаря!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(DoctorName.Text))
            {
                MessageBox.Show("Не вказано ПІБ лікаря!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cmdParams = new Dictionary<string, object>()
            {
                {"fullName",DoctorName.Text},
                {"phoneNum",PhoneNum.Text},
                {"idSpeciality",idSpeciality},
                {"idCategory",idCategory},
                {"visitable",Convert.ToInt32(IsVisitable.IsChecked)},
                {"idAddress",idAddress}
            };

            string? sql = null;

            if (Doctor == null)
            {
                sql = @"INSERT INTO DOCTORS (FULL_NAME, PHONE_NUM, SPECIALITY, ID_CATEGORY, VISITABLE, ID_ADDRESS)
                            VALUES (@fullName, @phoneNum, @idSpeciality, @idCategory, @visitable, @idAddress)";
            }
            else
            {
                sql = $@"UPDATE DOCTORS
                    SET FULL_NAME = @fullName,
                        PHONE_NUM = @phoneNum,
                        SPECIALITY = @idSpeciality,
                        ID_CATEGORY = @idCategory,
                        VISITABLE = @visitable,
                        ID_ADDRESS = @idAddress
                    WHERE ID_DOCTOR = {Doctor.IdDoctor}";
            }

            var errorUpd = DBHelper.DoCommand(sql, cmdParams);
            if (!string.IsNullOrEmpty(errorUpd))
            {
                MessageBox.Show(errorUpd, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Close();
        }
    }
}