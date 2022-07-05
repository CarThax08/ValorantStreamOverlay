using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ValorantStreamOverlay
{
    class RankDetection
    {
        public static dynamic rankJson;
        public static int currentRP;
        public static string rankName;
        public RankDetection()
        {
            //Start Update
            GetCloudRankJSON().GetAwaiter().GetResult();
            int rankNum = UPDATECompRankAsync().GetAwaiter().GetResult();
            RankParser(rankNum);
            
        }

        public void UpdateRank()
        {
            int rankNum = UPDATECompRankAsync().GetAwaiter().GetResult();
            RankParser(rankNum);
        }

        private async Task<int> UPDATECompRankAsync()
        {
            try
            {
                RestClient compRank = new RestClient(new Uri(
                    $"https://api.henrikdev.xyz/valorant/v1/mmr/"));
                RestRequest compRequest = new RestRequest($"{LogicHandler.region}/{LogicHandler.riotname}/{LogicHandler.riottag}");
                

                RestResponse rankedResp = compRank.Get(compRequest);

                Trace.WriteLine(rankedResp.Content);
                if (rankedResp.IsSuccessful)
                {
                    dynamic jsonconvert = JsonConvert.DeserializeObject<JObject>(rankedResp.Content);

                    int currentRank = jsonconvert["data"]["currenttier"];
                    
                    currentRP = jsonconvert["data"]["ranking_in_tier"];
                    return currentRank;
                }

                return 0;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Retrieving UPDATECompRankAsync function");
                Console.WriteLine(e.StackTrace);
                return 0;
            }

        }

        private async Task GetCloudRankJSON()
        {
            RestClient cloudRankJson = new RestClient(new Uri("https://raw.githubusercontent.com/CarThax08/ValorantStreamOverlay/master/Resources/RankInfo.json"));
            RestRequest rankRequest = new RestRequest("https://raw.githubusercontent.com/CarThax08/ValorantStreamOverlay/master/Resources/RankInfo.json", Method.Get);
            RestResponse rankResp = cloudRankJson.Get(rankRequest);
            rankJson = (rankResp.IsSuccessful) ? rankJson = rankResp.Content : rankJson = string.Empty;
        }
        void RankParser(int rankNumber)
        {
            //Getting Errors when trying to pull the Json Data.
            
            var cloudJsonDeserial = JsonConvert.DeserializeObject(rankJson);
            JToken cloudJson = JToken.FromObject(cloudJsonDeserial);
            string rankNameLower = cloudJson["Ranks"][rankNumber.ToString()].Value<string>();
            rankName = rankNameLower.ToUpper();


            Trace.Write("Setting Rank To Valid Rank Num");

            LogicHandler.ValorantOver.rankingLabel.Text = rankName;

            var resource = Properties.Resources.ResourceManager.GetObject("TX_CompetitiveTier_Large_" + rankNumber);
            Bitmap myImage = (Bitmap)resource;
            LogicHandler.ValorantOver.rankIconBox.Image = myImage;

            LogicHandler.ValorantOver.rankPointsElo.Text =
                $"{currentRP} RR | {(rankNumber * 100) - 300 + currentRP} TRR";

            LogicHandler.currentMMRorELO = (rankNumber * 100) - 300 + currentRP;
            LogicHandler.currentRankPoints = currentRP;
        }

       
    }
}
