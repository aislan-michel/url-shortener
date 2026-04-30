# UrlShortener

Aplicação ASP.NET Core MVC para encurtamento de URLs com geração de QR Code.

## Visão geral

- Projeto principal: `src/UrlShortener.App`
- Testes: `tests/UrlShortener.App.Tests`
- Plataforma: .NET 9.0
- Biblioteca adicional: `QRCoder`
- Containerização: `Dockerfile` incluído em `src/UrlShortener.App`

## Pré-requisitos

- .NET 9 SDK
- Docker (opcional para execução em container)
- Visual Studio, VS Code ou outro editor de sua preferência

## Executar localmente

No terminal, entre na pasta do projeto:

```bash
cd src/UrlShortener.App
```

Execute a aplicação:

```bash
dotnet run
```

A aplicação estará disponível em:

```bash
http://localhost:5000
```

> Observação: o ASP.NET Core pode escolher uma porta dinâmica em função da sua configuração de launch. Verifique a saída do `dotnet run`.

## Build e publicação

Para compilar em modo Release:

```bash
cd src/UrlShortener.App

dotnet build -c Release
```

Publicar para produção:

```bash
cd src/UrlShortener.App

dotnet publish -c Release -o publish
```

## Executar com Docker

No terminal, vá para a pasta do projeto:

```bash
cd src/UrlShortener.App
```

Construa a imagem Docker:

```bash
docker build -t urlshortener .
```

Execute o container:

```bash
docker run -d -p 8080:80 --name urlshortener urlshortener
```

Acesse em:

```bash
http://localhost:8080
```

Para parar o container:

```bash
docker stop urlshortener
```

Para remover o container:

```bash
docker rm urlshortener
```

## Testes

Para executar os testes:

```bash
cd tests/UrlShortener.App.Tests

dotnet test
```

## Estrutura do projeto

- `src/UrlShortener.App`
  - `Controllers/`
  - `Models/`
  - `Views/`
  - `Infrastructure/`
  - `wwwroot/`
- `tests/UrlShortener.App.Tests`

## Observações

- O projeto armazena dados em memória e serve como exemplo de aplicação simples de encurtamento.
- O QR Code é gerado com `QRCoder`.
