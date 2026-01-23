using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RavenDriveDB.Packge
{
    public class RavenDriveDB
    {
        private readonly IRavenStorage _storage;
        public string? IdDaColecaoPasta { get; private set; }
        private const string MimeTypeJson = "application/json";

        /// <summary>
        /// Construtor para uso em produção (Google Drive).
        /// </summary>
        public RavenDriveDB(string credencialPath, string? idDaColecaoPasta = null)
        {
            _storage = new GoogleDriveStorage(credencialPath);
            IdDaColecaoPasta = idDaColecaoPasta;
        }

        /// <summary>
        /// Construtor para Injeção de Dependência (Testes ou outros provedores).
        /// </summary>
        public RavenDriveDB(IRavenStorage storage, string? idDaColecaoPasta = null)
        {
            _storage = storage;
            IdDaColecaoPasta = idDaColecaoPasta;
        }

        public async Task<string> CreateFolderAsync(string folderName, string? parentFolderId = null)
        {
            IdDaColecaoPasta = await _storage.CreateFolderAsync(folderName, parentFolderId);
            return IdDaColecaoPasta;
        }

        public void SetFolderId(string folderId)
        {
            IdDaColecaoPasta = folderId;
        }

        public async Task<string?> FindFolderByNameAsync(string folderName, string? parentId = null)
        {
            return await _storage.FindFolderByNameAsync(folderName, parentId);
        }

        public async Task<string> StoreAsync<T>(T objeto) where T : class
        {
            if (string.IsNullOrEmpty(IdDaColecaoPasta))
                throw new RavenDriveException("ID da coleção não configurado. Chame CreateFolderAsync ou SetFolderId.");

            string propriedadesNoNome = ObterNomesDasPropriedades(objeto);
            string nomeDoDocumento = $"{typeof(T).Name}-{Guid.NewGuid()}-{propriedadesNoNome}-temp";

            var id = await _storage.StoreAsync(objeto, IdDaColecaoPasta, nomeDoDocumento);

            // Atualiza o nome para incluir o ID do Storage (RavenDB style)
            string novoNome = $"{typeof(T).Name}-{id}-{propriedadesNoNome}";
            await _storage.UpdateAsync(id, objeto, novoNome);

            return id;
        }

        public async Task<T> LoadAsync<T>(string documentoId)
        {
            return await _storage.LoadAsync<T>(documentoId);
        }

        public async Task<List<T>> GetAsync<T>(Func<T, bool> criterio, List<string> filtros = null) where T : class
        {
            if (string.IsNullOrEmpty(IdDaColecaoPasta))
                throw new RavenDriveException("ID da coleção não configurado.");

            filtros ??= new List<string>();
            filtros.Add($"{typeof(T).Name}-");

            var query = $"mimeType='{MimeTypeJson}'";
            if (filtros.Any())
            {
                var filtroQuery = string.Join(" and ", filtros.Select(f => $"name contains '{f}'"));
                query += $" and ({filtroQuery})";
            }

            var files = await _storage.ListFilesAsync(IdDaColecaoPasta, query);

            // Download paralelo
            var tarefas = files.Select(f => _storage.LoadAsync<T>(f.Id));
            var documentos = await Task.WhenAll(tarefas);

            return documentos.Where(d => d != null && (criterio == null || criterio(d))).ToList();
        }

        public async Task UpdateAsync<T>(string documentoId, T novoObjeto) where T : class
        {
            string propriedadesNoNome = ObterNomesDasPropriedades(novoObjeto);
            string novoNome = $"{typeof(T).Name}-{documentoId}-{propriedadesNoNome}";
            await _storage.UpdateAsync(documentoId, novoObjeto, novoNome);
        }

        public async Task DeleteAsync(string documentoId)
        {
            await _storage.DeleteAsync(documentoId);
        }

        public async Task DeleteAllAsync()
        {
            if (string.IsNullOrEmpty(IdDaColecaoPasta)) return;

            var files = await _storage.ListFilesAsync(IdDaColecaoPasta, $"trashed = false");
            var tarefas = files.Select(f => _storage.DeleteAsync(f.Id));
            await Task.WhenAll(tarefas);
        }

        private static string ObterNomesDasPropriedades<T>(T novoObjeto) where T : class
        {
            var propriedades = typeof(T).GetProperties()
                                                        .Where(prop => prop.Name != "Id")
                                                        .Select(prop => $"{prop.Name}:{prop.GetValue(novoObjeto)}")
                                                        .ToArray();

            string propriedadesNoNome = string.Join("_", propriedades);
            if (propriedadesNoNome.Length > 100)
                propriedadesNoNome = propriedadesNoNome.Substring(0, 97) + "...";

            return propriedadesNoNome;
        }

    }
}


