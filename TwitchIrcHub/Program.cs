using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TwitchIrcHub.Authentication;
using TwitchIrcHub.Authentication.Policies;
using TwitchIrcHub.Authentication.Policies.Handler;
using TwitchIrcHub.Authentication.Policies.Requirements;
using TwitchIrcHub.BackgroundServices;
using TwitchIrcHub.Controllers.AuthController.AuthData;
using TwitchIrcHub.Hubs.IrcHub;
using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.IrcClient;
using TwitchIrcHub.IrcBot.Irc.IrcPoolManager;
using TwitchIrcHub.Model;

// <summary>
// <c>TreatTinyAsBoolean=false</c> results in using bit(1) instead of tinyint(1) for <see cref="bool"/>.<br/>
// <br/>
// In 5.0.5 SSL was enabled by default. It isn't necessary for our usage.
// (We don't expose the DB to the internet.)
// https://stackoverflow.com/a/45108611
// </summary>
const string additionalMySqlConfigurationParameters = ";TreatTinyAsBoolean=false;SslMode=none";


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TwitchIrcHub", Version = "v1" });

    // This is more or less a hotfix due to the way I currently run docker. 
    // Once I cleanup the docker file this won't be needed anymore.
    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        c.IncludeXmlComments("TtsApi.xml");

    c.AddSecurityDefinition("API Key", new OpenApiSecurityScheme
    {
        Description = "32 char API Key",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "API Key"
                }
            },
            Array.Empty<string>()
        }
    });

    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    string commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
    string commentsFile = Path.Combine(baseDirectory, commentsFileName);
    if (File.Exists(commentsFile))
        c.IncludeXmlComments(commentsFile);
});
builder.Services.AddDbContext<IrcHubDbContext>(opt =>
{
    //Try env var first else use appsettings.json
    string? dbConString = Environment.GetEnvironmentVariable(@"IRCHUBDB_CONNECTIONSTRINGS_DB");
    if (string.IsNullOrEmpty(dbConString))
        dbConString = builder.Configuration.GetConnectionString("IrcHubDb");
    opt.UseMySQL(dbConString + additionalMySqlConfigurationParameters);
});

//https://josef.codes/asp-net-core-protect-your-api-with-api-keys/
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
        options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
    })
    .AddApiKeySupport(_ => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.IsRegisteredApp,
        policy => policy.Requirements.Add(new IsRegisteredAppRequirements()));
});

builder.Services.AddTransient<IAuthorizationHandler, IsRegisterdAppHandler>();


builder.Services.AddSignalR();

builder.Services.AddHostedService<PrepareBotAndPrefetchData>();
builder.Services.AddHostedService<BotManager>();

builder.Services.AddFactory<IBotInstance, BotInstance>();
builder.Services.AddFactory<IIrcPoolManager, IrcPoolManager>();
builder.Services.AddFactory<IIrcClient, IrcClient>();
builder.Services.AddFactory<IBotInstanceData, BotInstanceData>();

// Inject ServiceProvider into static BotDataAccess class
BotDataAccess.ServiceProvider = builder.Services.BuildServiceProvider();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TwitchIrcHub v1"));
    app.UseDeveloperExceptionPage();
    AuthData.RedirectUrl = AuthData.RedirectUrlDevelopment;
}
else
{
    AuthData.RedirectUrl = AuthData.RedirectUrlProduction;
}

//app.UseHttpsRedirection();
app.UseWebSockets();
app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<IrcHub>("/IrcHub", options =>
    {
        // Not sure if we even need this
        // options.ApplicationMaxBufferSize = 30 * 1024; // * 1000;
    });
});
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();
