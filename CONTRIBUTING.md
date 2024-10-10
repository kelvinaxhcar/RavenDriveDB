# Contribuindo para RavenDriveDB

Obrigado por considerar contribuir para o projeto! Este documento descreve o processo para contribuir de forma eficiente.

## Como Contribuir

1. **Fork o repositório**: Crie um fork do repositório para o seu perfil no GitHub.
2. **Clone o repositório**: Clone o fork no seu ambiente de desenvolvimento:
    ```bash
    git clone git@github.com:kelvinaxhcar/RavenDriveDB.git
    ```
3. **Crie uma branch**: Para realizar alterações, crie uma nova branch com um nome descritivo:
    ```bash
    git checkout -b nome-da-sua-branch
    ```
4. **Faça as alterações**: Implemente suas mudanças com commits claros e concisos.
5. **Teste suas mudanças**: Garanta que tudo esteja funcionando como esperado, e os testes estão passando:
    ```bash
    # Exemplo de execução de testes
    dotnet test
    ```
6. **Faça o commit das alterações**: Faça commits claros e utilize a seguinte convenção:
    ```bash
    git commit -m "Descrição clara do que foi alterado"
    ```
7. **Envie para o GitHub**:
    ```bash
    git push origin nome-da-sua-branch
    ```
8. **Crie um Pull Request**: Vá para a página do seu fork no GitHub e clique em "New Pull Request" para submeter suas mudanças para revisão.

## Estilo de Código

Por favor, siga as convenções de estilo do projeto:

- **C#**: Use o estilo padrão recomendado pelo .NET (convenções de nomenclatura PascalCase para classes e métodos, camelCase para variáveis).
  
Se tiver dúvidas, consulte o código existente ou peça ajuda!

## Issues

Para contribuir, você pode começar buscando por issues no projeto

## Revisão de Pull Requests

Todas as contribuições serão revisadas por membros do projeto. O tempo de resposta pode variar, mas nos esforçaremos para revisar PRs o mais rápido possível.

## Padrão de Commits

Utilize o seguinte padrão ao escrever suas mensagens de commit:
- `fix:` para correções de bugs.
- `feat:` para novas funcionalidades.
- `docs:` para alterações na documentação.
- `refactor:` para refatoração de código que não altere o comportamento externo.
