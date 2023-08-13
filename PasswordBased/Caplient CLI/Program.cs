using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Caplient_CLI
{
    internal class Program
    {
        private static IConfiguration configuration = new ConfigurationBuilder()
        .AddJsonFile(Path.GetFullPath("..\\..\\appsettings.json")) //TODO изменить путь если есть необходимость
        .Build();

        static void Main(string[] args)
        {


            CapibaraMain().Wait();

            Console.WriteLine("FINISH");
            Console.ReadLine();
        }

        /// <summary>
        /// Главный метод для выполнения операций с Capibara.
        /// </summary>
        static async Task CapibaraMain()
        {
            Console.WriteLine("Send request");
            var client = new HttpClient();

            var disco = await GetDiscovery(client);
            Console.WriteLine(disco.UserInfoEndpoint);
            string token = await GetToken(client, disco);

            Console.WriteLine("TOKEN IS: " + token);

            await CallApi(client, token);
        }

        /// <summary>
        /// Получение документа открытия (discovery document) для IdentityServer.
        /// </summary>
        /// <param name="client">HttpClient для отправки запросов.</param>
        /// <returns>Ответ от документа открытия (discovery document).</returns>
        static async Task<DiscoveryDocumentResponse> GetDiscovery(HttpClient client)
        {
            var disco = await client.GetDiscoveryDocumentAsync(configuration.GetValue<string>("IdentitiServerUrl"));
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                throw new Exception(disco.Error);
            }
            return disco;
        }

        /// <summary>
        /// Получение токена доступа от IdentityServer.
        /// </summary>
        /// <param name="client">HttpClient для отправки запросов.</param>
        /// <param name="discovery">Документ открытия (discovery document) для IdentityServer.</param>
        /// <returns>Токен доступа.</returns>
        static async Task<string> GetToken(HttpClient client, DiscoveryDocumentResponse discovery)
        {
            // Запрос токена
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discovery.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "AdminAPI CapibarAPI",
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                throw new Exception(tokenResponse.Error);
            }
            return tokenResponse.AccessToken;
        }

        /// <summary>
        /// Вызов API с использованием токена доступа.
        /// </summary>
        /// <param name="client">HttpClient для отправки запросов.</param>
        /// <param name="token">Токен доступа.</param>
        static async Task CallApi(HttpClient client, string token)
        {
            // Вызов API
            client.SetBearerToken(token);

            var response = await client.GetAsync(configuration.GetValue<string>("ApiServerUrl") + "/Capibaras");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
