using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using YashGems.Identity.Application.Interfaces;
using YashGems.Identity.Application.Messaging;
using YashGems.Identity.Application.Services;
using YashGems.Identity.Infrastructure.Authentication;
using YashGems.Identity.Infrastructure.Data;
using YashGems.Identity.Infrastructure.Messaging;
using YashGems.Identity.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext (Dùng MySQL)
var connectionString = builder.Configuration.GetConnectionString("MySQL");
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseMySql(
        connectionString, ServerVersion.AutoDetect(connectionString)
    )
);

// 2. Đăng ký các Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();

// 3. Đăng ký các Infrastructure Service
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>(); // giữ 1 kết nối duy nhất
builder.Services.AddScoped<ITokenProvider, JwtProvider>();

// 4. Đăng ký Application Services
builder.Services.AddScoped<IAuthService, AuthService>();

// 5. Cấu hình JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // configure events for JWT Bearer (middleware)
        // nếu kco middleware này thì khi 1 AC hoặc RFT đã bị revoke thì vẫn còn dùng đc
        // phải có thì middleware này kiểm tra thấy đã bị revoke thì return về luôn.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    context.Fail("Missing JTI claim in token");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<IdentityDbContext>();

                var refreshToken = await db.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.AccessTokenJti == jti);

                if (refreshToken != null && !refreshToken.IsActive)
                {
                    context.Fail("Token has been revoked or expired");
                    return;
                }
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();