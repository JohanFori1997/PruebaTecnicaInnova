using PruebaTecnicaInnova.Repositories;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaInnova.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyeccion de dependencias para el repositorio en memoria
builder.Services.AddScoped<IProductRepository, EfProductRepository>();



builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ProductsDb")
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }