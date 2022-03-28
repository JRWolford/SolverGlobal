using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Database
{
    public interface IDataRepository
    {
        Task<bool> TableExistsAsync(string tableName);
        Task<List<Dictionary<string, object>>> GetDataAsync(string tableName);
        Task<object> GetPagedDataAsync(string tableName, int pageSize, int currentPage);
    }
}
