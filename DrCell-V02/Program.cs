using DrCell_V02.Data;
using DrCell_V02.Data.Modelos;
using DrCell_V02.Middleware;
using DrCell_V02.Services;
using DrCell_V02.Services.Interface;
using DrCell_V02.HealthChecks;
using DrCell_V02.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Serilog;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

try
{
    var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(envFile))
    {
        Env.Load(envFile); // Cargar variables de entorno desde .env
        Log.Information($"🌱 Variables de entorno cargadas desde {envFile}");
    }
    else
    {
        Log.Warning($"⚠️ ADVERTENCIA: No se encontró el archivo .env en {envFile}. Asegúrate de que las variables de entorno estén configuradas correctamente.");

        var parentEnvFile = Path.Combine(Directory.GetCurrentDirectory(), "...", ".env");
        if (File.Exists(parentEnvFile))
        {
            Env.Load(parentEnvFile);
            Log.Information("Archivo .env cargado desde directorio padre: {EnvPath}", parentEnvFile);

        }
        else
        {
            Log.Warning("⚠️ ADVERTENCIA: No se encontró el archivo .env en el directorio padre. Asegúrate de que las variables de entorno estén configuradas correctamente.");

        }
    }

    var testVars = new[]
    {
       "DATABASE_CONNECTION_STRING",
        "JWT_SECRET",
        "JWT_ISSUER",
        "JWT_AUDIENCE",
        "CORS_ORIGINS"
    };
    foreach (var varName in testVars)
    {
        var value = Environment.GetEnvironmentVariable(varName);
        Log.Information($"🔑 {varName}: {(!string.IsNullOrEmpty(value) ? "✅ Cargada" : "❌ No encontrada")}");
    }
    // 🔑 Permitir lectura de variables desde .env y entorno
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // 🔒 OBTENER VARIABLES DE ENTORNO DE FORMA SEGURA
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? builder.Configuration["JWTKey:Secret"];

    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
        ?? builder.Configuration["JWTKey:ValidIssuer"];

    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
        ?? builder.Configuration["JWTKey:ValidAudience"];

    var corsOriginsEnv = Environment.GetEnvironmentVariable("CORS_ORIGINS");
    var corsOrigins = !string.IsNullOrEmpty(corsOriginsEnv)
        ? corsOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToArray()
        : builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();

    // 🌐 Agregar orígenes adicionales para desarrollo y producción
    var additionalOrigins = new List<string>();

    if (corsOrigins != null)
    {
        additionalOrigins.AddRange(corsOrigins);
    }

    // Agregar orígenes comunes para desarrollo y producción
    additionalOrigins.AddRange(new[]
    {
        "https://localhost:5000",
        "http://localhost:5000",
        "https://www.localhost:5000",
        "http://www.localhost:5000",
        "http://localhost:3000",
        "http://127.0.0.1:3000",
        "http://localhost:5000",
        "http://127.0.0.1:5000"
    });

    corsOrigins = additionalOrigins.Distinct().ToArray();

    // ✅ VALIDACIÓN DE VARIABLES CRÍTICAS EN PRODUCCIÓN
    if (builder.Environment.IsProduction())
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("🔴 CRÍTICO: DATABASE_CONNECTION_STRING es requerida en producción");
        }

        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException("🔴 CRÍTICO: JWT_SECRET es requerida en producción");
        }

        if (string.IsNullOrEmpty(jwtIssuer))
        {
            throw new InvalidOperationException("🔴 CRÍTICO: JWT_ISSUER es requerida en producción");
        }

        if (string.IsNullOrEmpty(jwtAudience))
        {
            throw new InvalidOperationException("🔴 CRÍTICO: JWT_AUDIENCE es requerida en producción");
        }

        if (corsOrigins == null || corsOrigins.Length == 0)
        {
            throw new InvalidOperationException("🔴 CRÍTICO: CORS_ORIGINS es requerida en producción");
        }

        // Validar longitud mínima del JWT Secret en producción
        if (jwtSecret.Length < 32)
        {
            throw new InvalidOperationException("🔴 CRÍTICO: JWT_SECRET debe tener al menos 32 caracteres en producción");
        }
    }

    // 📝 Configurar Serilog desde appsettings.json
    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .Enrich.WithProperty("Application", "DrCell-API")
            .WriteTo.Console()
            .WriteTo.File("logs/drcell-.log", rollingInterval: RollingInterval.Day);
    });

    // 📝 Logging temprano para debugging (SIN MOSTRAR CREDENCIALES)
    Log.Information($"🌍 Entorno: {builder.Environment.EnvironmentName}");
    Log.Information($"🔗 Conexión BD: {(!string.IsNullOrEmpty(connectionString) ? "✅ Configurada" : "❌ No configurada")}");
    Log.Information($"🔑 JWT Secret: {(!string.IsNullOrEmpty(jwtSecret) ? "✅ Configurado" : "❌ No configurado")}");
    Log.Information($"🌐 CORS Origins: {(corsOrigins?.Length > 0 ? string.Join(", ", corsOrigins) : "❌ No configurados")}");

    // 1. Configuración del DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("🔴 No se encontró la cadena de conexión");
        }

        options.UseNpgsql(connectionString);

        // Solo en Development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // 2. Configuración de CORS mejorada
    builder.Services.AddCors(options =>
    {
        var environment = builder.Environment.EnvironmentName;

        // Validar que existan orígenes configurados
        if (corsOrigins == null || corsOrigins.Length == 0)
        {
            Log.Warning("⚠️ ADVERTENCIA: No hay CORS origins configurados, usando configuración por defecto");
            corsOrigins = new[] { "http://localhost:3000", "https://localhost:5000" };
        }

        // Validar que los orígenes sean URLs válidas (solo en producción)
        if (builder.Environment.IsProduction())
        {
            foreach (var origin in corsOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    Log.Warning($"⚠️ URL de origen inválida: {origin}");
                }
            }
        }

        // Configuración permisiva para desarrollo
        if (environment == "Development")
        {
            options.AddPolicy("DevCORS", policy =>
            {
                policy.WithOrigins(
                          "http://localhost:3000",
                          "http://127.0.0.1:3000",
                          "http://localhost:5000",
                          "http://127.0.0.1:5000",
                          "https://localhost:3000",
                          "https://127.0.0.1:3000",
                          "https://localhost:5000",
                          "https://127.0.0.1:5000"
                      )
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        }
        else
        {
            // Configuración estricta para producción
            options.AddPolicy("ProductionCORS", policy =>
            {
                policy.WithOrigins(corsOrigins)
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                       .WithHeaders(
                          "Accept",
                          "Authorization",
                          "Content-Type",
                          "X-Requested-With",
                          "X-API-Key",
                          "Cache-Control",
                          "Pragma"
                      )
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
                      .SetIsOriginAllowed(origin =>
                      {
                          Log.Information($"🌐 Verificando origen CORS: {origin}");
                          return corsOrigins.Contains(origin) ||
                                 corsOrigins.Any(allowed => origin.StartsWith(allowed));
                      });
            });
        }

        // Configuración adicional para permitir todos los orígenes en desarrollo local
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var corsPolicy = builder.Environment.IsDevelopment() ? "DevCORS" : "ProductionCORS";

    // 3. RATE LIMITING
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "default",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = builder.Environment.IsDevelopment() ? 1000 : 100,
                    Window = TimeSpan.FromMinutes(1)
                });
        });

        options.AddFixedWindowLimiter("AuthPolicy", options =>
        {
            options.PermitLimit = builder.Environment.IsDevelopment() ? 200 : 20;
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 5;
        });

        options.AddFixedWindowLimiter("ApiPolicy", options =>
        {
            options.PermitLimit = builder.Environment.IsDevelopment() ? 2000 : 200;
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 10;
        });

        options.AddFixedWindowLimiter("CriticalPolicy", options =>
        {
            options.PermitLimit = builder.Environment.IsDevelopment() ? 500 : 50;
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 3;
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;

            context.HttpContext.Response.Headers["Retry-After"] = "60";
            context.HttpContext.Response.Headers["X-RateLimit-Limit"] = "100";
            context.HttpContext.Response.Headers["X-RateLimit-Remaining"] = "0";
            context.HttpContext.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString();

            var response = new
            {
                error = "Rate limit exceeded",
                message = "Too many requests. Please try again later.",
                retryAfter = 60
            };

            await context.HttpContext.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(response),
                cancellationToken: token);
        };
    });

    // 4. Configuración de autenticación JWT
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret ?? throw new InvalidOperationException("🔴 JWT Secret no configurado")))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers["Token-Expired"] = "true";
                }
                return Task.CompletedTask;
            }
        };
    });

    // 5. Configuración de Cookies seguras
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        // 🔧 FIX: Configuración más permisiva para desarrollo
        if (builder.Environment.IsDevelopment())
        {
            options.CheckConsentNeeded = context => false; // No requerir consentimiento en desarrollo
            options.MinimumSameSitePolicy = SameSiteMode.Lax; // Lax para compatibilidad con desarrollo
            options.HttpOnly = HttpOnlyPolicy.None; // Permitir acceso desde JavaScript en desarrollo
            options.Secure = CookieSecurePolicy.SameAsRequest; // Permitir HTTP en desarrollo
        }
        else
        {
            // Configuración segura para producción
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.HttpOnly = HttpOnlyPolicy.Always;
            options.Secure = CookieSecurePolicy.Always;
        }
    });

    // 6. Configuración de sesión
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.IsEssential = true;
        
        // 🔧 FIX: Configuración diferente para desarrollo vs producción
        if (builder.Environment.IsDevelopment())
        {
            options.Cookie.HttpOnly = false; // Permitir acceso desde JS en desarrollo
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTP permitido
            options.Cookie.SameSite = SameSiteMode.Lax; // Más permisivo
        }
        else
        {
            options.Cookie.HttpOnly = true; // Seguro en producción
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo HTTPS
            options.Cookie.SameSite = SameSiteMode.Strict; // Estricto en producción
        }
    });

    builder.Services.AddMemoryCache();

    // 7. Servicios de la aplicación
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddAuthorization();
    builder.Services.AddAutoMapper(typeof(Program));

    // 8. Configuración de Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "DrCell API",
            Version = "v1",
            Description = "API para la aplicación DrCell",
            Contact = new OpenApiContact
            {
                Name = "DrCell",
                Email = "info@drcell.com",
            }
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[]{}
            }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
        c.DocumentFilter<SwaggerSecurityFilter>();
    });


    // 9. Configuración de Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
        .AddDbContextCheck<ApplicationDbContext>("database")
        .AddCheck<MemoryHealthCheck>("memory")
        .AddCheck<DiskSpaceHealthCheck>("disk")
        .AddCheck<DatabaseConnectionHealthCheck>("database_connection");

    // 10. Inyección de dependencias personalizadas
    builder.Services.AddScoped<IUsuarioService, UsuarioService>();
    builder.Services.AddScoped<ICelularesService, EquiposService>();
    builder.Services.AddScoped<IPinesService, PinesService>();
    builder.Services.AddScoped<IModulosService, ModulosService>();
    builder.Services.AddScoped<IBateriasService, BateriasService>();
    builder.Services.AddScoped<IvCelularesInfoService, vCelularesInfoService>();
    builder.Services.AddScoped<IProductoService, ProductosService>();
    builder.Services.AddScoped<ICategoriaService, CategoriasService>();
    builder.Services.AddScoped<IStockService, StockService>();
    builder.Services.AddScoped<IVentaService, VentaService>();
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
    builder.Services.AddHostedService<StockCleanupJob>();
    
    var app = builder.Build();


    var ngrokurl= Environment.GetEnvironmentVariable("NGROK_BASE_URL")
        ?? builder.Configuration["ngrok:BaseUrl"];

    // 11. Configuración de Swagger por entorno
    var enableSwagger = builder.Configuration.GetValue<bool>("Swagger:EnabledInProduction", false);
    var swaggerPassword = Environment.GetEnvironmentVariable("SWAGGER_PASSWORD")
        ?? builder.Configuration.GetValue<string>("Swagger:Password");

    // Logger para información de inicio
    Log.Information($"🌍 Entorno: {app.Environment.EnvironmentName}");
    Log.Information($"📚 Swagger habilitado: {(app.Environment.IsDevelopment() || (app.Environment.IsStaging() && enableSwagger))}");

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "DrCell API v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "DrCell API - Development";
            c.DefaultModelsExpandDepth(-1);
            c.DocExpansion(DocExpansion.None);
        });
    }
    else if (app.Environment.IsStaging() && enableSwagger)
    {
        if (string.IsNullOrEmpty(swaggerPassword))
        {
            Log.Warning("⚠️ ADVERTENCIA: Swagger habilitado en Staging sin contraseña configurada");
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "DrCell API v1 - Staging");
            c.RoutePrefix = "docs";
            c.DocumentTitle = "DrCell API - Staging";
            c.DefaultModelsExpandDepth(-1);
            c.DocExpansion(DocExpansion.None);
        });

        // Middleware para proteger Swagger en staging
        app.UseWhen(context => context.Request.Path.StartsWithSegments("/docs") ||
                              context.Request.Path.StartsWithSegments("/swagger"),
            appBuilder =>
            {
                appBuilder.Use(async (context, next) =>
                {
                    if (!context.Request.Headers.ContainsKey("Authorization"))
                    {
                        context.Response.StatusCode = 401;
                        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger Documentation\"";
                        await context.Response.WriteAsync("Unauthorized access to API documentation");
                        return;
                    }

                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (authHeader.StartsWith("Basic "))
                    {
                        var encodedCredentials = authHeader.Substring("Basic ".Length);
                        var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                        var parts = credentials.Split(':');

                        if (parts.Length == 2 && parts[0] == "admin" && parts[1] == swaggerPassword)
                        {
                            await next();
                            return;
                        }
                    }

                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid credentials");
                });
            });
    }
    else
    {
        // 🔒 Producción: Swagger COMPLETAMENTE DESHABILITADO
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/docs"))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Not Found");
                return;
            }
            await next();
        });
    }

    // 12. Configuración de Health Checks Endpoints
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";

            var result = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                totalDuration = report.TotalDuration.TotalMilliseconds,
                entries = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration.TotalMilliseconds,
                    description = e.Value.Description,
                    exception = e.Value.Exception?.Message
                })
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        }
    });

    // Health check simplificado para load balancers
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live") || check.Name == "self"
    });

    // Health check completo para monitoreo
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready") || check.Name == "database"
    });

    // 13. Configuración de middleware pipeline MEJORADA
    app.UseRateLimiter();
    app.UseStaticFiles();
    app.UseRouting();

    // 🌐 CORS debe ir ANTES de Authentication y Authorization
    app.UseCors(corsPolicy);

    app.UseCookiePolicy();
    app.UseSession();

    // 🔑 Middleware personalizado para JWT en cookies
    app.UseMiddleware<JwtCookieMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    // ✅ Agregar logging de requests con Serilog
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? Serilog.Events.LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? Serilog.Events.LogEventLevel.Error
                : Serilog.Events.LogEventLevel.Information;
    });

    // 🌐 Middleware adicional para debugging CORS en desarrollo
    if (app.Environment.IsDevelopment())
    {
        app.Use(async (context, next) =>
        {
            // Log información de CORS para debugging
            Log.Information($"🌐 Request Origin: {context.Request.Headers["Origin"]}");
            Log.Information($"🌐 Request Method: {context.Request.Method}");
            Log.Information($"🌐 Request Path: {context.Request.Path}");

            await next();

            // Log headers de respuesta
            if (context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {
                Log.Information($"🌐 Response CORS Headers: {context.Response.Headers["Access-Control-Allow-Origin"]}");
            }
        });
    }

    app.MapControllers();

    // Log final antes de iniciar (SIN MOSTRAR CREDENCIALES)
    Log.Information("🚀 DrCell API iniciada correctamente");
    Log.Information($"🔗 Base de datos: {(!string.IsNullOrEmpty(connectionString) ? "✅ Conectada" : "❌ Error")}");
    Log.Information($"🔑 Autenticación: {(!string.IsNullOrEmpty(jwtSecret) ? "✅ Configurada" : "❌ Error")}");
    Log.Information($"🌐 CORS Policy: {corsPolicy}");
    Log.Information($"🌐 CORS Origins permitidos: {string.Join(", ", corsOrigins ?? new string[0])}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ ERROR FATAL AL INICIAR LA APP");
    throw;
}
finally
{
    Log.CloseAndFlush();
}