# Gestão de Ordens de Serviço

Sistema desktop corporativo para Gestão de Ordens de Serviço, desenvolvido em C# com Windows Forms, .NET Framework 4.6, PostgreSQL, Npgsql e ReportViewer.

O projeto foi estruturado como uma base evolutiva para produção, com separação de responsabilidades, regras de negócio centralizadas, controle transacional, concorrência otimista, auditoria e relatórios gerenciais.

## Sumário

- [Stack](#stack)
- [Arquitetura](#arquitetura)
- [Funcionalidades](#funcionalidades)
- [Banco de dados](#banco-de-dados)
- [Transações](#transações)
- [Concorrência otimista](#concorrência-otimista)
- [Auditoria](#auditoria)
- [Relatórios](#relatórios)
- [Tratamento de erros e logs](#tratamento-de-erros-e-logs)
- [Como executar](#como-executar)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Decisões técnicas](#decisões-técnicas)

## Stack

- Windows Forms
- .NET Framework 4.6
- PostgreSQL
- Npgsql 4.0.14
- Microsoft ReportViewer WinForms
- ADO.NET com comandos parametrizados
- Sem ORM

## Arquitetura

A solução segue uma organização inspirada em Clean Architecture, adaptada para uma aplicação desktop WinForms.

```text
GestaoOrdensServico
├── database
│   └── schema.sql
└── src
    ├── GestaoOS.Domain
    ├── GestaoOS.Application
    ├── GestaoOS.Infrastructure
    └── GestaoOS.WinForms
```

### GestaoOS.Domain

Contém o núcleo de domínio:

- entidades;
- enums;
- exceções de domínio;
- comportamentos simples das entidades, como recálculo de totais.

Não depende de banco de dados, WinForms, Npgsql ou ReportViewer.

### GestaoOS.Application

Contém os casos de uso e contratos:

- services;
- validators;
- filtros de pesquisa;
- projeções de relatório;
- interfaces de repositório;
- abstração de Unit of Work;
- abstração de log.

As regras de negócio ficam nesta camada.

### GestaoOS.Infrastructure

Contém implementações técnicas:

- conexão PostgreSQL;
- Unit of Work transacional;
- repositórios com `NpgsqlConnection` e `NpgsqlCommand`;
- queries com parâmetros nomeados;
- paginação com `LIMIT/OFFSET`;
- log técnico em arquivo.

### GestaoOS.WinForms

Contém a interface desktop:

- telas de clientes;
- telas de serviços;
- telas de ordens de serviço;
- relatório gerencial com ReportViewer;
- tratamento amigável de erros;
- composition root para montar as dependências.

## Funcionalidades

### Clientes

- Cadastro de clientes pessoa física ou jurídica.
- Documento único garantido por constraint no banco.
- Pesquisa combinável por nome, documento e ativo.
- Bloqueio de exclusão quando existe ordem de serviço vinculada.

### Serviços

- Cadastro de serviços com valor base e percentual de imposto.
- Validação de valor base maior que zero.
- Validação de percentual de imposto entre 0 e 100.
- Alteração de valor base não afeta ordens já criadas, pois os itens gravam o valor aplicado no momento da criação.

### Ordens de Serviço

- Cadastro de OS com cliente, status, observação e itens.
- Cálculo automático do total do item:

```text
ValorTotalItem = (Quantidade * ValorUnitario) + Imposto
```

- Recálculo automático do total da OS.
- Bloqueio de edição de itens quando a OS está concluída ou cancelada.
- Bloqueio de conclusão de OS com valor total igual a zero.
- Histórico de status.
- Controle de versão para concorrência otimista.

## Banco de dados

O script completo está em:

```text
database/schema.sql
```

O script contempla:

- primary keys;
- foreign keys;
- constraints `CHECK`;
- unique constraint para documento do cliente;
- índices para documento, data de abertura, status e cliente;
- índices parciais para registros ativos;
- tabelas de auditoria e histórico.

Índices principais:

```sql
CREATE INDEX ix_clientes_documento ON clientes(documento);
CREATE INDEX ix_ordens_servico_data_abertura ON ordens_servico(data_abertura);
CREATE INDEX ix_ordens_servico_status ON ordens_servico(status);
CREATE INDEX ix_ordens_servico_cliente_id ON ordens_servico(cliente_id);
```

## Transações

O salvamento de ordem de serviço é executado em uma única transação.

Na mesma unidade transacional são persistidos:

- cabeçalho da OS;
- itens;
- histórico de status;
- auditoria.

Em caso de qualquer falha, o `PostgresUnitOfWork` executa rollback.

## Concorrência otimista

A concorrência é controlada pelo campo `versao` da tabela `ordens_servico`.

Ao atualizar uma OS, o repositório usa a versão original carregada pela tela:

```sql
UPDATE ordens_servico
SET
    cliente_id = @cliente_id,
    data_abertura = @data_abertura,
    data_conclusao = @data_conclusao,
    status = @status,
    observacao = @observacao,
    valor_total = @valor_total,
    versao = versao + 1
WHERE id = @id
  AND versao = @versao;
```

Se nenhum registro for atualizado, o service lança `ConcurrencyException`.

Esse comportamento cobre o cenário em que dois usuários abrem a mesma OS e o segundo tenta salvar uma versão desatualizada.

## Auditoria

A tabela `auditoria` registra:

- entidade;
- id do registro;
- operação;
- data e hora;
- usuário;
- snapshot JSON do estado.

Eventos auditados:

- inclusão de OS;
- alteração de status;
- alteração de itens;
- alteração de valor total.

## Relatórios

O relatório gerencial usa ReportViewer com projeção em objeto (`OrdemServicoReportRow`).

Filtros:

- período;
- cliente, suportado no filtro e no repositório;
- status.

Exibição:

- agrupamento lógico por cliente na consulta;
- total por cliente;
- total geral;
- total de impostos;
- quantidade total de OS no período;
- exportação para PDF pela tela.

## Tratamento de erros e logs

A UI exibe mensagens amigáveis para:

- validações de negócio;
- erro de concorrência;
- violação de unique constraint;
- violação de foreign key;
- violação de check constraint.

O detalhe técnico é gravado em arquivo:

```text
bin/<config>/logs/gestao-os.log
```

## Como executar

### 1. Criar o banco

Execute o script:

```text
database/schema.sql
```

### 2. Ajustar a conexão

Edite o arquivo:

```text
src/GestaoOS.WinForms/App.config
```

Exemplo:

```xml
<add
  name="GestaoOsDb"
  connectionString="Host=localhost;Port=5432;Database=gestao_os;Username=postgres;Password=postgres"
  providerName="Npgsql" />
```

### 3. Restaurar e compilar

No diretório raiz da solução:

```powershell
MSBuild GestaoOrdensServico.sln /t:Restore,Build /p:Configuration=Debug /m
```

### 4. Executar

Defina `GestaoOS.WinForms` como projeto de inicialização no Visual Studio e execute.

Também é possível iniciar o executável gerado em:

```text
src/GestaoOS.WinForms/bin/Debug/net46/GestaoOS.WinForms.exe
```

## Estrutura do projeto

```text
src/GestaoOS.Domain
├── Entities
├── Enums
└── Exceptions

src/GestaoOS.Application
├── Abstractions
├── Common
├── Filters
├── Reports
├── Repositories
├── Services
└── Validation

src/GestaoOS.Infrastructure
├── Data
├── Logging
└── Repositories

src/GestaoOS.WinForms
├── Forms
├── Infrastructure
├── Properties
└── Reports
```

## Decisões técnicas

- Repositórios não possuem regra de negócio.
- Services coordenam regras, transações e auditoria.
- Validators centralizam validações de entrada.
- `UnitOfWork` encapsula conexão e transação.
- Listagens não carregam itens automaticamente.
- Itens são carregados apenas ao abrir a OS.
- Valores aplicados em itens são persistidos para preservar histórico financeiro.
- O projeto não usa ORM para atender ao requisito de acesso direto com Npgsql.
- As telas foram escritas sem designer para facilitar revisão do código no teste.

## Build validado

Ambiente usado na validação:

- Windows
- Visual Studio 2022 Community
- MSBuild 17
- .NET Framework 4.6 targeting pack

Comando executado:

```powershell
MSBuild GestaoOrdensServico.sln /t:Restore,Build /p:Configuration=Debug /m
```

Resultado:

```text
Compilação com êxito.
0 Aviso(s)
0 Erro(s)
```
