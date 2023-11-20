using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;
using SpaceWars.Logic;
using SpaceWars.Web;
using SpaceWars.Web.Components;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddOptions<GameConfig>()
    .BindConfiguration(nameof(GameConfig))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<SpaceWars.Logic.ITimer, CompetitionTimer>();
builder.Services.AddSingleton<Game>();
builder.Services.AddSingleton<IInitialLocationProvider, DefaultInitialLocationProvider>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.WriteTo.Console()
    .Enrich.WithExceptionDetails()
    ;
    //.WriteTo.Seq(builder.Configuration["SeqServer"] ?? throw new ApplicationException("Unable to locate key SeqServer in configuration"))
    //.WriteTo.LokiHttp(() => new LokiSinkConfiguration
    //{
    //    LokiUrl = builder.Configuration["LokiServer"],
    //    LogLabelProvider = new LogLabelProvider(),
    //});
});

builder.Services.AddProblemDetails(opts =>
{
    opts.IncludeExceptionDetails = (ctx, ex) => false;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SpaceWars", Version = "v1" });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var token = httpContext.Request.Query["token"].FirstOrDefault();
        var ipAddress = httpContext.Request.Headers.Host.ToString();
        var apiLimit = httpContext.RequestServices.GetRequiredService<IOptions<GameConfig>>().Value.ApiLimitPerSecond;

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: token ?? ipAddress,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = apiLimit,
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(1)
            });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "SpaceWars (2024 Coding Competition)");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .DisableRateLimiting();

app.UseRateLimiter();
app.MapControllers();

app.Run();

public partial class Program { }

public class GameConfig
{
    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = "password";
    [Range(10, 100_000)]
    public int ApiLimitPerSecond { get; set; } = 50;
    public int TickFrequencyMilliseconds = 333;
}