using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using static IdentityModel.OidcConstants;

namespace Caplient_CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {


            CapibaraMain().Wait();

            Console.WriteLine("FINISH");
            Console.ReadLine();


        }

        static async Task CapibaraMain()
        {
            Console.WriteLine("Send request");
            var client = new HttpClient();

            var disco = await GetDiscovery(client);
            Console.WriteLine(disco.UserInfoEndpoint);
            string token = await GetToken(client, disco);
            //string token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjI4NTg3RTAwQzI2REIwQTEyRDAxQkNBM0M3NDkxNTI0IiwidHlwIjoiYXQrand0In0.eyJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo1MDAxIiwibmJmIjoxNjg4NzI3OTE0LCJpYXQiOjE2ODg3Mjc5MTQsImV4cCI6MTY4ODczMTUxNCwic2NvcGUiOlsiQ2FwaWJhckFQSSJdLCJjbGllbnRfaWQiOiJjbGllbnQiLCJqdGkiOiJGNDE4RUIyMzQwQTVDQzEyQjM0MjlBQjdDOEU2RTcxNiJ9.n8AHi_5NhFmTwarvUyhxtJREY_uNxJesZnX_o66_sRtVODhPwFr3rb6DK9_ZPggbjEELHvcw7ginRg3FTmCk3ey-3YorUbZgBNRQb-KOBF7uxT6iHPaOQVBIlLWaEPGYA2BfgC9zjyTB_9bNp9HDTNECb1gftv9MVFRTO3PY64y7WAdy70iKRwOz399cSXoH7xEHzd9fbUyk2c4tO7pa5IK7kc5N-Rj30g9Ru_8o2Ros4xKIjo7tzrjXbLy9H5tHAZPmnufUL9DaDeQT0Voam4BiOfrIrjclICkiCfjMxPbrW0J5sXdvBJ0GVimVa_yrZEoeZ4f7uqNu1suNRFR-Ag";

            Console.WriteLine("TOKEN IS: "+token);

            await CallApi(client, token);

        }

        static async Task<DiscoveryDocumentResponse> GetDiscovery(HttpClient client)
        {
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                throw new Exception(disco.Error);
            }
            return disco;
        }

        static async Task<string> GetToken(HttpClient client, DiscoveryDocumentResponse discovery)
        {
            // request token
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
            Console.WriteLine("Refresh token is"+tokenResponse.RefreshToken);
            return tokenResponse.AccessToken;
        }


        static async Task CallApi(HttpClient client, string token)
        {


            // call api
            client.SetBearerToken(token);

            var response = await client.GetAsync("https://localhost:7162/Capibaras");
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
