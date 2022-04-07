using System.Text;
using Indexstat.RobotsParser;
using Indexstat.SerpApi;
using Indexstat.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

const string allowSpecificOrigins = "allowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowSpecificOrigins,
        policyBuilder =>
        {
            policyBuilder.WithOrigins("https://agreeable-coast-043b90910.1.azurestaticapps.net");
        });
});

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<SerpApiSearch>();
builder.Services.AddHttpClient<RobotsService>();
builder.Services.AddHttpClient<RobotsParser>();
builder.Services.AddHttpClient<IndexingService>();

builder.Services.AddScoped<IIndexingService, IndexingService>();
builder.Services.AddScoped<IRobotsService, RobotsService>();
builder.Services.AddScoped<ISerpApiSearch, SerpApiSearch>();
builder.Services.AddScoped<IRobotsParser, RobotsParser>();

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