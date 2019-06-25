using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EPiServer.Data;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Verndale.RedirectManager.Models;
using Verndale.RedirectManager.Util;

namespace Verndale.RedirectManager.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IRedirectManagerRepository))]
    public class RedirectManagerRepository : IRedirectManagerRepository
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        public RedirectManagerRepository()
        {
            SqlUtil.Initialize(GetConnection);
        }

        public IEnumerable<RedirectManagerModel> GetAll()
        {
            var sql = "SELECT * FROM RedirectManager";

            Logger.Debug("Start GetAll()");
            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteQueryReader(sql, RedirectManagerModel.LoadFromRecord);
        }

        public int Delete(int id)
        {
            var sql = $"DELETE FROM RedirectManager WHERE id = {id}";

            Logger.Debug("Start Delete()");
            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteNonQuery(sql);
        }

        public int DeleteAll()
        {
            var sql = $"DELETE FROM RedirectManager";

            Logger.Debug("Start DeleteAll()");
            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteNonQuery(sql);
        }

        public RedirectManagerModel GetById(int id)
        {
            var sql = $"SELECT * FROM RedirectManager WHERE id = {id}";

            Logger.Debug("Start GetById()");
            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteQueryReader(sql, RedirectManagerModel.LoadFromRecord).FirstOrDefault();
        }

        public RedirectManagerModel Get(string oldUrl)
        {
            Logger.Debug($"Start Get({oldUrl})");

            if (string.IsNullOrEmpty(oldUrl))
            {
                Logger.Debug("oldUrl is invalid.");
                return null;
            }

            var sql = $"SELECT * FROM RedirectManager WHERE '{oldUrl}' like '%' + OldUrl + '%' " +
                      $"ORDER BY CASE WHEN OldUrl = '{oldUrl}' THEN 0 ELSE 1 END";

            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteQueryReader(sql, RedirectManagerModel.LoadFromRecord).FirstOrDefault();
        }

        public IEnumerable<RedirectManagerModel> GetAll(string term, int page, int pageSize)
        {
            var sql = $"SELECT * FROM RedirectManager WHERE OldUrl like '%{term}%' ORDER BY id OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY;";

            Logger.Debug($"Start GetAll({term}, {page}, {pageSize})");
            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteQueryReader(sql, RedirectManagerModel.LoadFromRecord);
        }

        public int InsertOrUpdate(RedirectManagerModel model)
        {
            var sql = $"insert into RedirectManager values ('{model.OldUrl}', '{model.NewUrl}', {model.Type}, {(model.IncludeQuery ? 1 : 0)})";

            if (model.Id > 0)
            {
                sql = $"UPDATE RedirectManager SET OldUrl = '{model.OldUrl}', NewUrl = '{model.NewUrl}', RedirectType = {model.Type}, IncludeQuery = {(model.IncludeQuery ? 1 : 0)} where id = {model.Id}";
            }

            Logger.Debug("Start InsertOrUpdate()");
            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteNonQuery(sql);
        }

        public int RemoveAndInsert(RedirectManagerModel model)
        {
            var sql = $"delete from RedirectManager where OldUrl = '{model.OldUrl}'; " +
                      $"insert into RedirectManager values ('{model.OldUrl}', '{model.NewUrl}', {model.Type}, {(model.IncludeQuery ? 1 : 0)})";

            Logger.Debug("Start RemoveAndInsert()");
            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteNonQuery(sql);
        }

        public int Count()
        {
            const string sql = "SELECT COUNT(*) FROM RedirectManager";

            Logger.Debug("Start Count()");
            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteScalar<int>(sql);
        }

        public int Count(string term)
        {
            var sql = $"SELECT COUNT(*) FROM RedirectManager WHERE OldUrl like '%{term}%'";

            Logger.Debug($"Start Count({term})");
            Logger.Debug($"SQL: {sql}");

            return SqlUtil.ExecuteScalar<int>(sql);
        }

        private SqlConnection GetConnection()
        {
            var databaseHandler = ServiceLocator.Current.GetInstance<IDatabaseHandler>();

            Logger.Debug("Start GetConnection()");
            Logger.Debug($"ConnectionString: {databaseHandler.ConnectionSettings.ConnectionString}");

            
            return new SqlConnection(databaseHandler.ConnectionSettings.ConnectionString);
        }
    }
}