using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Verndale.RedirectManager.ViewModels
{
    public class ImportViewModel
    {
        [Required]
        public HttpPostedFileBase File { get; set; }

        public string ImportFile { get; set; }
    }
}