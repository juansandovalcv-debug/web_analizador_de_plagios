using Microsoft.EntityFrameworkCore;
using web_analizador_de_plagios.Components;
using web_analizador_de_plagios.Data.Models;
using web_analizador_de_plagios.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=sistema_plagio.db";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped(sp => 
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// Registro de Servicios Lógicos de la Capa de Negocio
builder.Services.AddSingleton<NormalizadorCodigo>();
builder.Services.AddSingleton<DetectorIAPatrones>();
builder.Services.AddSingleton<InspectorMetadatos>();
builder.Services.AddSingleton<GeneradorQuizDefensa>();
builder.Services.AddScoped<GestorRespaldos>();
builder.Services.AddSingleton<LectorOCR>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
