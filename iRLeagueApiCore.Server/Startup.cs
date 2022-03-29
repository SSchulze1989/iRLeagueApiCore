using FluentValidation;
using iRLeagueApiCore.Communication.Converters;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace iRLeagueApiCore.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IISServerOptions>(options =>
                options.AutomaticAuthentication = false);

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("*ireleaguemanager.net*")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddControllers().AddJsonOptions(option =>
            {
                option.JsonSerializerOptions.Converters.Add(new JsonTimeSpanConverter());
            });
            services.AddSwaggerGen(c =>
            {
                var readHostUrls = Configuration["ASPNETCORE_HOSTURLS"];
                if (readHostUrls != null)
                {
                    var hostUrls = readHostUrls.Split(';');
                    foreach (var hostUrl in hostUrls)
                    {
                        c.AddServer(new OpenApiServer() { Url = hostUrl });
                    }
                }
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "iRLeagueApiCore.Server", Version = "v1" });
                c.OperationFilter<OpenApiParameterIgnoreFilter>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below." +
                      "\r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                            {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
                var xmlPath = Path.Combine(AppContext.BaseDirectory, "iRLeagueApiCore.Server.xml");
                c.IncludeXmlComments(xmlPath);
                xmlPath = Path.Combine(AppContext.BaseDirectory, "iRLeagueApiCore.Communication.xml");
                c.IncludeXmlComments(xmlPath);
                c.MapType<TimeSpan>(() => new OpenApiSchema()
                {
                    Type = "string",
                    Format = "duration",
                    Pattern = "hh:mm:ss.fffff",
                    Example = new OpenApiString(new TimeSpan(0, 1, 2, 34, 567).ToString(@"hh\:mm\:ss\.fffff"))
                });
                c.MapType<TimeSpan?>(() => new OpenApiSchema()
                {
                    Type = "string",
                    Format = "duration",
                    Nullable = true,
                    Pattern = "hh:mm:ss.fffff",
                    Example = new OpenApiString(new TimeSpan(0, 1, 2, 34, 567).ToString(@"hh\:mm\:ss\.fffff"))
                });
            });

            // try get connection string
            services.AddDbContextFactory<ApplicationDbContext, ApplicationDbContextFactory>();
            services.AddScoped(x =>
                x.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
            services.AddDbContextFactory<LeagueDbContext, LeagueDbContextFactory>();
            services.AddScoped(x =>
                x.GetRequiredService<IDbContextFactory<LeagueDbContext>>().CreateDbContext());

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer  
            .AddJwtBearer(options =>
             {
                 options.SaveToken = true;
                 options.RequireHttpsMetadata = false;
                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidAudience = Configuration["JWT:ValidAudience"],
                     ValidIssuer = Configuration["JWT:ValidIssuer"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                 };
             });

            services.AddScoped<LeagueAuthorizeAttribute>();
            services.AddScoped<InsertLeagueIdAttribute>();

            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    //c.SwaggerEndpoint("./v1/swagger.json", "TestDeploy v1");
                    //c.RoutePrefix = string.Empty;
                    string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "./swagger/" : "";
                    c.SwaggerEndpoint($"{swaggerJsonBasePath}v1/swagger.json", "iRLeagueApiCore.Server v1");
                });
            }

            //app.UseHttpsRedirection();

            app.UseFileServer();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
