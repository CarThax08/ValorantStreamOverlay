using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using TwitchLib.Api.Core.Models.Undocumented.ChannelExtensionData;

namespace ValorantStreamOverlay
{
    
    class Authentication
    {
        

        public static void GetAuthorization(RestClient client)
        {
            RestRequest request = new RestRequest("authorization", Method.Post);
            string body = "{\"client_id\":\"play-valorant-web-prod\",\"nonce\":\"1\",\"redirect_uri\":\"https://playvalorant.com/opt_in" + "\",\"response_type\":\"token id_token\",\"scope\":\"account openid\"}";
            request.AddStringBody(body, DataFormat.Json);
            client.Options.UserAgent = "ShooterGame/13 Windows/10.0.19043.1.256.64bit";
            request.AddHeader("X-Riot-ClientPlatform",
                "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
            request.AddHeader("X-Riot-ClientVersion", "1.0");
            RestResponse res = client.Execute(request);
            Console.WriteLine(res.StatusCode);
        }

        public static string Authenticate(RestClient client, string user, string pass)
        {

            
            

            RestRequest request = new RestRequest("authorization", Method.Put);
            var auth = new {
                type = "auth",
                username = user,
                password = pass
            };
            string body = JsonConvert.SerializeObject(auth);
            request.AddJsonBody(body);

            //client.Options.UserAgent =
               // "RiotClient/51.0.0.4429735.4381201 rso-auth (Windows;10;;Professional, x64)";

            RestResponse response = client.Execute(request);
            
            Console.WriteLine(response.Content);
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.StatusDescription);
            Console.WriteLine(response.ErrorException);

            return response.Content;
        }
        public static string GenerateRandomAlphanumericString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
 
            var random       = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }
    }
    
}
