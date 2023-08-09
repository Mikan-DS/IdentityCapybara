using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace IdentityByCertificate
{
    public static class JwtTokenHandlerExtensions
    {
        public static void AddJwtTokenHandler(this IServiceCollection services)
        {
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    //// Устанавливаем URL-адрес, используемый в качестве Authority для проверки токенов
                    //options.Authority = configuration["ASPNETCORE_URLS"]; // Используется текущий сервер

                    //options.TokenValidationParameters = new TokenValidationParameters
                    //{
                    //    ValidateAudience = false,
                    //    IssuerSigningKeyResolver = (token, securityToken, keyIdentifier, parameters) =>
                    //    {
                    //        // Разбираем JWT токен
                    //        var handler = new JwtSecurityTokenHandler();
                    //        var jwtToken = handler.ReadJwtToken(token);

                    //        // Получаем сертификат из токена
                    //        var certificateString = jwtToken.Header["x5c"]?.ToString();
                    //        if (string.IsNullOrEmpty(certificateString))
                    //        {
                    //            return null;
                    //        }

                    //        var certBytes = Convert.FromBase64String(certificateString);
                    //        var certificate = new X509Certificate2(certBytes);

                    //        //ИИ Добавить сертификат в HttpContext.Items
                    //        parameters.

                    //        return new[] { new X509SecurityKey(certificate) };
                    //    },
                    //    ValidateIssuerSigningKey = true,
                    //    ValidateIssuer = true,
                    //    ValidateLifetime = true,
                    //    ClockSkew = TimeSpan.Zero
                    //};

                    // Устанавливаем URL-адрес, используемый в качестве Authority для проверки токенов
                    //options.Authority = configuration["ASPNETCORE_URLS"]; // Используется текущий сервер

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            // Разбираем JWT токен
                            var handler = new JwtSecurityTokenHandler();
                            //var jwtToken = handler.ReadJwtToken(context.SecurityToken.RawData);

                            if (context.SecurityToken is JwtSecurityToken jwtToken)
                            {

                                // Получаем сертификат из токена
                                var certificateString = jwtToken.Header["x5c"]?.ToString();
                                if (!string.IsNullOrEmpty(certificateString))
                                {
                                    var certBytes = Convert.FromBase64String(certificateString);
                                    var certificate = new X509Certificate2(certBytes);

                                    // Добавляем сертификат в HttpContext.Items через HttpContextAccessor
                                    var httpContextAccessor = context.HttpContext.RequestServices.GetRequiredService<IHttpContextAccessor>();
                                    httpContextAccessor.HttpContext.Items["X509Certificate"] = certificate;
                                }
                            }
                        }
                    };

                    //options.TokenValidationParameters = new TokenValidationParameters
                    //{
                    //    //ValidateAudience = false,
                    //    //ValidateIssuerSigningKey = true,
                    //    //ValidateIssuer = true,
                    //    //ValidateLifetime = true,
                    //    //ClockSkew = TimeSpan.Zero,

                    //    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)),
                    //    //comment this and add this line to fool the validation logic
                    //    SignatureValidator = delegate (string token, TokenValidationParameters parameters)
                    //    {
                    //        var jwt = new JwtSecurityToken(token);
                    //        return jwt;
                    //    },
                    //};


                });
        }
    }
}
