using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityByCertificate.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace IdentityByCertificate.Controllers
{
    [Route("API/[Controller]/[Action]")]
    public class Register : Controller
    {

        private readonly ConfigurationDbContext _configContext;
        private readonly ApplicationDbContext _applicationDbContext;


        public Register(ConfigurationDbContext configContext, ApplicationDbContext applicationDbContext)
        {
            _configContext = configContext;
            _applicationDbContext = applicationDbContext;

        }


        [HttpPost]
        public IActionResult Certificate()
        {

            //this.HttpContext.i

            string? clientId = this.HttpContext.Items["ClientId"] as string;
            X509Certificate2? certificate = this.HttpContext.Items["x509_certificate"] as X509Certificate2;



            if (string.IsNullOrWhiteSpace(clientId)) // Возможно в этом уже нет необходимости
            {
                return BadRequest("Идентификатор пользователя не может быть пустым.");
            }
            if (certificate == null)
            {
                return BadRequest("Сертификат не может быть пустым.");
            }



            if (ValidateCertificate(certificate)) // Для доп. валидации сертификата
            {

                var existingClient = _configContext.Clients.Include(c => c.ClientSecrets).FirstOrDefault(x => x.ClientId == clientId);
                if (existingClient == null)
                {
                    var client = new Client()
                    {
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientId = clientId,
                        ClientSecrets =
                    {
                        new Secret(certificate.Thumbprint.Sha256())
                    },
                        AllowedScopes = { "CapibarAPI" },
                    };

                    _configContext.Clients.Add(client.ToEntity());
                    _configContext.SaveChanges();


                    return Ok($"Создан новый пользователь");

                }
                else
                {

                    existingClient.ClientSecrets.First().Value = certificate.Thumbprint.Sha256();

                    _configContext.SaveChanges();

                    return Ok($"Сертификат обновлен: " + certificate.Thumbprint);

                }

            }
            else
            {
                return Unauthorized();
            }




            // Вернуть ответ клиенту
            //return Ok($"User '{client}' registered successfully with certificate '{x509Certificate}' and salt '{salt}'.");
        }

        private bool ValidateCertificate(X509Certificate2 certificate)
        {
            return true;
        }

        //[HttpPost]
        //public IActionResult Client([FromHeader] string client, [FromHeader] string x509Certificate)
        //{
        //    // Обработка полученных данных, например, сохранение в базу данных или другие действия.
        //    // Важно помнить, что передавать сертификаты в теле POST запроса может быть не самым безопасным решением в зависимости от контекста.

        //    X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(x509Certificate));



        //    // Вернуть ответ клиенту
        //    //return Ok($"User '{client}' registered successfully with certificate '{x509Certificate}' and salt '{salt}'.");
        //    return Ok($"{certificate.IssuerName}");
        //}


    }
}
