namespace RavenDriveDB.Test
{
    public class Test_RavenDriveDB
    {
        private const string _credencialPath = @"./credenciais.json";
        private const string _idDaColecaoPasta = "12XHnuZsa26-XJL_uQJK2NVl-CqHEVLd5";
        private readonly Packge.RavenDriveDB _ravenDriveDB;

        public Test_RavenDriveDB()
        {
            _ravenDriveDB = new Packge.RavenDriveDB(_credencialPath, _idDaColecaoPasta);
        }

        [Fact]
        public async Task Deve_criar_documento()
        {
            var produto = new MinhaClasse
            {
                Nome = "Notebook 2",
                Preco = 3500.99M
            };

            var result = await _ravenDriveDB.StoreAsync(produto);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Deve_consultar_documento()
        {
            var nome = $"Notebook {Guid.NewGuid()}";
            var produto = new MinhaClasse
            {
                Nome = nome,
                Preco = 3500.99M
            };

            var result = await _ravenDriveDB.StoreAsync(produto);
            await Task.Delay(10);

            var produtosFiltrados = await _ravenDriveDB.GetAsync<MinhaClasse>(p => p.Nome == nome, new List<string>() {$"Nome:{nome}" }) ?? new List<MinhaClasse>();

            const int quantidadeEsperada = 1;
            Assert.Equal(quantidadeEsperada, produtosFiltrados.Count);
        }


        [Fact]
        public async Task Deve_atualizar_documento()
        {
            var nome = $"Notebook {Guid.NewGuid()} {nameof(Deve_atualizar_documento)}";

            var produto = new MinhaClasse
            {
                Nome = nome,
                Preco = 3500.99M
            };

            var id = await _ravenDriveDB.StoreAsync(produto);
            await Task.Delay(10);

            var produtosFiltrados = await _ravenDriveDB.GetAsync<MinhaClasse>(p => p.Nome == nome, new List<string>() { $"Nome:{nome}" });

            var produtoEncontrado = produtosFiltrados.FirstOrDefault();

            produtoEncontrado.Preco = 2999.99M;

            await _ravenDriveDB.UpdateAsync(id, produtoEncontrado);

            var produtoLido = await _ravenDriveDB.LoadAsync<MinhaClasse>(id);

            Assert.Equal(2999.99M, produtoLido.Preco);
        }
    }
}