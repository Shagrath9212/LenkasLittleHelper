using NPOI.XSSF.UserModel;
using System.IO;

namespace LenkasLittleHelper.Helpers
{
    public class ExcelDocument : XSSFWorkbook
    {
        public static ExcelDocument? OpenFromTemplate(string path)
        {
            if (!File.Exists(path))
            {
                MainEnv.ShowErrorDlg($"Відсутній файл template.xlsx ({path})");
            }

            try
            {
                using (var stream = File.OpenRead(path))
                {
                    return new ExcelDocument(stream);
                }
            }
            catch (System.Exception)
            {
                MainEnv.ShowErrorDlg("Помилка при відкритті шаблону. Закрий файл template.xlsx, та спробуй знову.");
            }
            return null;
        }

        private ExcelDocument(Stream stream) : base(stream)
        {

        }

        private ExcelDocument() : base()
        {

        }

        public static ExcelDocument CreateNew()
        {
            return new ExcelDocument();
        }

        public void SaveAs(string fileName)
        {
            using (var stream = File.Create(fileName))
            {
                this.Write(stream);
            }
        }
    }
}