using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TwitchIrcHub.Authentication;
using TwitchIrcHub.Authentication.Policies;
using TwitchIrcHub.Authentication.Policies.Handler;
using TwitchIrcHub.Authentication.Policies.Requirements;
using TwitchIrcHub.BackgroundServices;
using TwitchIrcHub.Controllers.TwitchAppController.TwitchAppData;
using TwitchIrcHub.ExternalApis.Discord;
using TwitchIrcHub.Hubs.IrcHub;
using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.IrcClient;
using TwitchIrcHub.IrcBot.Irc.IrcPoolManager;
using TwitchIrcHub.Model;

namespace TwitchIrcHub;

public class Startup
{
    /// <summary>
    /// <c>TreatTinyAsBoolean=false</c> results in using bit(1) instead of tinyint(1) for <see cref="bool"/>.<br/>
    /// <br/>
    /// In 5.0.5 SSL was enabled by default. It isn't necessary for our usage.
    /// (We don't expose the DB to the internet.)
    /// https://stackoverflow.com/a/45108611
    /// </summary>
    private const string AdditionalMySqlConfigurationParameters = ";TreatTinyAsBoolean=false;SslMode=none";

    private IConfiguration Configuration { get; }
    private IHostEnvironment HostingEnvironment { get; }

    public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
    {
        Configuration = configuration;
        HostingEnvironment = hostingEnvironment;
        DiscordWebhook.SetWebhooks(Configuration.GetSection("DiscordWebhooks"));
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "TwitchIrcHub", Version = "v1" });

            // This is more or less a hotfix due to the way I currently run docker. 
            // Once I cleanup the docker file this won't be needed anymore.
            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
                c.IncludeXmlComments("TwitchIrcHub.xml");

            c.AddSecurityDefinition("OAuth", new OpenApiSecurityScheme
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
                            Id = "OAuth"
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

        services.AddDbContext<IrcHubDbContext>(opt =>
        {
            //Try env var first else use appsettings.json
            string? dbConString = Environment.GetEnvironmentVariable(@"IRCHUBDB_CONNECTIONSTRINGS_DB");
            if (string.IsNullOrEmpty(dbConString))
                dbConString = Configuration.GetConnectionString("IrcHubDb");
            opt.UseMySQL(dbConString + AdditionalMySqlConfigurationParameters);
        });

        //https://josef.codes/asp-net-core-protect-your-api-with-api-keys/
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            })
            .AddApiKeySupport(_ => { });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.IsRegisteredApp,
                policy => policy.Requirements.Add(new IsRegisteredAppRequirements()));
        });

        services.AddTransient<IAuthorizationHandler, IsRegisterdAppHandler>();

        //services.AddCors(options =>
        //    {
        //        options.AddDefaultPolicy(builder =>
        //        {
        //            if (HostingEnvironment.IsDevelopment())
        //                builder.SetIsOriginAllowed(_ => true)
        //                    .AllowAnyHeader()
        //                    .AllowAnyMethod()
        //                    .AllowCredentials();
        //            else
        //                builder.WithOrigins("https://*.icdb.dev")
        //                    .AllowAnyHeader()
        //                    .AllowAnyMethod()
        //                    .AllowCredentials()
        //                    .SetIsOriginAllowedToAllowWildcardSubdomains();
        //        });
        //    }
        //);
        services.AddSignalR();

        services.AddHostedService<PrepareBotAndPrefetchData>();
        services.AddHostedService<BotManager>();

        services.AddFactory<IBotInstance, BotInstance>();
        services.AddFactory<IIrcPoolManager, IrcPoolManager>();
        services.AddFactory<IIrcClient, IrcClient>();
        services.AddFactory<IBotInstanceData, BotInstanceData>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "TwitchIrcHub v1"); });
            TwitchAppData.RedirectUrl = TwitchAppData.RedirectUrlDevelopment;
        }
        else
        {
            TwitchAppData.RedirectUrl = TwitchAppData.RedirectUrlProduction;
        }

        app.UseExceptionHandler(appErr =>
            appErr.Run(context =>
                {
                    context.Response.StatusCode = 500;
                    IExceptionHandlerPathFeature? exception = context.Features.Get<IExceptionHandlerPathFeature>();
                    //if (exception != null)
                    //    DiscordLogger.LogException(exception.Error);
                    return Task.CompletedTask;
                }
            )
        );

        // Inject ServiceProvider into static BotDataAccess class
        // This is a horrible anti-pattern
        // https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/
        // https://blog.ploeh.dk/2014/05/15/service-locator-violates-solid/
        // But because we need access to BotDataAccess from a static context I'm calling this a necessary evil.
        BotDataAccess.ServiceProvider = app.ApplicationServices;

        //app.UseHttpsRedirection(); //This breaks UseCors

        app.UseWebSockets();
        app.UseCors();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<IrcHub>("/IrcHub", options =>
            {
                // Not sure if we even need this
                // options.ApplicationMaxBufferSize = 30 * 1024; // * 1000;
            });
        });
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
