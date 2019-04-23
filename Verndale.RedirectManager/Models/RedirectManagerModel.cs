using System.Data;
using Verndale.RedirectManager.Util;

namespace Verndale.RedirectManager.Models
{
    public class RedirectManagerModel
    {
        public RedirectManagerModel()
        {
            IncludeQuery = true;
        }

        public int Id { get; set; }
        public string OldUrl { get; set; }
        public string NewUrl { get; set; }
        public int Type { get; set; }
        public bool IncludeQuery { get; set; }

        public static RedirectManagerModel LoadFromRecord(IDataRecord record)
        {
            var item = new RedirectManagerModel
            {
                Id = record.SafeGetNumber("Id"),
                OldUrl = record.SafeGetString("OldUrl"),
                NewUrl = record.SafeGetString("NewUrl"),
                Type = record.SafeGetNumber("RedirectType"),
                IncludeQuery = record.SafeGetNumber("IncludeQuery") == 1
            };

            return item;
        }
    }
}