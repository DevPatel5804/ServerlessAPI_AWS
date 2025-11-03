using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon.Runtime;
using Amazon; 
using serverless_auth.BusinessObjects;
using serverless_auth.Utilities;

namespace serverless_auth
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
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            var awsOptions = Configuration.GetAWSOptions();

            //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ||
            //    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == null)
            //{
            //    awsOptions.Credentials = new BasicAWSCredentials("dummy", "dummy");
            //    Console.WriteLine("Configuring AWS for DynamoDB Local at http://localhost:8000");
            //}
            //else
            {
                Console.WriteLine($"Configuring AWS for DynamoDB in region: {awsOptions.Region?.SystemName}");
            }

            services.AddDefaultAWSOptions(awsOptions);

            services.AddSingleton<IAmazonDynamoDB>(serviceProvider =>
            {
                var config = new AmazonDynamoDBConfig();

                //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ||
                //    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == null)
                //{
                //    config.ServiceURL = "http://localhost:8000";
                //}
                //else
                {
                    if (awsOptions.Region != null)
                    {
                        config.RegionEndpoint = RegionEndpoint.GetBySystemName(awsOptions.Region.SystemName);
                    }
                    else
                    {
                        Console.WriteLine("Warning: AWS Region not configured for production environment. Using default region.");
                    }
                }
                return new AmazonDynamoDBClient(config);
            });

            services.AddScoped<IDynamoDBContext>(serviceProvider =>
            {
                var client = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
                return new DynamoDBContext(client);
            });

            services.AddScoped<UserBusinessObject>();
            services.AddSingleton<JWTUtilities, JWTServiceUtilities>();


            // Configure JWT Authentication.
            var jwtSettings = Configuration.GetSection("JwtSettings");
            var secret = jwtSettings.GetValue<string>("Secret");
            var issuer = jwtSettings.GetValue<string>("Issuer");
            var audience = jwtSettings.GetValue<string>("Audience");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

//            app.UseHttpsRedirection(); // Keep this if you want HTTPS redirection for local dev as well

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/jwt-auth", async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            });
        }
    }
}
