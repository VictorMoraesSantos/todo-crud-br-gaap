# todo-crud

Aplicação para gerenciamento de tarefas (ToDo) com backend em .NET 9 e frontend SAPUI5.

## Endpoints principais (API)

- `GET /api/todos` — Lista todas as tarefas
- `GET /api/todos/{id}` — Busca uma tarefa pelo ID
- `POST /api/todos/sync` — Carrega tarefas de uma fonte externa
- `PUT /api/todos/{id}` — Atualiza uma tarefa existente

## Paginação e Filtros de Busca

A API suporta paginação e filtros nos endpoints de listagem:

- Parâmetros de paginação:
  - `page`: número da página (padrão: 1)
  - `pageSize`: quantidade de itens por página (padrão: 10)
- Filtros disponíveis:
  - `title`: busca por título da tarefa
  - `id`: busca por ID da tarefa
  - `userId`: busca por ID do usuário
  - `sort`: campo para ordenação (`title`, `id`, `userId`)
  - `order`: direção da ordenação (`asc` ou `desc`)

**Exemplo de requisição com paginação e filtro:**

```
GET /api/todos?page=2&pageSize=5&title=importante&sort=title&order=desc
```

Esse exemplo retorna a segunda página, com 5 tarefas por página, filtrando pelo título "importante" e ordenando pelo título de forma decrescente.

## Casos de uso

- Listar tarefas
- Criar nova tarefa
- Editar tarefa
- Excluir tarefa

## Como executar o backend (API) com Docker Compose

1. Certifique-se de ter o Docker e Docker Compose instalados.
2. Execute o comando abaixo na raiz do projeto:

    ```sh
    docker compose up --build
    ```

A API estará disponível em `https://localhost:6001`.

## Como executar o frontend

1. Acesse a pasta do frontend:

    ```sh
    cd src/FrontEnd/webapp
    ```

2. Instale as dependências:

    ```sh
    npm install
    ```

3. Inicie o frontend:

    ```sh
    npm start
    ```

O frontend estará disponível em `http://localhost:8080/index.html`.

## Informações adicionais

- O backend utiliza Entity Framework Core e SQL Server (configuração padrão).
- O frontend consome a API REST para exibir e manipular tarefas.
- Para desenvolvimento, utilize o Swagger disponível em `/swagger` para testar os endpoints.
