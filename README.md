# UrlShortener

Aplicacao ASP.NET Core MVC para encurtamento e gerenciamento de URLs. O projeto permite criar links curtos, redirecionar acessos pelo codigo, acompanhar cliques, definir data de expiracao, gerar QR Code e validar URLs em segundo plano.

## Recursos

- Criacao de URLs curtas com codigo personalizado ou gerado automaticamente.
- Redirecionamento pela rota `/{shortCode}`.
- Listagem de links com busca por codigo e paginacao.
- Tela de detalhes com URL original, URL curta, status, expiracao e contador de cliques.
- Edicao da data de expiracao.
- Ativacao, desativacao e exclusao de links.
- Geracao de QR Code para compartilhamento.
- Controle de status: `Processing`, `Active`, `Inactive` e `Invalid`.
- Validacao periodica das URLs em processamento por um `BackgroundService`.

## Tecnologias

- .NET 9
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- QRCoder
- Bootstrap
- Bootstrap Icons
- jQuery
- xUnit
- Moq

## Persistencia

A aplicacao usa SQLite com Entity Framework Core. A connection string padrao fica em `src/UrlShortener.App/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=urlshortener.db"
}
```

Na inicializacao, o banco e criado automaticamente com `EnsureCreated()`. Quando a tabela de URLs esta vazia, a aplicacao tambem cadastra alguns links de exemplo.

## Pre-requisitos

- .NET 9 SDK
- Docker, opcional para execucao em container

## Como executar

Execute os comandos a partir da raiz do repositorio.

Restaure as dependencias:

```bash
dotnet restore
```

Compile a solucao:

```bash
dotnet build
```

Execute a aplicacao:

```bash
dotnet run --project src/UrlShortener.App/UrlShortener.App.csproj
```

Tambem e possivel executar com hot reload:

```bash
dotnet watch --project src/UrlShortener.App/UrlShortener.App.csproj run
```

Pelas configuracoes de `launchSettings.json`, a aplicacao usa:

- HTTP: `http://localhost:5282`
- HTTPS: `https://localhost:7125`

## Rotas uteis

- `/`: tela inicial ou redirecionamento quando recebe um codigo curto.
- `/UrlShortener`: area de gerenciamento dos links curtos.
- `/{shortCode}`: acessa uma URL curta e redireciona para a URL original quando o link esta ativo e nao expirado.

## Docker

Entre na pasta do projeto web:

```bash
cd src/UrlShortener.App
```

Construa a imagem:

```bash
docker build -t urlshortener .
```

Execute o container:

```bash
docker run -d -p 8080:80 --name urlshortener urlshortener
```

Acesse:

```bash
http://localhost:8080
```

Para parar e remover o container:

```bash
docker stop urlshortener
docker rm urlshortener
```

## Scripts

O diretorio `scripts` contem atalhos para execucao:

- `scripts/start.sh`: para, remove, reconstrui e executa o container Docker.
- `scripts/start-watch-run.sh`: executa a aplicacao com `dotnet watch`.

## Testes

Execute todos os testes a partir da raiz do repositorio:

```bash
dotnet test
```

Os testes cobrem os servicos principais, o controller de redirecionamento e o controller de gerenciamento de URLs.

## Estrutura

```text
.
|-- src/
|   `-- UrlShortener.App/
|       |-- Controllers/
|       |-- Infrastructure/
|       |-- Models/
|       |-- Views/
|       |-- wwwroot/
|       `-- Program.cs
|-- tests/
|   `-- UrlShortener.App.Tests/
|-- scripts/
|-- UrlShortener.sln
`-- README.md
```

## Observacoes

- O codigo curto aceita ate 8 caracteres quando informado manualmente.
- Links expirados exibem a tela `Expired` em vez de redirecionar.
- Links inativos exibem a tela `Inactive` em vez de redirecionar.
- A validacao em background verifica URLs com status `Processing` a cada 5 minutos.
