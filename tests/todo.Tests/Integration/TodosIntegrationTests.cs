using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using task_crud.Application.DTOs;
using task_crud.Infrastructure.Data;
using Testcontainers.MsSql;
using Xunit;

namespace todo.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options => { options.UseSqlServer(_dbContainer.GetConnectionString()); });
            });
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
        }
    }

    public class TodosIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        public TodosIntegrationTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 20)]
        public async Task GetAll_ShouldReturnPagedResults(int page, int pageSize)
        {
            //Arrange
            var response = await _client.GetAsync($"/api/todos?page={page}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();

            //Act
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoDTO>>();

            //Assert
            Assert.NotNull(result);
            Assert.True(result.PageSize == pageSize);
        }

        [Theory]
        [InlineData("expedita")]
        [InlineData("fugiat")]
        [InlineData("laboriosam")]
        public async Task GetAll_ShouldFilterByTitle(string filter)
        {
            //Arrange
            var response = await _client.GetAsync($"/api/todos?title={filter}");
            response.EnsureSuccessStatusCode();

            //Act
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoDTO>>();

            //Assert
            Assert.NotNull(result);
            Assert.All(result.Items, t => Assert.Contains(filter, t.Title));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetAll_ShouldFilterById(int filter)
        {
            //Arrange
            var response = await _client.GetAsync($"/api/todos?id={filter}");
            response.EnsureSuccessStatusCode();

            //Act
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoDTO>>();

            //Assert
            Assert.NotNull(result);
            Assert.All(result.Items, t => Assert.Equal(filter, t.Id));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetAll_ShouldFilterByUserId(int filter)
        {
            //Arrange
            var response = await _client.GetAsync($"/api/todos?userid={filter}");
            response.EnsureSuccessStatusCode();

            //Act
            var result = await response.Content.ReadFromJsonAsync<PagedResult<TodoDTO>>();

            //Assert
            Assert.NotNull(result);
            Assert.All(result.Items, t => Assert.Equal(filter, t.UserId));
        }

        [Fact]
        public async Task Put_ShouldRespectIncompleteLimit()
        {
            //Arrange
            int userId = 1;
            await _client.PostAsync("/api/todos/sync", null);
            
            //Act
            var getResponse = await _client.GetAsync($"/api/todos?page=1&pageSize=100");
            var result = await getResponse.Content.ReadFromJsonAsync<PagedResult<TodoDTO>>();
            var todos = result.Items.Where(t => t.UserId == userId && t.Completed == false).Take(6).ToList();
            foreach (var todo in todos.Take(5))
                await _client.PutAsJsonAsync($"/api/todos/{todo.Id}", new UpdateTodoDTO(userId, todo.Title, false));
            var response = await _client.PutAsJsonAsync($"/api/todos/{todos[5].Id}", new UpdateTodoDTO(userId, todos[5].Title, false));

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_ShouldUpdateCompletedStatus()
        {
            //Arrange
            int userId = 1;
            await _client.PostAsync("/api/todos/sync", null);

            //Act
            var getResponse = await _client.GetAsync($"/api/todos?page=1&pageSize=100");
            var result = await getResponse.Content.ReadFromJsonAsync<PagedResult<TodoDTO>>();
            var todo = result.Items.First(t => t.UserId == userId);
            await _client.PutAsJsonAsync($"/api/todos/{todo.Id}", new UpdateTodoDTO(userId, todo.Title, true));
            var updated = await (await _client.GetAsync($"/api/todos/{todo.Id}")).Content.ReadFromJsonAsync<TodoDTO>();
            
            //Assert
            Assert.True(updated.Completed);
        }
    }
}
