using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ValorantStreamOverlay
{


    class LogicHandler
    {
        public static string AccessToken = "JJQ8rTaTXlb5Cwi3Hio9tIXemADCjZhCLvnYt4Tj0g";
        public static string EntitlementToken { get; set; }
        public static string UserID { get; set; }

        public static string riotname, riottag, region;
        public static int refreshTimeinSeconds;
        public Timer relogTimer, pointTimer;

        public static ValorantOverStream ValorantOver;
        public LogicHandler logic;
        public RankDetection rankDetect;

        //Twitch Bot Variables
        public static int currentRankPoints, currentMMRorELO;
        private bool botEnabled;

        public LogicHandler(ValorantOverStream instance)
        {
            logic = this;
            ValorantOver = instance;

            Trace.Write("Reading Settings");
            ReadSettings();
        }

         void ReadSettings()
        {

            if (string.IsNullOrEmpty(Properties.Settings.Default.password) || string.IsNullOrEmpty(Properties.Settings.Default.username))
                MessageBox.Show("Welcome, You have to set your username and password in the settings menu");
            else
            {
                riotname = Properties.Settings.Default.username;
                riottag = Properties.Settings.Default.password;
                region = new SettingsParser().ReadRegion(Properties.Settings.Default.region).GetAwaiter().GetResult();
                refreshTimeinSeconds = new SettingsParser().ReadDelay(Properties.Settings.Default.region).GetAwaiter().GetResult();
                new SettingsParser().ReadSkin(Properties.Settings.Default.skin).GetAwaiter();
                botEnabled = new SettingsParser().ReadTwitchBot().GetAwaiter().GetResult();

                //RiotGamesLogin();

                UpdateToLatestGames();
                new RankDetection();

                StartPointRefresh();
                StartRELOGTimer();
                StartTwitchBot();
            }

        }


        /*void RiotGamesLogin()
        {
            try
            {
                RestClient client = new RestClient("https://auth.riotgames.com/api/v1");
                client.Options.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli;
                client.Options.CookieContainer = new CookieContainer();
                //Cookie cookie = new Cookie();
                //cookie.Name = "__cf_bm";
                //cookie.Value =
                   // "Y8uyu4ZbvLeOjyuhgUqAj5TZi5ycABrULjzGPx91ep4-1657026438-0-AT5KLhCXsAytHtw/0OLmtHz33njLOZXmliB+d7hB46bNKLr9DujptX6x4taRZyVdFjwfZmdKGhhWNmIkVr/w00A=";
                //cookie.HttpOnly = true;
                //cookie.Domain = "riotgames.com";
                //client.CookieContainer.Add(cookie);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
                Authentication.GetAuthorization(client);

                var authJson = JsonConvert.DeserializeObject(Authentication.Authenticate(client, username, password));
                JToken authObj = JObject.FromObject(authJson);
                Console.WriteLine(1);
                if (authObj.ToString().Contains("error"))
                {
                    // error time lmfao
                    MessageBox.Show("Login Failed! Response from server was " + authObj);
                }
                else
                {
                    Console.WriteLine(2);
                    string authURL = authObj["response"]["parameters"]["uri"].Value<string>();
                    var access_tokenVar = Regex.Match(authURL, @"access_token=(.+?)&scope=").Groups[1].Value;
                    AccessToken = $"{access_tokenVar}";
                    
                    RestRequest request = new RestRequest("https://entitlements.auth.riotgames.com/api/token/v1", Method.Post);

                    request.AddHeader("Authorization", $"Bearer {AccessToken}");
                    request.AddJsonBody("{}");

                    string response = client.Execute(request).Content;
                    var entitlement_token = JsonConvert.DeserializeObject(response);
                    JToken entitlement_tokenObj = JObject.FromObject(entitlement_token);

                    EntitlementToken = entitlement_tokenObj["entitlements_token"].Value<string>();

                    
                    RestRequest userid_request = new RestRequest("https://auth.riotgames.com/userinfo", Method.Post);

                    userid_request.AddHeader("Authorization", $"Bearer {AccessToken}");
                    userid_request.AddJsonBody("{}");

                    string userid_response = client.Execute(userid_request).Content;
                    dynamic userid = JsonConvert.DeserializeObject(userid_response);
                    JToken useridObj = JObject.FromObject(userid);

                    //Console.WriteLine(userid_response);

                    UserID = useridObj["sub"].Value<string>();
                }


                

            }
            catch (Exception e)
            {
                MessageBox.Show("Login Failed! Threw exception " + e.Message + " from class " + e.Source);
                Console.WriteLine(e.StackTrace);
            }
        }
*/


        async Task UpdateToLatestGames()
        {
            
            Trace.Write("UPDATING");
            dynamic response = GetCompApiAsync().GetAwaiter().GetResult();
            if (response != null)
            {
                int lastMatchPoints = response["data"]["mmr_change_to_last_game"];
                int pointsInRank = response["data"]["ranking_in_tier"];
                int rankTier = response["data"]["currenttier"];
                //Send Points to Function that changes the UI
                SetChangesToOverlay(lastMatchPoints, pointsInRank, rankTier).GetAwaiter();
            }
        

        }


        private async Task<JObject> GetCompApiAsync()
        {
            
            RestClient compClient = new RestClient(new Uri("https://api.henrikdev.xyz/valorant/v1/mmr/"));
            RestRequest compRequest = new RestRequest($"{region}/{riotname}/{riottag}");

            


            RestResponse rankedResp = compClient.Get(compRequest);

            return rankedResp.IsSuccessful ? JsonConvert.DeserializeObject<JObject>(rankedResp.Content) : null;
        }


        private async Task SetChangesToOverlay(int lastMatch, int pointsInRank, int rankTier)
        {
            Label[] rankChanges = { ValorantOver.recentGame1, ValorantOver.recentGame2, ValorantOver.recentGame3 };
            
            
            if (lastMatch != int.Parse(rankChanges[0].Text.Replace("+", "").Replace("-", "")))
            {
                rankChanges[2].ForeColor = rankChanges[1].ForeColor;
                rankChanges[2].Text = rankChanges[1].Text;
                rankChanges[1].ForeColor = rankChanges[0].ForeColor;
                rankChanges[1].Text = rankChanges[0].Text;
            }

            if (lastMatch < 0)
            {
                lastMatch *= -1;
                rankChanges[0].ForeColor = Color.Red;
                rankChanges[0].Text = $"-{lastMatch}";
            }
            else
            {
                rankChanges[0].ForeColor = Color.LimeGreen;
                rankChanges[0].Text = $"+{lastMatch}";
            }
            
        }


        private void StartPointRefresh()
        {
            pointTimer = new Timer();
            pointTimer.Tick += new EventHandler(pointTimer_Tick);
            pointTimer.Interval = refreshTimeinSeconds * 1000;
            pointTimer.Start();
        }

        private void pointTimer_Tick(object sender, EventArgs e)
        {
            UpdateToLatestGames().GetAwaiter();

            if (rankDetect == null)
            {
                rankDetect = new RankDetection();
            }
            else
            {
                rankDetect.UpdateRank();
            }
        }

        private void StartRELOGTimer()
        {
            relogTimer = new Timer();
            relogTimer.Tick += new EventHandler(relogTimer_Tick);
            relogTimer.Interval = 2700 * 1000;
            relogTimer.Start();
            
        }

        private void relogTimer_Tick(object sender, EventArgs e)
        {
            pointTimer.Stop();
            UpdateToLatestGames().Wait();
            pointTimer.Start();
        }

        public void StartTwitchBot()
        {
            if (botEnabled)
            {
                Trace.WriteLine("Bot enabled");
                TwitchIntegration bot = new TwitchIntegration();
            }
            else
            {
                Trace.WriteLine("Bot not enabled");
            }
        }
    }
}
