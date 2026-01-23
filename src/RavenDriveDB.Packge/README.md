# RavenDriveDB
RavenDriveDB é uma solução que simula as funcionalidades de um banco de dados RavenDB, utilizando o Google Drive como meio de armazenamento de documentos JSON. Este projeto permite a criação, leitura, atualização e consulta de documentos diretamente no Google Drive, oferecendo uma alternativa simples para quem precisa de uma simulação de banco de dados baseada em documentos sem a necessidade de infraestrutura complexa. Ideal para protótipos, pequenas aplicações ou ambientes de teste, o DriveDB facilita o gerenciamento de dados em um ambiente distribuído, utilizando a API do Google Drive.


## 🚀 Configuração do Google Drive

Para que o **RavenDriveDB** funcione, você precisa configurar uma Conta de Serviço no Google Cloud e ativar a API do Google Drive. Siga os passos abaixo:

### 1. Criar um Projeto e Ativar a API
1. Acesse o [Google Cloud Console](https://console.cloud.google.com/).
2. Crie um novo projeto (ou selecione um existente).
3. Vá em **Ativar APIs e Serviços** ou acesse diretamente a [Biblioteca de APIs](https://console.cloud.google.com/apis/library).
4. Pesquise por **"Google Drive API"** e clique em **Ativar**.

### 2. Criar Conta de Serviço e Chave JSON
1. No menu lateral, vá em **APIs e Serviços > Credenciais**.
2. Clique em **+ Criar Credenciais** e selecione **Conta de serviço**.
3. Preencha o nome (ex: `ravendrivedb-service`) e clique em **Criar e Continuar**.
4. Em **Papel (Role)**, selecione **Editor** (ou um papel com permissões de leitura/escrita no Drive) e clique em **Concluir**.
5. Na lista de "Contas de serviço", clique no e-mail da conta que você acabou de criar.
6. Vá na aba **Chaves**, clique em **Adicionar Chave > Criar nova chave**.
7. Selecione o formato **JSON** e clique em **Criar**. O download do arquivo será iniciado automaticamente.
   > [!IMPORTANT]  
   > Guarde este arquivo JSON com segurança. Ele contém as credenciais que sua aplicação usará para se autenticar.

### 3. Compartilhar a Pasta do Drive (Obrigatório)
Uma Conta de Serviço funciona como um "usuário virtual" com seu próprio e-mail (ex: `nome-da-conta@projeto.iam.gserviceaccount.com`).
1. Abra o seu Google Drive convencional.
2. Localize ou crie a pasta que você deseja usar como banco de dados.
3. Clique com o botão direito na pasta > **Compartilhar**.
4. Adicione o e-mail da **Conta de Serviço** (aquele criado no passo anterior) com permissão de **Editor**.
   > [!WARNING]  
   > Se você não compartilhar a pasta com o e-mail da Conta de Serviço, o RavenDriveDB retornará erro de "Arquivo não encontrado", pois ele não terá permissão para ver seus arquivos pessoais.
