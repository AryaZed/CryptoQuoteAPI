using CryptoQuoteAPI.Configurations;
using CryptoQuoteAPI.Endpoints;
using CryptoQuoteAPI.Hubs;
using CryptoQuoteAPI.Middlewares;
using CryptoQuoteAPI.Services.BackgroundServices;
using CryptoQuoteAPI.Services.CoinMarketCap;
using CryptoQuoteAPI.Services.ExchangeRate;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.Configure<CoinMarketCapConfig>(builder.Configuration.GetSection("CoinMarketCap"));
builder.Services.Configure<ExchangeRatesConfig>(builder.Configuration.GetSection("ExchangeRates"));
builder.Services.Configure<CryptoUpdateSettings>(builder.Configuration.GetSection("CryptoUpdateSettings"));

builder.Services.AddSingleton<ICryptoSettingsService, CryptoSettingsService>();

// Add endpoints API explorer and Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    else
    {
        Log.Warning("XML documentation file '{XmlPath}' not found. Swagger documentation will not include XML comments.", xmlPath);
    }
});

IAsyncPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            Log.Warning("Delaying for {Delay} seconds, then making retry {RetryAttempt}.", timespan.TotalSeconds, retryAttempt);
        });

// Add HttpClient services
builder.Services.AddHttpClient<ICryptoService, CryptoService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<CoinMarketCapConfig>>().Value;
    client.BaseAddress = new Uri(config.BaseUrl);
})
.AddPolicyHandler(retryPolicy);

builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<ExchangeRatesConfig>>().Value;
    client.BaseAddress = new Uri(config.BaseUrl);
})
.AddPolicyHandler(retryPolicy);

builder.Services.AddMemoryCache();

builder.Services.AddTransient<CryptoEndpoint>();

builder.Services.AddSignalR();

builder.Services.AddHostedService<BackgroundCryptoUpdateService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<CryptoHub>("/cryptohub");

var cryptoEndpoint = app.Services.GetRequiredService<CryptoEndpoint>();
cryptoEndpoint.MapEndpoint(app);

app.Run();
