using NPOI.XSSF.UserModel;
using System.IO;

namespace LenkasLittleHelper.Helpers
{
    public class ExcelDocument : XSSFWorkbook
    {
        public static XSSFWorkbook? OpenFromTemplate(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using (var stream = File.OpenRead(path))
            {

            }

            return null;
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