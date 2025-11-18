using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatApp.Api.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        // Use env var DefaultConnection if present, otherwise fallback to localdb
        var conn = Environment.GetEnvironmentVariable("DefaultConnection")
                   ?? "Server=(localdb)\\mssqllocaldb;Database=ChatAppDb;Trusted_Connection=True;";

        builder.UseSqlServer(conn);
        return new AppDbContext(builder.Options);
    }
}