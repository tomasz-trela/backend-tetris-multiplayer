using backend_tetris.entities;
using Microsoft.EntityFrameworkCore;

namespace backend_tetris.database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MyUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.Entity<MyUser>().HasData([new MyUser {Id = 1,Username = "admin", Email = "admin@admin.com", PasswordHash = "pasymnam"}]);
    }
}