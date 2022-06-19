using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using SubMerge.API;
using SubMerge.API.Dtos;
using SubMerge.API.Repositories;
using SubMerge.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<CosmosClient>((s) =>
{
    // TODO : Implement Lazy load CosmosClient
    var client = new CosmosClient(builder.Configuration["CosmosDb:Endpoint"], builder.Configuration["CosmosDb:AuthorizationKey"]);
    var db = client.CreateDatabaseIfNotExistsAsync(builder.Configuration["CosmosDb:Database"]).Result;
    db.Database.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosContainers.Document, "/id")).Wait();
    db.Database.CreateContainerIfNotExistsAsync(new ContainerProperties(Constants.CosmosContainers.Record, "/documentId")).Wait();
    return client;
});

builder.Services.AddTransient<SubMerge.Engine.IProcessService, SubMerge.Engine.ProcessService>();
builder.Services.AddScoped<IBlobStorageRepository, BlobStorageRepository>();
builder.Services.AddScoped<IProcessService, ProcessService>();
builder.Services.AddSingleton<ICosmosRepository, CosmosCommonRepository>();

Seeder.Seed(builder.Services, builder.Configuration).GetAwaiter().GetResult();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/process", ([FromServices] ProcessService service, [FromBody] ProcessRequestDto model, CancellationToken cancellationToken) =>
{
    service.ProcessFiles(model, cancellationToken);
})
.WithName("Process");

app.Run();