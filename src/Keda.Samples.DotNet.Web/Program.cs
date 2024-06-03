using Keda.Samples.Dotnet.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder();

builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSwagger();
builder.Services.AddOrderQueueServices();


var app = builder.Build();
if (app.Environment.IsDevelopment())app.UseDeveloperExceptionPage();
else app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(swaggerUiOptions =>
{
    swaggerUiOptions.SwaggerEndpoint("v1/swagger.json", "Keda.Samples.Dotnet.API");
    swaggerUiOptions.DocumentTitle = "KEDA API";
});

await app.RunAsync();
