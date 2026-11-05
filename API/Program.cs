using API.AuthFilters;
using API.Middlewares.Extensions;
using Domain.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Persistence;
using Serilog;
using Serilog.Ui.Core.Extensions;
using Serilog.Ui.MsSqlServerProvider.Extensions;
using Serilog.Ui.Web.Extensions;
using Service;
using Shared.Settings;
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Add services to the container 

            #region Serilog Configuration

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();

            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection is missing.");

            builder.Services.AddSerilogUi(options =>
                options.UseSqlServer(opts => opts
                    .WithConnectionString(connectionString)
                    .WithSchema("Logging")
                    .WithTable("Logs")
                )
                .AddScopedAsyncAuthFilter<SerilogUiAuthorizationFilter>()
            );
            #endregion

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "AppointmentBookingApi", Version = "v1" });

                options.CustomSchemaIds(type => type.FullName ?? type.Name);

                options.MapType<DateOnly>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "date",
                    Example = new OpenApiString("2024-12-02")
                });

                options.MapType<TimeOnly>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "time",
                    Example = new OpenApiString("14:30")
                });

                options.AddSecurityDefinition("Bearer", securityScheme: new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT key"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Name ="Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices(builder.Configuration);
            builder.Services.AddSignalR();
            //builder.Services.AddScoped<INotificationRealTimeSender, NotificationRealTimeSender>();
            #region Hashing Decryption Configuration
            //((IConfigurationBuilder)builder.Configuration).Add(new DecryptedConfigurationSource());
            #endregion

            #region Settings Configuration
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
            builder.Services.Configure<BookingSettings>(builder.Configuration.GetSection("BookingSettings"));
            #endregion

            #region Add Identity
            //builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            //{
            //    options.Password.RequiredLength = 8;
            //    options.Password.RequireDigit = true;
            //    options.Password.RequireLowercase = true;
            //    options.Password.RequireUppercase = true;
            //    options.Password.RequireNonAlphanumeric = true;
            //})
            //.AddEntityFrameworkStores<ApplicationDbContext>()
            //.AddErrorDescriber<LocalizedIdentityErrorDescriber>()
            //.AddDefaultTokenProviders();
            #endregion

            #region Add Authentication

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"] ?? String.Empty))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs/notifications"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }

                };

            });

            #endregion

            #region Localization
            builder.Services.AddControllersWithViews();
            builder.Services.AddLocalization();
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("ar")
                };

                options.DefaultRequestCulture = new RequestCulture("ar");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                // Override default provider
                options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
                {
                    var acceptLang = context.Request.Headers["Accept-Language"].ToString();

                    if (string.IsNullOrWhiteSpace(acceptLang))
                        return await Task.FromResult(new ProviderCultureResult("ar"));

                    var lang = acceptLang.Split(',').FirstOrDefault()?.Trim().ToLower();

                    if (lang != "ar" && lang != "en")
                        lang = "ar";

                    return await Task.FromResult(new ProviderCultureResult(lang));
                }));
            });

            #endregion

            #region CORS
            //var allowedOrigins = new[]
            //{
            //    "http://localhost:4200",
            //};

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("UnifiedPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            #endregion

            #region RateLimiter Configuration
            builder.Services.AddMemoryCache();
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromSeconds(10),
                            //QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            //QueueLimit = 2 
                        }
                    )
                );
            });
            #endregion

            #endregion

            var app = builder.Build();

            #region Data Seeding
            using var scope = app.Services.CreateScope();
            var objectDataSeeding = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
            objectDataSeeding.DataSeed();
            #endregion

            #region Configure the HTTP request pipeline

            app.UseForwardedHeaders();

            app.UseWebSockets();

            app.UseSwagger();
            app.UseSwaggerUI();

            #region Localization Middleware
            var options = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options!.Value);
            #endregion

            app.UseErrorHandlingMiddleware();

            app.UseHttpsRedirection();

            #region CORS Middleware
            app.UseCors("UnifiedPolicy");
            #endregion

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var path = ctx.Context.Request.Path.Value ?? "";

                    if (path.StartsWith("/Images/", StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                        ctx.Context.Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin";
                        ctx.Context.Response.Headers["Vary"] = "Origin";
                    }
                }
            });

            #region IP Security Middleware
            app.UseWhen(
                context => !context.Request.Path.StartsWithSegments("/hubs"),
                appBuilder =>
                {
                    appBuilder.UseIpSecurity();
                }
            );
            #endregion

            app.UseAuthentication();

            app.UseAuthorization();

            #region RateLimiter Middleware
            app.UseWhen(
                context => !context.Request.Path.StartsWithSegments("/hubs"),
                appBuilder =>
                {
                    appBuilder.UseCustomRateLimiter();
                    appBuilder.UseRateLimiter();
                }
            );
            #endregion

            #region SerilogUI Middleware
            app.UseSerilogUiAuth();
            app.UseSerilogUi();
            #endregion

            app.MapControllers();

            app.Run();

            #endregion
        }
    }
}
