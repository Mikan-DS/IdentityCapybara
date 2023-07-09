using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������� � ���������.
builder.Services.AddControllers();
// ��������� ��������� Swagger/OpenAPI. 
// ��������� � ��������� Swagger/OpenAPI ����� ������ �� ������ https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// ����������� �������������� � �������������� ����� "Bearer" � JWT-�������.
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration.GetValue<string>("IdentitiServerUrl"); // ��������� URL ������������� ������� (Identity Provider)

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false // ��������� �������� ��������� ������
        };
    });

// ����������� �����������.
builder.Services.AddAuthorization(options =>
{
    // ��������� �������� "CapibarsScope", ������� ������� �������������������� ������������ 
    // � ������� � ���� ����������� "Scope" �� ��������� "CapibarAPI".
    options.AddPolicy("CapibarsScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("Scope", "CapibarAPI");
    });
});


var app = builder.Build();

// ��������������� � HTTP �� HTTPS.
app.UseHttpsRedirection();

// ��������� �������������� � ����������� � ����������.
app.UseAuthentication();
app.UseAuthorization();

// ����������� ������������� ��� ������������.
app.MapControllers();

// ��������� ���������� ��� ��������� ���� ���������� ("/"), ������� ���������� ������ "I love carburator".
app.MapGet("/", () => "I love carburator");

// ��������� ����������.
app.Run();

