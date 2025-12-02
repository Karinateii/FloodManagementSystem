using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Models.Configuration;
using GlobalDisasterManagement.Repositories.Interfaces;
using GlobalDisasterManagement.Repositories.Implementations;
using GlobalDisasterManagement.Services.Interfaces;
using GlobalDisasterManagement.Services.Implementations;
using GlobalDisasterManagement.Services.Implementation;
using GlobalDisasterManagement.Services.Abstract;
using GlobalDisasterManagement.Hubs;
using FloodManagementSystem.Hubs;
using GlobalDisasterManagement.Resources;
using Microsoft.AspNetCore.Identity;
using GlobalDisasterManagement.AccountRepository.Implementation;
using GlobalDisasterManagement.AccountRepository.Abstract;
using Microsoft.ML;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting Global Disaster Management System");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    builder.Services.AddDbContext<DisasterDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configure Localization
    builder.Services.AddLocalization();
    
    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[]
        {
            new CultureInfo("en"), // English
            new CultureInfo("fr"), // French
            new CultureInfo("ar"), // Arabic
            new CultureInfo("es"), // Spanish
            new CultureInfo("pt"), // Portuguese
            new CultureInfo("ha"), // Hausa
            new CultureInfo("yo"), // Yoruba
            new CultureInfo("ig")  // Igbo
        };

        options.DefaultRequestCulture = new RequestCulture("en");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
        
        // Set culture providers in priority order (Cookie first, then Query string, then AcceptLanguage)
        options.RequestCultureProviders = new List<IRequestCultureProvider>
        {
            new CookieRequestCultureProvider(),
            new QueryStringRequestCultureProvider(),
            new AcceptLanguageHeaderRequestCultureProvider()
        };
    });

    builder.Services.AddControllersWithViews()
        .AddViewLocalization()
        .AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (type, factory) =>
                factory.Create(typeof(SharedResources));
        });

    // Configure Email Settings
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

    // Register Repository Pattern
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    // Register Services
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IIncidentService, IncidentService>();
    builder.Services.AddScoped<IShelterService, ShelterService>();
    builder.Services.AddScoped<IEvacuationService, EvacuationService>();
    builder.Services.AddScoped<ISmsService, SmsService>();
    builder.Services.AddScoped<IUssdService, UssdService>();
    builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
    builder.Services.AddScoped<IVoiceService, VoiceService>();
    builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
    builder.Services.AddScoped<IIoTSensorService, IoTSensorService>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

    // Add Response Caching
    builder.Services.AddResponseCaching();
    builder.Services.AddMemoryCache();

    // Add SignalR for real-time notifications
    builder.Services.AddSignalR();

    builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
        .AddEntityFrameworkStores<DisasterDbContext>()
        .AddDefaultTokenProviders();
    builder.Services.ConfigureApplicationCookie(op => op.LoginPath = "/UserAuthentication/Login");

    // Configure JWT Authentication for Mobile API
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
    
    // Add JWT Bearer authentication (Identity already registered cookie auth)
    builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

    builder.Services.AddScoped<MLContext>();
    builder.Services.AddAuthorization();

    // Add Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Global Disaster Management API",
            Version = "v1",
            Description = "API for Global Disaster Management, Emergency Response, and Prediction System - Supporting floods, earthquakes, fires, hurricanes, and more",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Disaster Management Team",
                Email = "support@disastermanagement.com"
            }
        });
        
        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    else
    {
        // Enable Swagger in development
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Global Disaster Management API V1");
            c.RoutePrefix = "api/docs"; // Access at /api/docs
        });
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Add Request Localization Middleware - must be configured with options
    var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
    app.UseRequestLocalization(localizationOptions);

    app.UseResponseCaching();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    
    app.MapHub<DisasterAlertHub>("/disasterAlertHub");
    app.MapHub<IoTMonitoringHub>("/iotMonitoringHub");

    // Seed SMS templates and Admin user on startup
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            // Seed SMS templates
            var context = services.GetRequiredService<DisasterDbContext>();
            var logger = services.GetRequiredService<ILogger<SmsTemplateSeeder>>();
            var seeder = new SmsTemplateSeeder(context, logger);
            await seeder.SeedAsync();

            // Seed admin user
            await AdminSeeder.SeedAdminUser(services);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding data.");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
