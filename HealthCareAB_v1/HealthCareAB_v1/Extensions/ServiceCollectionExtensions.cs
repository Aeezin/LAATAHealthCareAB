using System.Text;
using HealthCareAB_v1.Configuration;
using HealthCareAB_v1.Constants;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace HealthCareAB_v1.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor(); // Required for AuthService to check current user
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }

        public static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"))
            );

            services
                .AddIdentity<ApplicationUser, IdentityRole<int>>(options => { })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IAppDbContext>(provider =>
                provider.GetRequiredService<AppDbContext>()
            );

            return services;
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequiredUniqueChars = 1;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequiredLength = 8;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    options.User.AllowedUserNameCharacters =
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                    options.User.RequireUniqueEmail = true;

                    options.SignIn.RequireConfirmedEmail = false; //Set true if email verification is implemented.
                })
                //.AddRoles<IdentityRole>(); If we want to use identity cores role managment.
                .AddEntityFrameworkStores<AppDbContext>()
                .AddSignInManager<SignInManager<ApplicationUser>>()
                .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Bind and validate JWT settings
            var jwtSettings =
                configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                ?? throw new InvalidOperationException(
                    "JwtSettings configuration section is missing"
                );

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtSecret =
                        Environment.GetEnvironmentVariable("JWT_SECRET")
                        ?? throw new InvalidOperationException(
                            "JWT_SECRET environment variable is not set."
                        );
                    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                    };

                    // Read JWT from HttpOnly cookie
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies[CookieNames.Jwt];
                            return Task.CompletedTask;
                        },
                    };
                });

            return services;
        }
    }
}
