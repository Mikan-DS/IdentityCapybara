using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace IdentityTest4
{
    /// <summary>
    /// ���������� ����������� ����� � �������� ���������� ��� ������������ ��������.
    /// </summary>
    internal static class HostingExtensions
    {
        /// <summary>
        /// ������������� ������� ��� ���������� Web.
        /// </summary>
        /// <param name="builder">��������� ���-����������.</param>
        /// <returns>���-���������� � ������������ ���������.</returns>
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            // �������� ��� ������ ��������
            var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

            // �������� ������ ����������� � ���� ������
            string connectionString = builder.Configuration.GetConnectionString("DefaultString");

            // ��������� �����������
            builder.Services.AddControllers();

            // ����������� IdentityServer
            builder.Services.AddIdentityServer(options =>
            {
                options.EmitStaticAudienceClaim = true;
            })
            .AddConfigurationStore(options =>
            {
                // ����������� �������� ���� ������ ��� �������� ������������
                options.ConfigureDbContext = b => b.UseSqlite(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                // ����������� �������� ���� ������ ��� ������������ ������
                options.ConfigureDbContext = b => b.UseSqlite(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            });

            // ��������� �������������� � ������� ������� JWT
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    // ������������� URL-�����, ������������ � �������� Authority ��� �������� �������
                    options.Authority = Environment.GetEnvironmentVariable("ASPNETCORE_URLS"); // ������������ ������� ������
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                });

            // ��������� �������� ����������� "AdminScope"
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminScope", policy =>
                {
                    // ��������� ������������������� ������������
                    policy.RequireAuthenticatedUser();

                    // ��������� ������� "Scope" �� ��������� "AdminAPI"
                    policy.RequireClaim("Scope", "AdminAPI");
                });
            });

            // ���������� ����������� ���-����������
            return builder.Build();
        }


        /// <summary>
        /// ����������� �������� ����������.
        /// </summary>
        /// <param name="app">��������� ������ <see cref="WebApplication"/>.</param>
        /// <returns>��������� ������ <see cref="WebApplication"/>.</returns>
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.MapControllers(); // ���������� �������� ������������

            app.UseSerilogRequestLogging(); // ��������� ������������� �� ��� ����������� �������� � Serilog

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // ���������� �������� ������������ ��� ������������� ���������� � ������ ����������
            }

            app.UseIdentityServer(); // ��������� ������������� �� ��� ��������� IdentityServer

            app.UseAuthentication(); // ��������� ������������� �� ��� �������������� �������������
            app.UseAuthorization(); // ��������� ������������� �� ��� ����������� �������������

            InitializeDatabase(app); // �������������� ���� ������ �� ���������� �� ��������� // �������� ������ ���� ��� �������������

            return app; // ���������� ��������� ������ WebApplication
        }


        /// <summary>
        /// �������������� ���� ������ IdentityServer.
        /// </summary>
        /// <param name="app">��������� IApplicationBuilder.</param>
        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                // �������� ���� ������ PersistedGrantDbContext.
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                if (!context.Clients.Any())
                {
                    // ���������� �������� � ���� ������.
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    // ���������� �������� Identity � ���� ������.
                    foreach (var resource in Config.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    // ���������� ApiScopes � ���� ������.
                    foreach (var resource in Config.ApiScopes)
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

    }
}