using Microsoft.EntityFrameworkCore;
using task_crud.Domain.Entities;

namespace task_crud.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public DbSet<Todo> Todos { get; set; }
    }
}
