using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Sandbox.api.Middleware;
using Sandbox.db;
using Sandbox.db.Repositories;
using W.Ind.Core.Config;
using W.Ind.Core.Entity;
using W.Ind.Core.Helper;


var builder = WebApplication.CreateBuilder(args);

// Map JWT Configuration section
JwtConfig jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfig>()!;


// .ConfigureWicServices(jwtConfig): Configure injectable W.Ind.Core Services
    // Optional type parameters <TConfig, TUser, TUserKey>
        // TConfig: Any class derived from default JwtConfig
            // default: JwtConfig
        // TUser: Your User entity type - any class derived from UserBase<TUserKey>
            // default: CoreUser
        // TUserKey: The PK type of your User entity
            // default: long
builder.Services.ConfigureWicServices(jwtConfig);


// Inject derived sandbox.db.RefreshTokenRepository
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();


// Configure EF Identity
builder.Services.AddIdentity<CoreUser, CoreRole>()
    .AddEntityFrameworkStores<SandboxDbContext>()
    .AddDefaultTokenProviders();


// Add DbContext (Default Connection)
builder.Services.AddDbContext<SandboxDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// Configure Swagger with Bearer token auth scheme
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Sandbox API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
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
    };

    options.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// Inject derived Access Token Middleware
app.UseMiddleware<JwtAccessHandler>();


app.Run();
