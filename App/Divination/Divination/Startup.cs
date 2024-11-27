using Divination.DivinationDeployer;
using Divination.Common;
using Divination.Db.Context;
using Divination.Db.Interface;
using Divination.Db.Repository;
using Divination.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;

namespace Divination
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CommonOptions>(Configuration);
            services.AddControllersWithViews();
            services.AddLogging();
            services.AddSingleton<IErrorNotifyService, ErrorNotifyService>();
            services.AddDbContextPool<DbPgContext>((opt) =>
            {
                opt.EnableSensitiveDataLogging();
                var connectionString = Configuration.GetConnectionString("MainConnection");
                opt.UseNpgsql(connectionString);
            });

            services.AddCors();
            services
                .AddAuthentication()
                .AddJwtBearer("Token", (options) =>
                {
                    AuthOptions settings = Configuration.GetSection("AuthOptions").Get<AuthOptions>();
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //// укзывает, будет ли валидироваться издатель при валидации токена
                        ValidateIssuer = true,
                        //// строка, представляющая издателя
                        ValidIssuer = settings.Issuer,
                        //// будет ли валидироваться потребитель токена
                        ValidateAudience = true,
                        //// установка потребителя токена
                        ValidAudience = settings.Audience,
                        //// будет ли валидироваться время существования
                        ValidateLifetime = true,
                        // установка ключа безопасности
                        IssuerSigningKey = settings.GetSymmetricSecurityKey(),
                        // валидация ключа безопасности
                        ValidateIssuerSigningKey = true,
                    };
                });

            services.AddAuthorization(options =>
            {
                var defPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("Token")
                    .Build();                   
                options.AddPolicy("Token", defPolicy);
                options.DefaultPolicy = defPolicy;
            });

            services.AddScoped<IRepository<Db.Model.User>, Repository<Db.Model.User>>();         
            services.AddScoped<IRepository<Db.Model.UserHistory>, Repository<Db.Model.UserHistory>>();
            services.AddScoped<IRepository<Db.Model.UserSettings>, Repository<Db.Model.UserSettings>>();
            services.AddScoped<IRepository<Db.Model.UserSettingsHistory>, Repository<Db.Model.UserSettingsHistory>>();
            services.AddScoped<IRepository<Db.Model.Formula>, Repository<Db.Model.Formula>>();
            services.AddScoped<IRepository<Db.Model.FormulaHistory>, Repository<Db.Model.FormulaHistory>>();
            services.AddScoped<IRepository<Db.Model.Product>, Repository<Db.Model.Product>>();
            services.AddScoped<IRepository<Db.Model.ProductHistory>, Repository<Db.Model.ProductHistory>>();
            services.AddScoped<IRepository<Db.Model.Incoming>, Repository<Db.Model.Incoming>>();
            services.AddScoped<IRepository<Db.Model.IncomingHistory>, Repository<Db.Model.IncomingHistory>>();
            services.AddScoped<IRepository<Db.Model.Outgoing>, Repository<Db.Model.Outgoing>>();
            services.AddScoped<IRepository<Db.Model.OutgoingHistory>, Repository<Db.Model.OutgoingHistory>>();
            services.AddScoped<IRepository<Db.Model.Reserve>, Repository<Db.Model.Reserve>>();
            services.AddScoped<IRepository<Db.Model.ReserveHistory>, Repository<Db.Model.ReserveHistory>>();
            services.AddScoped<IRepository<Db.Model.Correction>, Repository<Db.Model.Correction>>();
            services.AddScoped<IRepository<Db.Model.CorrectionHistory>, Repository<Db.Model.CorrectionHistory>>();

            services.AddScoped<IAllocateReservesService, AllocateReservesService>();
            services.AddScoped<IReservesRevisorService, ReservesRevisorService>();

            services.AddDataServices();

            services.AddScoped<IDeployService, DeployService>();
            services.AddScoped<ICalculator, CalculatorNCalc>();            
            services.AddSwaggerGen(swagger =>
            {                
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                          Array.Empty<string>()
                    }
                });
            });            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
                       
            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });                   
        }
    }
}
