using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace IdentityByCertificate.Controllers
{
    [Route("API/[Controller]/[Action]")]
    public class Register : Controller
    {

        private readonly ConfigurationDbContext _configContext;
        private readonly PersistedGrantDbContext _grandContext; // Пока что не нашел применения

        public Register(ConfigurationDbContext configContext, PersistedGrantDbContext persistedGrantDb)
        {
            _configContext = configContext;
            _grandContext = persistedGrantDb;
        }


        [HttpPost]
        public IActionResult Certificate([FromHeader] string clientId, [FromHeader] string x509Certificate) // Не уверен как лучше передавать сертификаты
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return BadRequest("Идентификатор пользователя не может быть пустым.");
            }
            if (string.IsNullOrWhiteSpace(x509Certificate))
            {
                return BadRequest("Сертификат не может быть пустым.");
            }


            X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(x509Certificate));


            if (ValidateCertificate(certificate))
            {

                var existingClient = _configContext.Clients.FirstOrDefault(x => x.ClientId == clientId);
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
                        AllowedScopes = { "CapibarAPI" }
                    };

                    _configContext.Clients.Add(client.ToEntity());
                    _configContext.SaveChanges();


                    return Ok($"Создан новый пользователь");

                }
                else
                { // Проблема в том что сейчас при обновлении пароля пользователя - он просто пересоздается. Т.е. меняется первичный ключ id, могут слететь ссылки.
                  // Но на данный момент это наиболее простой вариант. Доступа к секретам при текущей конфигурации - нет
                    _configContext.Clients.Remove(existingClient);
                    var client = new Client()
                    {
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientId = clientId,
                        ClientSecrets =
                    {
                        new Secret(certificate.Thumbprint.Sha256())
                    },
                        AllowedScopes = { "CapibarAPI" }
                    };


                    _configContext.Clients.Update(client.ToEntity());
                    _configContext.SaveChanges();


                    // Сохранить изменения в базе данных
                    _configContext.SaveChanges();

                    return Ok($"Сертификат обновлен");

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
