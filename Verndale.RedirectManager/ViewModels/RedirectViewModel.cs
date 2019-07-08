using System;
using System.Collections.Generic;
using Verndale.RedirectManager.Models;

namespace Verndale.RedirectManager.ViewModels
{
    public class RedirectViewModel
    {
        public const int PageSize = 20;

        public RedirectViewModel()
        {
            Items = new List<RedirectManagerModel>();
        }

        public string SelectedTab { get; set; }

        public string Term { get; set; }

        public int PageNumber { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages => (int)Math.Ceiling(((double)TotalItems) / PageSize);

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public IEnumerable<RedirectManagerModel> Items { get; set; }
    }
}