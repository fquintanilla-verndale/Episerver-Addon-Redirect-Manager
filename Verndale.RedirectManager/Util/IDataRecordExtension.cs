using System;
using System.Data;

namespace Verndale.RedirectManager.Util
{
    public static class IDataRecordExtension
    {
        public static string SafeGetString(this IDataRecord record, int colIndex)
        {
            return !record.IsDBNull(colIndex) ? record.GetString(colIndex) : string.Empty;
        }

        public static string SafeGetString(this IDataRecord record, string colName)
        {
            return !(Convert.IsDBNull(record[colName])) ? record[colName].ToString() : string.Empty;
        }

        public static int SafeGetNumber(this IDataRecord record, int colIndex)
        {
            return !record.IsDBNull(colIndex) ? record.GetInt32(colIndex) : 0;
        }

        public static int SafeGetNumber(this IDataRecord record, string colName)
        {
            return !(Convert.IsDBNull(record[colName])) ? Convert.ToInt32(record[colName]) : 0;
        }
    }
}