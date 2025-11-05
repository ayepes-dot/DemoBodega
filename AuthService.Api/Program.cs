using AuthDatabase.Context;
using AuthDatabase.Repository;
using AuthService.Api.AuthCustoms;
using AuthService.Application.Clients;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthDBSettings>(
    builder.Configuration.GetSection("AuthDBSettings"));

builder.Services.AddSingleton<AuthServiceDbContext>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<AuthDBSettings>>();
    return new AuthServiceDbContext(settings);
});

builder.Services.AddScoped<RevokedTokenRepository>();

builder.Services.AddHttpClient("UserServiceClients", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:UserServiceUrl"]);
});

builder.Services.AddScoped<UserServiceClients>();
builder.Services.AddScoped<AuthUtilities>();

builder.Services.AddScoped<RevokedTokenRepository>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
