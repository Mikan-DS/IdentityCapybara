using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

try
{
    IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    // Путь к файлу PFX с закрытым ключом
    string pfxFilePath = configuration.GetValue<string>("pfxFile");
    string pfxPassword = configuration.GetValue<string>("pfxPassword");

    // Загрузка X509 сертификата из pem файла
    X509Certificate2 certificate = new X509Certificate2(pfxFilePath, "P@55w0rd");

    // Получение имени пользователя
    string username = configuration.GetValue<string>("ClientName");

    // Создание JWT токена
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
                    new Claim("username", username),
                    new Claim("X509Certificate", Convert.ToBase64String(certificate.GetRawCertData()))
                }),
        SigningCredentials = new X509SigningCredentials(certificate),

        // Установка срока действия токена
        Expires = DateTime.UtcNow.AddYears(1)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);

    // Запись JWT токена в файл
    string jwtToken = tokenHandler.WriteToken(token);
    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    string jwtFilePath = Path.Combine(desktopPath, "JWT.txt");
    File.WriteAllText(jwtFilePath, jwtToken);

    Console.WriteLine("JWT токен успешно создан и записан в файл JWT.txt на рабочем столе.");
}
catch (Exception ex)
{
    Console.WriteLine($"Произошла ошибка: {ex.Message}");
}