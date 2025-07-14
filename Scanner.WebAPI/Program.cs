using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Mobile_Scanner.DataAccess;
using Scanner.Application.Services;
using Scanner.Core.Application.Interfaces;
using Scanner.Core.Application.Services;
using Scanner.Infrastructure.Data;
using Scanner.Infrastructure.DataAccess;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add necessary services
builder.Services.AddScoped<RefreshTokenRepository>();
builder.Services.AddScoped<IConnectionStringProvider, ConnectionStringProvider>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddScoped<LoadingDataAccess>();
builder.Services.AddScoped<UnloadingDataAccess>();
builder.Services.AddScoped<ConfirmDeliveryDataAccess>();
builder.Services.AddScoped<NCRDataAccess>();
builder.Services.AddScoped<OptimizedRouteDataAccess>();
builder.Services.AddScoped<RefreshTokenRepository>();
builder.Services.AddScoped<RunningSheetDataAccess>();

builder.Services.AddAuthorization();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDbContext<TenantContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TenantDatabase")));

builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IDriversService, DriversService>();
builder.Services.AddScoped<DriversDataAccess>();
builder.Services.AddScoped<DataContextDapper>();
builder.Services.AddScoped<IConnectionStringProvider, ConnectionStringProvider>();

// Configure dynamic tenant-specific DbContext
builder.Services.AddScoped<AppDbContext>(provider =>
{
    var tenantService = provider.GetRequiredService<ITenantService>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var tenantGuid = (httpContextAccessor.HttpContext.Request.Headers["Tenant-ID"]);
    var connectionString = tenantService.GetConnectionString(tenantGuid);

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new Exception("Invalid Tenant ID");
    }

    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
    optionsBuilder.UseSqlServer(connectionString);

    return new AppDbContext(optionsBuilder.Options);
});

// Add Swagger with JWT Bearer support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Mobile Scanner API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by the JWT token"
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
                }
            },
            new string[] {}
        }
    });
});

// Build the application
var app = builder.Build();

// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mobile Scanner API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger at root URL
});

// Add middleware
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Define the endpoints for the application
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Run the application
app.Run();
