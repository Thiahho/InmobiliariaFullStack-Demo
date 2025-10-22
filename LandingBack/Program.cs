using LandingBack.Data;
using LandingBack.Services;
using LandingBack.Services.Interfaces;
using LandingBack.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
// using Hangfire; // Commented out for now
// using Hangfire.PostgreSql;
// using LandingBack.Filters; // Commented out while Hangfire is disabled

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsBuilder =>
    {
        // Leer orígenes permitidos desde configuración
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" }; // Fallback si no hay configuración

        corsBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add Authentication & JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured. Set the 'Jwt:Key' environment variable.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "InmobiliariaApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "InmobiliariaApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireAgenteRole", policy => policy.RequireRole("Admin", "Agente"));
    options.AddPolicy("RequireCargadorRole", policy => policy.RequireRole("Admin", "Agente", "Cargador"));
});

// Rate limiting will be added in a future version

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IPropiedadesService, PropiedadesService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IGeoService, GeoService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<IVisitaService, VisitaService>();
builder.Services.AddScoped<IVisitaAuditoriaService, VisitaAuditoriaService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVisitaJobService, VisitaJobService>();
builder.Services.AddScoped<IAdvancedSearchService, AdvancedSearchService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

// Configure Hangfire (commented out for now - can be enabled later)
/*
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();
*/

// Add Health Checks (temporarily disabled DB check)
builder.Services.AddHealthChecks();
    //.AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAll");


app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure Hangfire Dashboard (commented out for now)
/*
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});
*/

app.MapControllers();
app.MapHealthChecks("/health");

// Configure recurring jobs (commented out for now)
/*
RecurringJob.AddOrUpdate<IVisitaJobService>("process-reminders", 
    service => service.ProcessRemindersAsync(), 
    Cron.Hourly); // Ejecutar cada hora para verificar recordatorios
*/

app.Run();
