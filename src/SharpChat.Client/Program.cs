using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Messenger.BlazorClient;
using Messenger.BlazorClient.Services;
using Messenger.BlazorClient.Storage;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Регистрация IndexedDB Service
builder.Services.AddIndexedDbService();

// Регистрация экземпляра IndexedDB с нашей конфигурацией
builder.Services.AddIndexedDb(
    databaseName: IndexedDbConfig.DatabaseName,
    objectStores: IndexedDbConfig.ObjectStores,
    version: IndexedDbConfig.DatabaseVersion,
    jsonSerializerOptions: new JsonSerializerOptions(JsonSerializerDefaults.Web),
    key: "id"); // Используем "id" как ключ для всех объектов

// Регистрация сервиса хранилища
builder.Services.AddScoped<IStorageService, StorageService>();

await builder.Build().RunAsync();