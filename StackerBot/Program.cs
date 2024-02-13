using Coravel;
using Serilog;
using Serilog.Events;
using StackerBot.Services;
using StackerBot.Tasks;

Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Debug()
  .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
  .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
  .MinimumLevel.Override("DSharpPlus.BaseDiscordClient", LogEventLevel.Information)
  .WriteTo.Console(
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3} {SourceContext}::{Message}{NewLine}{Exception}"
  )
  .CreateLogger();

var logger = Log.Logger.ForContext<Program>();
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.Services.AddScheduler();

builder.WebHost.UseUrls(Parameters.LISTEN_URL);
builder.WebHost.UseKestrel();

builder.Services.AddDbContextFactory<DatabaseContext>(options => {
  options.UseNpgsql(
    Tools.DatabaseConnectString(),
    settings => settings.EnableRetryOnFailure(5, TimeSpan.FromSeconds(1), new List<string>())
  );
  options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddTransient<IExternals, Externals>();

builder.Services.AddSingleton<EventBus>();
builder.Services.AddSingleton<IRepository, Repository>();
builder.Services.AddHostedService<DiscordBot>();

builder.Services.AddTransient<DailyYouTubeReSubscribe>();

builder.Services.AddControllers().AddXmlSerializerFormatters();

var application = builder.Build();

application.Services.UseScheduler(scheduler => {
  var gmt = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
  var gmtMidnight = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.Date, gmt).AddDays(1);
  var midnightHour = gmtMidnight.Hour;

  scheduler.Schedule<DailyYouTubeReSubscribe>().DailyAtHour(midnightHour);
});

application.UseRouting();
application.MapControllers();
application.MapGet("/", () => "ALIVE");

try {
  logger.Information("StackerBot is applying database migrations");
  await using var scope = application.Services.CreateAsyncScope();
  var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
  var db = await factory.CreateDbContextAsync();
  await db.Database.MigrateAsync();

  logger.Information("StackerBot is starting");
  await application.RunAsync();
} catch (Exception error) {
  logger.Error(error, "StackerBot has encountered an unhandled exception");
  Environment.ExitCode = 1;
}

Environment.Exit(Environment.ExitCode);
