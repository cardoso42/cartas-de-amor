# Cartas de Amor

Implementação do jogo *Love Letter* em Sistema Distribuído com uma interface Web.

## Sobre o Projeto

Este projeto é uma implementação digital do jogo de cartas "Love Letter" (Cartas de Amor), desenvolvido como um sistema distribuído com uma interface web interativa. O jogo permite que múltiplos jogadores participem remotamente através de uma aplicação web, utilizando uma arquitetura cliente-servidor.

### O que é Love Letter?

Love Letter é um jogo de dedução, risco e eliminação onde os jogadores competem para entregar uma carta de amor à princesa. Cada carta tem um valor diferente e habilidades especiais que podem ajudar ou atrapalhar os jogadores.

## Características

- **Sistema Distribuído**: Arquitetura baseada em microserviços utilizando .NET Core
- **Comunicação em Tempo Real**: Implementada via SignalR para atualizações instantâneas do estado do jogo
- **Interface Web Responsiva**: Frontend desenvolvido com HTML/CSS/JavaScript
- **Autenticação de Usuários**: Sistema completo de cadastro e login
- **Persistência de Dados**: Armazenamento das informações do usuário e histórico de jogos

## Arquitetura

O projeto segue uma arquitetura em camadas:

- **Apresentação**: API REST e hub SignalR para comunicação com clientes
- **Aplicação**: Lógica de negócio e coordenação de serviços
- **Domínio**: Entidades, lógica de jogo e regras de negócio
- **Infraestrutura**: Persistência de dados, serviços externos e configurações

## Tecnologias Utilizadas

### Backend
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- SignalR
- JWT Authentication
- Serilog para logging

### Frontend
- HTML5/CSS3
- JavaScript (ES6+)
- Comunicação assíncrona com a API

## Como Executar

### Pré-requisitos
- [.NET 6 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/)
- Um navegador web moderno
- Make (para usar os comandos do makefile)

### Configuração do Banco de Dados
1. Crie um banco de dados PostgreSQL nomeado `love_letter`
2. Atualize a string de conexão no arquivo `appsettings.json`
3. Execute a migração do banco de dados usando o makefile:
   ```bash
   make database-update
   ```

### Usando o Makefile

O projeto inclui um makefile para simplificar as operações comuns:

#### Executar o Backend
```bash
make back
```

#### Executar o Frontend
```bash
make front
```

#### Gerenciar Banco de Dados
```bash
# Conectar ao banco de dados PostgreSQL
make database

# Aplicar migrações existentes
make database-update

# Criar uma nova migração
make migration-add

# Criar uma nova migração e atualizar o banco de dados
make migration

# Desfazer a última migração
make undo-last-migration
```

#### Gerar Documentação
```bash
# Gerar diagramas e PDF do SRS (Software Requirements Specification)
make srs

# Gerar apenas os diagramas PlantUML
make srs-diagrams

# Gerar apenas o PDF do SRS
make srs-pdf
```

### Execução Manual

Se preferir não usar o makefile, você pode executar os comandos diretamente:

#### Backend
```bash
cd src/CartasDeAmorBack/CartasDeAmor.Presentation
dotnet run
```

#### Frontend
```bash
cd src/CartasDeAmorFrontTest
python3 -m http.server 8080
```

## Regras do Jogo

O jogo Love Letter utiliza um baralho de 16 cartas, cada uma com valores e habilidades específicas:

| Carta | Valor | Efeito |
|-------|-------|--------|
| Spy | 0 | Se você for o único jogador a jogar esta carta, ganhe +1 ponto |
| Guard | 1 | Escolha um jogador e adivinhe sua carta. Se acertar, ele é eliminado |
| Priest | 2 | Veja a mão de outro jogador |
| Baron | 3 | Compare sua mão com a de outro jogador. O jogador com o valor mais baixo é eliminado |
| Servant | 4 | Proteção contra efeitos até seu próximo turno |
| Prince | 5 | Force um jogador (pode ser você mesmo) a descartar sua mão e comprar uma nova carta |
| Chancellor | 6 | Compre 2 cartas adicionais e coloque 2 cartas de volta no fundo do baralho |
| King | 7 | Troque sua mão com a de outro jogador |
| Countess | 8 | Deve ser descartada se estiver com o Rei ou o Príncipe na mão |
| Princess | 9 | Se descartada, você é eliminado |

Para regras completas, consulte a [documentação](/docs/cards.md).

## Documentação

- [Documentação da API](/docs/api-documentation.md)
- [Diagramas de Sequência](/docs/sequence_diagrams)
- [Relatórios de Bugs](/docs/bug-report)

## Estado Atual

O projeto encontra-se em desenvolvimento ativo. Novas funcionalidades e melhorias são bem-vindas através de pull requests.

## Licença

Este projeto é distribuído sob a licença MIT.

**Autores**:
- [Lucas Cardoso](https://cardoso42.github.io/)
- Maya Monteiro
- Miguel de Carvalho
