using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SystemInfoApi.Middleware;
using SystemInfoApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RepoDb;
using Serilog;

namespace SystemInfoApi
{
    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Program.cbCPUMetricsCollection.Clear();
            Program.cbMemoryMetricsCollection.Clear();
            Program.cbDrivesMetricsCollection.Clear();

            Configuration = configuration;

            Program.CurrentDatabase = configuration.GetSection( "DatabaseInfo" ).GetSection( "CurrentDatabase" ).Value;
            switch ( Program.CurrentDatabase )
            {
                case "SQLite":
                    Program.CurrentConnectionString = configuration.GetSection( "connectionstrings" ).GetSection( "sqLite" ).Value;
                    GlobalConfiguration
                        .Setup()
                        .UseSqlite();
                    break;
                case "Postgres":
                    Program.CurrentConnectionString = configuration.GetSection( "connectionstrings" )
                        .GetSection( "postgresConnection" ).Value;
                    GlobalConfiguration
                        .Setup()
                        .UsePostgreSql();
                    break;
                case "MySQL":
                    Program.CurrentConnectionString = configuration.GetSection( "connectionstrings" )
                        .GetSection( "mysqlConnection" ).Value;
                    GlobalConfiguration
                        .Setup()
                        .UseMySql();
                    break;
                case "SQLServer":
                    Program.CurrentConnectionString = configuration.GetSection( "connectionstrings" )
                        .GetSection( "sqlConnection" ).Value;
                    GlobalConfiguration
                        .Setup()
                        .UseSqlServer();
                    break;
                case "LiteDB":
                    Program.CurrentConnectionString =
                        configuration.GetSection( "connectionstrings" ).GetSection( "liteDB" ).Value;
                    break;
                case "MongoDB":
                    Program.CurrentConnectionString = configuration.GetSection( "connectionstrings" )
                        .GetSection( "mongoConnection" ).Value;
                    break;
                case "RavenDB":
                    Program.CurrentConnectionString = configuration.GetSection( "connectionstrings" )
                        .GetSection( "ravenDBConnection" ).Value;
                    break;
                default:
                    Program.CurrentConnectionString =
                        configuration.GetSection( "connectionstrings" ).GetSection( "sqLite" ).Value;
                    break;
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            services.AddControllers().AddJsonOptions( o =>
            {
                //o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                //o.JsonSerializerOptions.MaxDepth = 0;
                //o.JsonSerializerOptions.IgnoreNullValues = true;
                //o.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
            } );

            services.AddSwaggerGen( c =>
            {
                c.SwaggerDoc( "v1", new OpenApiInfo { Title = "SystemInfoApi", Version = "v1" } );
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token **_only_**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // must be lower case
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition( securityScheme.Reference.Id, securityScheme );
                c.AddSecurityRequirement( new OpenApiSecurityRequirement
                {
                    { securityScheme, new string[] { } }
                } );

                // add Basic Authentication
                // var basicSecurityScheme = new OpenApiSecurityScheme
                // {
                //     Type = SecuritySchemeType.Http,
                //     Scheme = "basic",
                //     Reference = new OpenApiReference { Id = "BasicAuth", Type = ReferenceType.SecurityScheme }
                // };
                // c.AddSecurityDefinition(basicSecurityScheme.Reference.Id, basicSecurityScheme);
                // c.AddSecurityRequirement(new OpenApiSecurityRequirement
                // {
                //     { basicSecurityScheme, new string[] { } }
                // });
            } );

            services.AddTokenAuthentication( Configuration );
            services.AddHostedService<SaveStatsPerSecond>();
            services.AddHostedService<SaveStatsPerMinute>();
            // services.AddHostedService<SaveStatsPerHour>();
            // services.AddHostedService<SaveStatsPerDay>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI( c => c.SwaggerEndpoint( "/swagger/v1/swagger.json", "SystemInfoApi v1" ) );
            }

            app.UseHttpsRedirection();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints( endpoints => { endpoints.MapControllers(); } );
        }
    }
}