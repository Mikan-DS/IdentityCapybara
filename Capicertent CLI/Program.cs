

using System.Security.Cryptography.X509Certificates;



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
        .AddJsonFile(Path.GetFullPath("..\\..\\..\\appsettings.json")) //TODO изменить путь если есть необходимость
        .Build();

        private static X509Certificate2 certificate = new X509Certificate2("..\\..\\..\\CERT\\IdentityServer4_certificate.cer", "P@55W0RD");


        private static HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMinutes(1) };
        private static string _token = string.Empty;


        public static string clientId = "CLIClient";


        static void Main(string[] args)
        {

            Thread.Sleep(5000); // Чтобы другие сервисы успели включиться

            CapibaraMain().Wait();

            Console.ReadLine();
        }

        /// <summary>
        /// Метод для регистрации или обновления сертификата на сервере.
        /// </summary>
        /// <param name="certificate">Сертификат <see cref="X509Certificate2"/> для регистрации или обновления.</param>
        static async void RegisterOrUpdateCertificate(X509Certificate2 certificate)
        {

            await Console.Out.WriteLineAsync("[?] Попытка зарегистрировать сертификат...");


            // Получаем URL из конфигурации (ваш код получения конфигурации)
            string apiUrl = configuration.GetValue<string>("IdentitiServerUrl") + "/API/Register/Certificate";

            // Создаем объект HttpRequestMessage для отправки запроса
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUrl);


            // Добавляем заголовок (header) к запросу

            request.Headers.Add("ClientId", clientId);
            request.Headers.Add("x509Certificate", Convert.ToBase64String(certificate.RawData));


            try
            {
                var response = client.Send(request);
                if (!response.IsSuccessStatusCode)
                {
                    await Console.Out.WriteLineAsync($"[-] Произошла ошибка: код ошибки {response.StatusCode}");
                }
                else
                {
                    await Console.Out.WriteLineAsync($"[+] Удачно! {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                await Console.Out.WriteLineAsync($"[-] Ошибка подключения! {ex.Message}");
                throw ex;
            }

        }

        /// <summary>
        /// Метод для установки токена.
        /// </summary>
        /// <remarks>
        /// Этот метод получает информацию о конечной точке <see cref="DiscoveryDocumentResponse"/> и токене, 
        /// используя заданный клиент. Полученный токен сохраняется в поле _token.
        /// </remarks>
        static async Task SetToken()
        {
            await Console.Out.WriteLineAsync("[+] Получаем информацию о конечной точке");
            var disco = await GetDiscovery(client);

            await Console.Out.WriteLineAsync("[+] Получаем токен");
            _token = await GetToken(client, disco);
        }


        /// <summary>
        /// Главный метод для выполнения операций с Capibara.
        /// </summary>
        static async Task CapibaraMain()
        {
            await Console.Out.WriteLineAsync("[!] Запуск скрипта для подключения к CapybaraAPI");
            try
            {
                RegisterOrUpdateCertificate(certificate); // Предположим что у нас есть новый сертификат, для дальнейшей работы мы должны его зарегистрировать на сервере

                await SetToken(); // Получаем токен

                await Console.Out.WriteLineAsync($"[+] Теперь у нас есть все необходимые данные. \n Отпечаток: {certificate.Thumbprint}\n Токен: {_token}");

                await Console.Out.WriteLineAsync("[?] Попытка воспользоваться сервисом API");

                await CallApi(client, _token);


            }
            catch (Exception)
            {
                await Console.Out.WriteLineAsync("[-] Скрипт неудался");
            }
            await Console.Out.WriteLineAsync("[!] Завершение работы...");
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

                ClientId = clientId,
                ClientSecret = certificate.Thumbprint,
                Scope = "CapibarAPI",
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
