using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Newtonsoft.Json;

namespace RavenDriveDB.Packge
{
    public class RavenDriveDB
    {
        private readonly DriveService _service;
        private readonly string _idDaColecaoPasta;

        private const string MimeTypeJson = "application/json";

        public RavenDriveDB(string credencialPath, string idDaColecaoPasta)
        {
            var credencial = GoogleCredential.FromFile(credencialPath)
                .CreateScoped(DriveService.ScopeConstants.Drive);

            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credencial,
            });

            _idDaColecaoPasta = idDaColecaoPasta;
        }

        public async Task<string> CriarDocumentoAsync<T>(T objeto) where T : class
        {
            try
            {
                var propriedades = typeof(T).GetProperties()
                                            .Select(prop => $"{prop.Name}:{prop.GetValue(objeto)}")
                                            .ToArray();

                string propriedadesNoNome = string.Join("_", propriedades);

                string nomeDoDocumentoTemp = $"{typeof(T).Name}-{Guid.NewGuid()}-{propriedadesNoNome}-temp";
                string conteudoDoDocumento = JsonConvert.SerializeObject(objeto, Formatting.Indented);

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = nomeDoDocumentoTemp,
                    Parents = new List<string> { _idDaColecaoPasta }
                };

                using var mediaContent = CriarMemoryStream(conteudoDoDocumento);

                var uploadRequest = _service.Files.Create(fileMetadata, mediaContent, MimeTypeJson);
                uploadRequest.Fields = "*";

                var progress = await uploadRequest.UploadAsync();
                ValidarProgressoUpload(progress);

                string novoNome = $"{typeof(T).Name}-{uploadRequest.ResponseBody.Id}-{propriedadesNoNome}";
                await AtualizarNomeDocumentoAsync(uploadRequest.ResponseBody.Id, novoNome);

                return uploadRequest.ResponseBody.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar documento: {ex.Message}");
            }
        }


        private static MemoryStream CriarMemoryStream(string conteudo)
        {
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(conteudo));
        }

        private static void ValidarProgressoUpload(IUploadProgress progress)
        {
            if (progress.Status == UploadStatus.Failed)
            {
                throw new Exception($"Erro no upload: {progress.Exception?.Message}");
            }
        }

        private async Task AtualizarNomeDocumentoAsync(string documentoId, string novoNome)
        {
            var updateMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = novoNome
            };

            var updateRequest = _service.Files.Update(updateMetadata, documentoId);
            updateRequest.Fields = "id, name";
            await updateRequest.ExecuteAsync();
        }

        public async Task<T> LerDocumentoAsync<T>(string documentoId)
        {
            try
            {
                var getRequest = _service.Files.Get(documentoId);
                using var stream = new MemoryStream();
                await getRequest.DownloadAsync(stream);

                string conteudoJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());

                T objeto = JsonConvert.DeserializeObject<T>(conteudoJson);

                return objeto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao ler documento: {ex.Message}");
            }
        }

        public async Task ListarDocumentosAsync()
        {
            try
            {
                var request = _service.Files.List();
                request.Q = $"'{_idDaColecaoPasta}' in parents";
                var result = await request.ExecuteAsync();

                Console.WriteLine($"Documentos na coleção (ID: {_idDaColecaoPasta}):");
                if (result.Files != null && result.Files.Any())
                {
                    foreach (var file in result.Files)
                    {
                        Console.WriteLine("{0} ({1})", file.Name, file.Id);
                    }
                }
                else
                {
                    throw new Exception("Nenhum documento encontrado na coleção.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao listar documentos: {ex.Message}");
            }
        }

        public async Task<List<T>> ConsultarDocumentosAsync<T>(Func<T, bool> criterio, List<string> filtros = null) where T : class
        {
            var resultados = new List<T>();

            try
            {
                var request = _service.Files.List();
                var query = $"'{_idDaColecaoPasta}' in parents and mimeType='{MimeTypeJson}'";

                if (filtros != null && filtros.Any())
                {
                    var filtroQuery = string.Join(" and ", filtros.Select(f => $"name contains '{f}'"));
                    query += $" and ({filtroQuery})";
                }

                request.Q = query;
                request.Fields = "files(id, name)";

                var result = await request.ExecuteAsync();

                if (result.Files != null && result.Files.Any())
                {
                    var tarefas = result.Files.Select(file => LerDocumentoAsync<T>(file.Id));
                    var documentos = await Task.WhenAll(tarefas);

                    resultados.AddRange(documentos.Where(d => d != null && criterio(d)));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao consultar documentos: {ex.Message}");
            }

            return resultados;
        }

        public async Task AtualizarDocumentoAsync<T>(string documentoId, T novoObjeto) where T : class
        {
            try
            {
                string conteudoAtualizado = JsonConvert.SerializeObject(novoObjeto, Formatting.Indented);
                using var mediaContent = CriarMemoryStream(conteudoAtualizado);

                var updateMetadata = new Google.Apis.Drive.v3.Data.File();
                var updateRequest = _service.Files.Update(updateMetadata, documentoId, mediaContent, MimeTypeJson);
                updateRequest.Fields = "id, name";

                var updatedFile = await updateRequest.UploadAsync();

                ValidarProgressoUpload(updatedFile);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar documento: {ex.Message}");
            }
        }
    }
}
