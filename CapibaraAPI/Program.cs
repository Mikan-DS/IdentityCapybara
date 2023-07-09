using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы в контейнер.
builder.Services.AddControllers();
// Добавляем поддержку Swagger/OpenAPI. 
// Подробнее о настройке Swagger/OpenAPI можно узнать по адресу https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Настраиваем аутентификацию с использованием схемы "Bearer" и JWT-токенов.
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("IdentitiServerUrl"); // Указываем URL авторитетного сервера (Identity Provider)

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false // Отключаем проверку аудитории токена
        };
    });

// Настраиваем авторизацию.
builder.Services.AddAuthorization(options =>
{
    // Добавляем политику "CapibarsScope", которая требует аутентифицированного пользователя 
    // и наличие у него утверждения "Scope" со значением "CapibarAPI".
    options.AddPolicy("CapibarsScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("Scope", "CapibarAPI");
    });
});


var app = builder.Build();

// Перенаправление с HTTP на HTTPS.
app.UseHttpsRedirection();

// Включение аутентификации и авторизации в приложении.
app.UseAuthentication();
app.UseAuthorization();

// Настраиваем маршрутизацию для контроллеров.
app.MapControllers();

// Добавляем обработчик для корневого пути приложения ("/"), который возвращает строку "I love carburator".
app.MapGet("/", () => "I love carburator");

// Запускаем приложение.
app.Run();

