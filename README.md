# PokerEngine

PokerEngine é uma biblioteca de poker em C# e um console para testar as regras e comparações de mãos.

## Requisitos

- .NET SDK 10.0.111
- VS Code (opcional, mas recomendado)

O projeto já inclui um `global.json` para fixar a versão do SDK e garantir compatibilidade com o ambiente do repositório.

## Estrutura do projeto

- `src/PokerEngine.Domain` — lógica do domínio, cartas, baralho e avaliação de mãos.
- `src/PokerEngine.Console` — aplicação de console para testar a engine.
- `src/PokerEngine.Web` — projeto web minimalista para consumir o domínio com inputs simples.
- `test/PokerEngine.XunitTest` — suíte de testes unitários com xUnit.

## Getting Started

### 1. Verifique a instalação do SDK

```bash
dotnet --version
```

Deve retornar a versão `10.0.111` configurada pelo arquivo `global.json`.

### 2. Restaurar dependências

```bash
dotnet restore
```

### 3. Compilar a solução

```bash
dotnet build PokerEngine.slnx --nologo
```

### 4. Executar os testes

```bash
dotnet test PokerEngine.slnx --nologo
```

### 5. Executar a aplicação de console

```bash
dotnet run --project src/PokerEngine.Console/PokerEngine.Console.csproj
```

### 6. Executar a aplicação web

```bash
dotnet run --project src/PokerEngine.Web/PokerEngine.Web.csproj
```

A interface web expõe uma página simples com dois campos: uma mão de 5 cartas e uma simulação de Texas Hold'em.

## Executar localmente

Os passos acima já cobrem a execução local do projeto. Em caso de uso direto apenas em uma parte específica, você também pode rodar:

```bash
dotnet test test/PokerEngine.XunitTest/PokerEngine.XunitTest.csproj --nologo
```

## VS Code

Os arquivos em `.vscode/` já configuram:

- build da solução
- execução da aplicação
- execução dos testes
- seleção do SDK pelo `global.json`

Você pode abrir a pasta no VS Code e usar o painel de "Run and Debug".

### Tasks disponíveis

- `build solution`
- `build web`
- `build console`
- `build tests`
- `run console app`
- `run tests`
- `build`
- `publish`
- `watch`

As configurações de depuração `.NET Core Launch (web)`, `.NET Core Launch (console)` e `.NET Core Launch (xUnit tests)` compilam o projeto correspondente antes de iniciar. A aplicação web usa `http://localhost:5044`.

O projeto `PokerEngine.XunitTest` está incluído em `PokerEngine.slnx` e é descoberto automaticamente pelo Test Explorer do VS Code quando a pasta do repositório é aberta.

## Exemplos de uso

A aplicação de console aceita entradas como:

```text
AC, KC, QH, JD, 10S
```

Ou comandos de teste:

```text
TEXAS 2
RANDOM 3
```

## Observação

A solução usa o formato `.slnx` do .NET 10, os projetos compilam para `net10.0` e os pacotes do xUnit são compatíveis com o SDK atual.
