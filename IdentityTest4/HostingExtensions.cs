using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace IdentityTest4
{
    /// <summary>
    /// Внутренний статический класс с методами расширения для конфигурации хостинга.
    /// </summary>
    internal static class HostingExtensions
    {
        /// <summary>
        /// Конфигурирует сервисы для приложения Web.
        /// </summary>
        /// <param name="builder">Строитель веб-приложения.</param>
        /// <returns>Веб-приложение с настроенными сервисами.</returns>
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            // Получаем имя сборки миграций
            var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

            // Получаем строку подключения к базе данных
            string connectionString = builder.Configuration.GetConnectionString("DefaultString");

            // Добавляем контроллеры
            builder.Services.AddControllers();

            // Настраиваем IdentityServer
            builder.Services.AddIdentityServer(options =>
            {
                options.EmitStaticAudienceClaim = true;
            })
            .AddConfigurationStore(options =>
            {
                // Настраиваем контекст базы данных для хранения конфигурации
                options.ConfigureDbContext = b => b.UseSqlite(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                // Настраиваем контекст базы данных для операционных данных
                options.ConfigureDbContext = b => b.UseSqlite(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            });

            // Добавляем аутентификацию с помощью токенов JWT
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    // Устанавливаем URL-адрес, используемый в качестве Authority для проверки токенов
                    options.Authority = Environment.GetEnvironmentVariable("ASPNETCORE_URLS"); // Используется текущий сервер
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                });

            // Добавляем политику авторизации "AdminScope"
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminScope", policy =>
                {
                    // Требуется аутентифицированный пользователь
                    policy.RequireAuthenticatedUser();

                    // Требуется наличие "Scope" со значением "AdminAPI"
                    policy.RequireClaim("Scope", "AdminAPI");
                });
            });

            // Возвращаем построенное веб-приложение
            return builder.Build();
        }


        /// <summary>
        /// Настраивает конвейер приложения.
        /// </summary>
        /// <param name="app">Экземпляр класса <see cref="WebApplication"/>.</param>
        /// <returns>Экземпляр класса <see cref="WebApplication"/>.</returns>
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.MapControllers(); // Отображает маршруты контроллеров

            app.UseSerilogRequestLogging(); // Добавляет промежуточное ПО для регистрации запросов в Serilog

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Использует страницу разработчика при возникновении исключений в режиме разработки
            }

            app.UseIdentityServer(); // Добавляет промежуточное ПО для поддержки IdentityServer

            app.UseAuthentication(); // Добавляет промежуточное ПО для аутентификации пользователей
            app.UseAuthorization(); // Добавляет промежуточное ПО для авторизации пользователей

            InitializeDatabase(app); // Инициализирует базу данных со значениями по умолчанию // Возможно убрать если нет необходимости

            return app; // Возвращает экземпляр класса WebApplication
        }


        /// <summary>
        /// Инициализирует базу данных IdentityServer.
        /// </summary>
        /// <param name="app">Экземпляр IApplicationBuilder.</param>
        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                // Миграция базы данных PersistedGrantDbContext.
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                if (!context.Clients.Any())
                {
                    // Добавление клиентов в базу данных.
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    // Добавление ресурсов Identity в базу данных.
                    foreach (var resource in Config.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    // Добавление ApiScopes в базу данных.
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