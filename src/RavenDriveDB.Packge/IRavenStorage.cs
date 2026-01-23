using System.Collections.Generic;
using System.Threading.Tasks;

namespace RavenDriveDB.Packge
{
    public interface IRavenStorage
    {
        Task<string> CreateFolderAsync(string folderName, string? parentFolderId = null);
        Task<string?> FindFolderByNameAsync(string folderName, string? parentFolderId = null);
        Task<string> StoreAsync<T>(T objeto, string folderId, string nomeDoDocumento) where T : class;
        Task<T> LoadAsync<T>(string fileId);
        Task<List<RavenFileInfo>> ListFilesAsync(string folderId, string query);
        Task UpdateAsync<T>(string fileId, T objeto, string novoNome) where T : class;
        Task DeleteAsync(string fileId);
    }

    public class RavenFileInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
