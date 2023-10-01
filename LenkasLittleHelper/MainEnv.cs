using System.Windows;
using System;

namespace LenkasLittleHelper
{
    public class MainEnv
    {
        public enum ReportType
        {
            Undefined = 0,
            Fact = 1,
            Plan = 2
        }

        public static void ShowErrorDlg(string error)
        {
            MessageBox.Show($"Ти знаєш, до кого із цим звернутись... " +
                            $"{Environment.NewLine} {error}", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}