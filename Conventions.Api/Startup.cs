namespace Conventions.Api
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using Conventions.Api.Configuration;
    using Conventions.Interaction.Authorization;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private const string PermissionsClaimType = "permissions";

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInjections(Configuration);
            services.AddControllers();
            services.AddSwaggerGen(
                c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Conventions", Version = "v1" });
                        c.AddSecurityDefinition(
                            "Bearer",
                            new OpenApiSecurityScheme
                            {
                                Description = @"JWT Authorization header using the Bearer scheme. Prefix with `Bearer `.",
                                Name = "Authorization",
                                In = ParameterLocation.Header,
                                Type = SecuritySchemeType.ApiKey,
                                Scheme = "Bearer"
                            });
                        c.AddSecurityRequirement(
                            new OpenApiSecurityRequirement()
                            {
                                {
                                    new OpenApiSecurityScheme
                                    {
                                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                                        Scheme = "oauth2",
                                        Name = "Bearer",
                                        In = ParameterLocation.Header,
                                    },
                                    new List<string>()
                                }
                            });
                    });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder
                        .WithOrigins("http://localhost:8080")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });
            });

            var domain = $"https://{Configuration["Auth0:Domain"]}/";
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = domain;
                    options.Audience = Configuration["Auth0:Audience"];
                    // If the access token does not have a `sub` claim, `User.Identity.Name` will be `null`. Map it to a different claim by setting the NameClaimType below.
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

            services.AddAuthorization(
                options =>
                    {
                        GenerateSimplePermissionsPolicy(options, Policies.ReadUsers, Permissions.ReadUsers);
                        GenerateSimplePermissionsPolicy(options, Policies.UpdateUsers, Permissions.UpdateUsers);
                        GenerateSimplePermissionsPolicy(options, Policies.DeleteUsers, Permissions.DeleteUsers);

                        GenerateSimplePermissionsPolicy(options, Policies.CreateVenues, Permissions.CreateVenues);
                        GenerateSimplePermissionsPolicy(options, Policies.UpdateVenues, Permissions.UpdateVenues);
                        GenerateSimplePermissionsPolicy(options, Policies.DeleteVenues, Permissions.DeleteVenues);

                        GenerateSimplePermissionsPolicy(options, Policies.CreateConventions, Permissions.CreateConventions);
                        GenerateSimplePermissionsPolicy(options, Policies.UpdateConventions, Permissions.UpdateConventions);
                        GenerateSimplePermissionsPolicy(options, Policies.DeleteConventions, Permissions.DeleteConventions);
                        GenerateSimplePermissionsPolicy(options, Policies.SignUpConventions, Permissions.SignUpConventions);
                        GenerateSimplePermissionsPolicy(options, Policies.EjectConventions, Permissions.EjectConventions);

                        options.AddPolicy(
                            Policies.CreateTalks,
                            policy => policy.RequireAssertion(
                                context => context.User.HasClaim(
                                    c => c.Type == "permissions"
                                         && (c.Value == Permissions.CreateTalksOnBehalf || c.Value == Permissions.CreateTalks))));
                        GenerateSimplePermissionsPolicy(options, Policies.UpdateTalks, Permissions.UpdateTalks);
                        GenerateSimplePermissionsPolicy(options, Policies.DeleteTalks, Permissions.DeleteTalks);
                        GenerateSimplePermissionsPolicy(options, Policies.SignUpTalks, Permissions.SignUpTalks);
                        GenerateSimplePermissionsPolicy(options, Policies.EjectTalks, Permissions.EjectTalks);
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigin");
            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(
                c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conventions V1");
                    });

            app.UseEndpoints(endpoints =>
            {
                var conventionBuilder = endpoints.MapControllers();
                conventionBuilder.RequireAuthorization();
            });
        }

        private static void GenerateSimplePermissionsPolicy(AuthorizationOptions options, string policyName, string permission) =>
            options.AddPolicy(
                policyName,
                policy => policy.AddRequirements(new ClaimsAuthorizationRequirement(PermissionsClaimType, new[] { permission })));
    }
}
