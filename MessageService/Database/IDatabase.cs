namespace MessageService.Data.Database
{
    using System.Data;

    public interface IDatabase
    {
        void ExecuteNonQuery(string query, Dictionary<string, object>? parameters = null);
        DataTable ExecuteQuery(string query, Dictionary<string, object>? parameters = null);
        void InitTables();
    }
}