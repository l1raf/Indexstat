using Indexstat.Controllers;
using Indexstat.SerpApi;
using Indexstat.Services;

var builder = WebApplication.CreateBuilder(args);

var allowSpecificOrigins = "allowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowSpecificOrigins,
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:3000");
        });
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<SerpApiSearch>();
builder.Services.AddHttpClient<IndexingController>();

builder.Services.AddScoped<IIndexingService, IndexingService>();
builder.Services.AddScoped<ISerpApiSearch, SerpApiSearch>();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(allowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();