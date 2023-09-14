using System;

namespace LenkasLittleHelper.Helpers
{
    internal static class Extensions
    {
        private const string DtFormat = "dd/MM/yyyy HH:mm:ss";
        private const string DFormat = "dd/MM/yyyy";

        public static string ToDBFormat(this DateTime dt)
        {
            return dt.ToString(DtFormat);
        }
        public static string ToDBFormat_DateOnly(this DateTime dt)
        {
            return dt.ToString(DFormat);
        }
    }
}