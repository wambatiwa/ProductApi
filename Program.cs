using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/products", async(AppDbContext db) => {
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
});

app.MapGet("/products{id}",async(int id, AppDbContext db) => 
    await db.Products.FindAsync(id) is Product product? Results.Ok(product):Results.NotFound() 
);

app.MapPost("/products", async(Product product, AppDbContext db) => {
    var existingProduct = await db.Products.FindAsync(product.Id);
    if(existingProduct != null) {
        return Results.BadRequest("A Product with this Id already Exist");
    }
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/products/product.Id",product);
});

app.MapDelete("/products/{id}", async (int id, AppDbContext db) => 
{
    var product = await db.Products.FindAsync(id);
    if(product is null) return Results.NotFound();
    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.Ok(product);
});

app.MapPut("/products/{id}", async(int id, Product product, AppDbContext db) => {
    var produce = await db.Products.FindAsync(id);
    if(produce is null) return Results.NotFound();
    
    produce.Name = product.Name;
    produce.Description = product.Description;
    produce.Price = product.Price;
    produce.Size = product.Size;

    await db.SaveChangesAsync();
    return Results.NoContent();
});


app.Run();

