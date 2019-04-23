using System.Data.SqlClient;
using EPiServer.Data;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace Verndale.RedirectManager.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(DataInitialization))]
    public class RedirectManagerInitializer : IInitializableModule
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private static readonly string PageViewsDataSqlCreateColumns =
            @"[OldUrl] nvarchar(512) not null,  
             [NewUrl] nvarchar(512) not null,
             [RedirectType] int not null,";

        public void Initialize(InitializationEngine initializationEngine)
        {
            Logger.Debug("Start Initialize()");
            CreateTable();
        }


        public void Uninitialize(InitializationEngine initializationEngine)
        {
        }

        private void CreateTable()
        {
            Logger.Debug("Start CreateTable()");

            var databaseHandler = ServiceLocator.Current.GetInstance<IDatabaseHandler>();

            var sqlCreateTable =
            @"  if OBJECT_ID('dbo.RedirectManager', 'U') is null 
                begin
                    create table [dbo].[RedirectManager] 
                    (
                         [Id] int identity primary key,
                         [OldUrl] nvarchar(800) not null,  
                         [NewUrl] nvarchar(800) not null,
                         [RedirectType] int not null,
                         [IncludeQuery] bit
                    );
                    create index idx_oldurl on RedirectManager(OldUrl);
                end";

            Logger.Debug($"Sql: {sqlCreateTable}");

            using (var connection = new SqlConnection(databaseHandler.ConnectionSettings.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sqlCreateTable;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}