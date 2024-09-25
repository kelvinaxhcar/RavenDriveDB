namespace RavenDriveDB.Test {
public class Test_RavenDriveDB {
  private const string _credencialPath = @"./credenciais.json";
  private const string _idDaColecaoPasta = "12XHnuZsa26-XJL_uQJK2NVl-CqHEVLd5";
  private readonly Packge.RavenDriveDB _ravenDriveDB;

  public Test_RavenDriveDB() {
    _ravenDriveDB = new Packge.RavenDriveDB(_credencialPath, _idDaColecaoPasta);
  }

  [Fact]
  public async Task Deve_criar_documento() {
    var produto = new Produto { Nome = "Notebook 2", Preco = 3500.99M };

    var result = await _ravenDriveDB.CriarDocumentoAsync(produto);
    Assert.NotNull(result);
  }

  [Fact]
  public async Task Deve_consultar_documento() {
    var nome = $"Notebook {Guid.NewGuid()}";
    var produto = new Produto { Nome = nome, Preco = 3500.99M };

    var result = await _ravenDriveDB.CriarDocumentoAsync(produto);

    var produtosFiltrados =
        await _ravenDriveDB.ConsultarDocumentosAsync<Produto>(p => p.Nome ==
                                                                   nome) ??
        new List<Produto>();

    const int quantidadeEsperada = 1;
    Assert.Equal(quantidadeEsperada, produtosFiltrados.Count);
  }

  [Fact]
  public async Task Deve_atualizar_documento() {
    var nome = $"Notebook {Guid.NewGuid()} {nameof(Deve_atualizar_documento)}";

    var produto = new Produto { Nome = nome, Preco = 3500.99M };

    var id = await _ravenDriveDB.CriarDocumentoAsync(produto);

    var produtosFiltrados =
        await _ravenDriveDB.ConsultarDocumentosAsync<Produto>(p => p.Nome ==
                                                                   nome);

    var produtoEncontrado = produtosFiltrados.FirstOrDefault();

    produtoEncontrado.Preco = 2999.99M;

    await _ravenDriveDB.AtualizarDocumentoAsync(id, produtoEncontrado);

    var produtoLido = await _ravenDriveDB.LerDocumentoAsync<Produto>(id);

    Assert.Equal(2999.99M, produtoLido.Preco);
  }
}
}
