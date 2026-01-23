using RavenDriveDB.Packge;

namespace RavenDriveDB.Test
{
    public class Test_RavenDriveDB : TesteBase
    {
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

        [Fact]
        public async Task Deve_excluir_documento()
        {
            var produto = new MinhaClasse { Nome = "Para Excluir", Preco = 100 };
            var id = await _ravenDriveDB.StoreAsync(produto);

            await _ravenDriveDB.DeleteAsync(id);

            await Assert.ThrowsAsync<RavenDriveNotFoundException>(() => _ravenDriveDB.LoadAsync<MinhaClasse>(id));
        }

        [Fact]
        public async Task Deve_excluir_todos_os_documentos()
        {
            await _ravenDriveDB.StoreAsync(new MinhaClasse { Nome = "Bulk 1" });
            await _ravenDriveDB.StoreAsync(new MinhaClasse { Nome = "Bulk 2" });

            await _ravenDriveDB.DeleteAllAsync();

            var resultados = await _ravenDriveDB.GetAsync<MinhaClasse>(null);
            Assert.Empty(resultados);
        }
    }
}
