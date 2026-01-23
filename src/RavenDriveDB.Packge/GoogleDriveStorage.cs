using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Newtonsoft.Json;

namespace RavenDriveDB.Packge
{
    public class GoogleDriveStorage : IRavenStorage
    {
        private readonly DriveService _service;
        private const string MimeTypeJson = "application/json";

        public GoogleDriveStorage(string credencialPath)
        {
            try
            {
                var json = File.ReadAllText(credencialPath);

                if (json.Contains("\"web\":") || json.Contains("\"installed\":"))
                {
                    throw new RavenDriveAuthException(
                        "O arquivo de credenciais parece ser um 'OAuth 2.0 Client ID' em vez de uma 'Service Account Key'.");
                }

                var credencial = GoogleCredential.FromJson(json)
                    .CreateScoped(DriveService.ScopeConstants.Drive);

                _service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credencial,
                });
            }
            catch (Exception ex) when (ex is not RavenDriveException)
            {
                throw TratarExcecao("Erro ao inicializar GoogleDriveStorage", ex);
            }
        }

        public async Task<string> CreateFolderAsync(string folderName, string? parentFolderId = null)
        {
            try
            {
                var folderMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };

                if (!string.IsNullOrEmpty(parentFolderId))
                {
                    folderMetadata.Parents = new List<string> { parentFolderId };
                }

                var request = _service.Files.Create(folderMetadata);
                request.SupportsAllDrives = true;
                request.Fields = "id";
                var folder = await request.ExecuteAsync();

                return folder.Id;
            }
            catch (Exception ex)
            {
                throw TratarExcecao("Erro ao criar pasta", ex);
            }
        }

        public async Task<string?> FindFolderByNameAsync(string folderName, string? parentId = null)
        {
            try
            {
                var request = _service.Files.List();
                var query = $"name = '{folderName}' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
                if (!string.IsNullOrEmpty(parentId))
                {
                    query += $" and '{parentId}' in parents";
                }

                request.Q = query;
                request.SupportsAllDrives = true;
                request.IncludeItemsFromAllDrives = true;
                request.Fields = "files(id)";

                var result = await request.ExecuteAsync();
                return result.Files?.FirstOrDefault()?.Id;
            }
            catch (Exception ex)
            {
                throw TratarExcecao("Erro ao buscar pasta", ex);
            }
        }

        public async Task<string> StoreAsync<T>(T objeto, string folderId, string nomeDoDocumento) where T : class
        {
            try
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = nomeDoDocumento,
                    Parents = new List<string> { folderId }
                };

                string conteudo = JsonConvert.SerializeObject(objeto, Formatting.Indented);
                using var mediaContent = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(conteudo));

                var uploadRequest = _service.Files.Create(fileMetadata, mediaContent, MimeTypeJson);
                uploadRequest.SupportsAllDrives = true;
                uploadRequest.Fields = "id";

                var progress = await uploadRequest.UploadAsync();
                if (progress.Status == UploadStatus.Failed)
                {
                    throw progress.Exception;
                }

                return uploadRequest.ResponseBody.Id;
            }
            catch (Exception ex)
            {
                throw TratarExcecao("Erro ao armazenar documento", ex);
            }
        }

        public async Task<T> LoadAsync<T>(string fileId)
        {
            try
            {
                var getRequest = _service.Files.Get(fileId);
                using var stream = new MemoryStream();
                await getRequest.DownloadAsync(stream);

                string conteudoJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                return JsonConvert.DeserializeObject<T>(conteudoJson);
            }
            catch (Exception ex)
            {
                throw TratarExcecao("Erro ao carregar documento", ex);
            }
        }

        public async Task<List<RavenFileInfo>> ListFilesAsync(string folderId, string query)
        {
            try
            {
                var request = _service.Files.List();
                request.SupportsAllDrives = true;
                request.IncludeItemsFromAllDrives = true;
                request.Q = $"'{folderId}' in parents and {query}";
                request.Fields = "files(id, name)";

                var result = await request.ExecuteAsync();
                return result.Files?.Select(f => new RavenFileInfo { Id = f.Id, Name = f.Name }).ToList() ?? new List<RavenFileInfo>();
            }
            catch (Exception ex)
            {
                throw TratarExcecao("Erro ao listar arquivos", ex);
            }
        }

        public async Task UpdateAsync<T>(string fileId, T objeto, string novoNome) where T : class
        {
            try
            {
                var updateMetadata = new Google.Apis.Drive.v3.Data.File { Name = novoNome };
                string conteudo = JsonConvert.SerializeObject(objeto, Formatting.Indented);
                using var mediaContent = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(conteudo));

                var updateRequest = _service.Files.Update(updateMetadata, fileId, mediaContent, MimeTypeJson);
                updateRequest.SupportsAllDrives = true;
                updateRequest.Fields = "id";

                var progress = await updateRequest.UploadAsync();
                if (progress.Status == UploadStatus.Failed)
                {
                    throw progress.Exception;
                }
            }
            catch (Exception ex)
            {
                throw TratarExcecao("Erro ao atualizar documento", ex);
            }
        }

        public async Task DeleteAsync(string fileId)
        {
            try
            {
                var request = _service.Files.Delete(fileId);
                request.SupportsAllDrives = true;
                await request.ExecuteAsync();
            }
            catch (Exception ex)
            {
                throw TratarExcecao("Erro ao excluir arquivo", ex);
            }
        }

        private RavenDriveException TratarExcecao(string context, Exception ex)
        {
            if (ex is Google.GoogleApiException apiEx)
            {
                if (apiEx.HttpStatusCode == System.Net.HttpStatusCode.Forbidden && 
                    apiEx.Message.Contains("quota", StringComparison.OrdinalIgnoreCase))
                {
                    return new RavenDriveQuotaException($"{context}: Cota de armazenamento excedida.", ex);
                }
                if (apiEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new RavenDriveNotFoundException($"{context}: Recurso não encontrado.", ex);
                }
            }
            return new RavenDriveException($"{context}: {ex.Message}", ex);
        }
    }
}
