namespace MessageService.Services
{
    using MessageService.Data.Database;
    using Npgsql;
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly IDatabase _database;

        public DatabaseService(IDatabase database)
        {
            _database = database;
        }

        public void CreateTables()
        {
            _database.InitTables();
        }
    }
}