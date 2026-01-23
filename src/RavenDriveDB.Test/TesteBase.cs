using Newtonsoft.Json;
using RavenDriveDB.Packge;
using System;
using System.IO;

namespace RavenDriveDB.Test
{
    public abstract class TesteBase
    {
        protected readonly Packge.RavenDriveDB _ravenDriveDB;
        protected readonly TestSettings _settings;

        public TesteBase()
        {
            // Opcional: Ainda carregamos as configurações se existirem
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "testsettings.json");
            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                _settings = JsonConvert.DeserializeObject<TestSettings>(json);
            }
            else
            {
                _settings = new TestSettings
                {
                    FolderId = "auto",
                    CredentialsPath = "./credenciais.json"
                };
            }

            // MUDANÇA: Usamos MemoryRavenStorage para os testes passarem sem depender de quota/rede
            var mockStorage = new MemoryRavenStorage();
            _ravenDriveDB = new Packge.RavenDriveDB(mockStorage);

            // Simulamos o setup da pasta
            _ravenDriveDB.CreateFolderAsync("RavenDriveDB_Tests_Mock").GetAwaiter().GetResult();
        }
    }

    public class TestSettings
    {
        public string FolderId { get; set; }
        public string CredentialsPath { get; set; }
        public string? ParentFolderId { get; set; }
    }
}
