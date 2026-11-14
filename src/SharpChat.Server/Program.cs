using MessengerServer;
using MessengerServer.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllMessenger", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Разрешает любой IP (и localhost, и 192.168.x.x)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("BlazorPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAllMessenger");

// Маппим наш хаб на определенный URL-адрес
app.MapHub<ChatHub>("/chathub");

app.MapHub<SignalingHub>("/signalinghub");

app.Run();