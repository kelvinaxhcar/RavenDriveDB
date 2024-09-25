using RavenDriveDB.Packge;

namespace RavenDriveDB.Test
{
    public class Test_RavenDriveDB
    {
        [Fact]
        public async Task Test1()
        {
            string credencialPath = @"./credenciais.json";
            string idDaColecaoPasta = "12XHnuZsa26-XJL_uQJK2NVl-CqHEVLd5";

            var ravenDbSimulado = new Packge.RavenDriveDB(credencialPath, idDaColecaoPasta);

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

    class Produto
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
    }
}