using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Newtonsoft.Json;

class SimulacaoRavenDbNoGoogleDrive
{
    private readonly DriveService _service;
    private readonly string _idDaColecaoPasta;

    private const string MimeTypeJson = "application/json";

    public SimulacaoRavenDbNoGoogleDrive(string credencialPath, string idDaColecaoPasta)
    {
        var credencial = GoogleCredential.FromFile(credencialPath)
                         .CreateScoped(DriveService.ScopeConstants.Drive);

        _service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credencial,
        });

        _idDaColecaoPasta = idDaColecaoPasta;
    }

    public async Task CriarDocumentoAsync<T>(T objeto) where T : class
    {
        try
        {
            string nomeDoDocumentoTemp = $"{typeof(T).Name}-{Guid.NewGuid()}-temp";
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

            string novoNome = $"{typeof(T).Name}-{uploadRequest.ResponseBody.Id}";
            await AtualizarNomeDocumentoAsync(uploadRequest.ResponseBody.Id, novoNome);

            Console.WriteLine($"Documento '{novoNome}' criado e atualizado com sucesso! ID: {uploadRequest.ResponseBody.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar documento: {ex.Message}");
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

            Console.WriteLine($"Documento com ID {documentoId} lido com sucesso e deserializado.");
            return objeto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler documento: {ex.Message}");
            throw;
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
                Console.WriteLine("Nenhum documento encontrado na coleção.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao listar documentos: {ex.Message}");
        }
    }

    public async Task<List<T>> ConsultarDocumentosAsync<T>(Func<T, bool> criterio) where T : class
    {
        var resultados = new List<T>();

        try
        {
            var request = _service.Files.List();
            request.Q = $"'{_idDaColecaoPasta}' in parents and mimeType='{MimeTypeJson}'";
            request.Fields = "files(id, name)";
            //request.PageSize = 100;

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
            Console.WriteLine($"Erro ao consultar documentos: {ex.Message}");
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

            Console.WriteLine($"Documento com ID {documentoId} atualizado com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar documento: {ex.Message}");
        }
    }
}

class Produto
{
    public string Id {
        get;
        set;
    }
    public string Nome {
        get;
        set;
    }
    public decimal Preco {
        get;
        set;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        string credencialPath = @"./credenciais.json";
        string idDaColecaoPasta = "12XHnuZsa26-XJL_uQJK2NVl-CqHEVLd5";

        var ravenDbSimulado = new SimulacaoRavenDbNoGoogleDrive(credencialPath, idDaColecaoPasta);

        var produto = new Produto
        {
            Nome = "Notebook 2",
            Preco = 3500.99M
        };

        //await ravenDbSimulado.CriarDocumentoAsync(produto);
        //var a = 0;
        //while (a < 200)
        //{
        //    await ravenDbSimulado.CriarDocumentoAsync(produto);
        //    a++;
        //}

        var produtosFiltrados = await ravenDbSimulado.ConsultarDocumentosAsync<Produto>(p => p.Nome == "Notebook");

        Console.WriteLine("Produtos com preço maior que 1000:");
        foreach (var p in produtosFiltrados)
        {
            Console.WriteLine($"- {p.Nome}: {p.Preco:C}");
        }

        var produtoAtualizado = new Produto
        {
            Id = "1HZatRt7Wn4zJHEhan87f8UrOXdhYov71", // ID do documento a ser atualizado
            Nome = "Notebook Atualizado ss",
            Preco = 2999.99M
        };

        await ravenDbSimulado.AtualizarDocumentoAsync(produtoAtualizado.Id, produtoAtualizado);

        var produtoLido = await ravenDbSimulado.LerDocumentoAsync<Produto>("1HZatRt7Wn4zJHEhan87f8UrOXdhYov71");
        Console.WriteLine($"Produto lido: {produtoLido.Nome} - {produtoLido.Preco:C}");
    }
}
