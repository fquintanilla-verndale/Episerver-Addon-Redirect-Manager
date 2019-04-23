using System.Collections.Generic;

namespace Verndale.RedirectManager.ViewModels
{
    public class ImportResultViewModel
    {
        public ImportResultViewModel()
        {
            ImportErrorItems = new List<ImportErrorItem>();
        }

        public int TotalImported { get; set; }

        public int TotalErrors => ImportErrorItems.Count;

        public List<ImportErrorItem> ImportErrorItems { get; set; }

        public void AddError(int line, string message)
        {
            ImportErrorItems.Add(new ImportErrorItem(line, message));
        }
    }

    public class ImportErrorItem
    {
        public ImportErrorItem(int line, string message)
        {
            Line = line;
            Message = message;
        }

        public int Line { get; set; }
        public string Message { get; set; }
    }
}