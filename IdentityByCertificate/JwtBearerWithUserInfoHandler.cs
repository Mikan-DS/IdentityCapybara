using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityByCertificate
{

    public class JwtBearerWithUserInfoHandler : AuthenticationHandler<JwtBearerOptions>
    {
        public JwtBearerWithUserInfoHandler(
            IOptionsMonitor<JwtBearerOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
            ) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            try
            {
                if (Request.Headers["Authorization"].IsNullOrEmpty())
                {
                    return AuthenticateResult.NoResult();
                }


                var token = TryGetBearerFromHeader(Request, out var tokenValue) ? tokenValue : null;

                if (string.IsNullOrEmpty(token))
                {
                    return AuthenticateResult.NoResult();
                }



                var tokenHandler = new JwtSecurityTokenHandler();
                X509Certificate2 certificate;

                try
                {
                   certificate = new X509Certificate2(Convert.FromBase64String(((JwtSecurityToken)tokenHandler.ReadToken(token)).Claims.ElementAt(1).Value));
                }
                catch (Exception)
                {

                    return AuthenticateResult.Fail("Неверный формат сертификата"); // ну или если совсем нет
                }


                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    //ValidIssuer = Options.TokenValidationParameters.ValidIssuer,
                    ValidateAudience = false,
                    //ValidAudience = Options.TokenValidationParameters.ValidAudience,
                    ValidateIssuerSigningKey = false,
                    //IssuerSigningKey = Options.TokenValidationParameters.IssuerSigningKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new X509SecurityKey(certificate)
                };


                ClaimsPrincipal claimsPrincipal;
                try
                {
                    claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                }
                catch (Exception)
                {
                    return AuthenticateResult.Fail("Валидация токена не удалась.");
                }

                var username = claimsPrincipal.FindFirstValue("username");

                if (string.IsNullOrEmpty(username))
                {
                    return AuthenticateResult.Fail("В токене не найдено имя пользователя.");
                }

                var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

                Request.HttpContext.Items["username"] = username;
                Request.HttpContext.Items["x509_certificate"] = certificate;

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"Произошла ошибка: {ex.Message}");
            }
        }

        private static bool TryGetBearerFromHeader(HttpRequest request, out string tokenValue)
        {
            tokenValue = null;

            if (request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    tokenValue = authHeader.Substring("Bearer ".Length).Trim();
                    return true;
                }
            }

            return false;
        }
    }
}
