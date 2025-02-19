using Microsoft.EntityFrameworkCore;

namespace WebApi;

public class TodoDb(DbContextOptions<TodoDb> options) : DbContext(options)
{
    public DbSet<Todo> Todos => Set<Todo>();
}
