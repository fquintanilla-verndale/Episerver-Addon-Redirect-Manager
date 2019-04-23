using System.Collections.Generic;
using Verndale.RedirectManager.Models;

namespace Verndale.RedirectManager.Repositories
{
    public interface IRedirectManagerRepository
    {
        IEnumerable<RedirectManagerModel> GetAll();

        int Delete(int id);

        int DeleteAll();

        RedirectManagerModel GetById(int id);

        RedirectManagerModel Get(string oldUrl);

        IEnumerable<RedirectManagerModel> GetAll(string term, int page, int pageSize);

        int InsertOrUpdate(RedirectManagerModel model);

        int Count();

        int Count(string term);
    }
}