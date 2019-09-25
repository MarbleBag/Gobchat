using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat
{
    // This class can determine the current plugin version, as well as the latest version
    // released of the plugin on GitHub. 
    // This version is currently identical to quisquous work in cactbot
    // https://github.com/quisquous/cactbot/blob/master/CactbotOverlay/VersionChecker.cs
    public class VersionFetcher
    {
        public const string RELEASE_API_ENDPOINT_URL = @"https://api.github.com/repos/marblebag/gobchat/releases/latest";
        public const string RELEASE_URL = "https://github.com/marblebag/gobchat/releases/latest";
        public const string ISSUE_URL = "https://github.com/marblebag/gobchat/issues";

        private Logger logger;

        public VersionFetcher(Logger logger)
        {
            this.logger = logger;
        }

        public Version GetLocalVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        public string GetLocation()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public Version GetOverlayPluginVersion()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(RainbowMage.OverlayPlugin.IOverlay)).GetName().Version;
        }

        public string GetOverlayPluginLocation()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(RainbowMage.OverlayPlugin.IOverlay)).Location;
        }

        public Version GetFFXIVPluginVersion()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)).GetName().Version;
        }

        public string GetFFXIVPluginLocation()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)).Location;
        }

        public Version GetACTVersion()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(Advanced_Combat_Tracker.ActGlobals)).GetName().Version;
        }

        public string GetACTLocation()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(Advanced_Combat_Tracker.ActGlobals)).Location;
        }

        public Version GetRemoteVersion()
        {
            try
            {
                string json;
                using (var web_client = new System.Net.WebClient())
                {
                    // https://developer.github.com/v3/#user-agent-required
                    web_client.Headers.Add("User-Agent", "Gobchat");
                    using (var reader = new System.IO.StreamReader(web_client.OpenRead(RELEASE_API_ENDPOINT_URL)))
                    {
                        json = reader.ReadToEnd();
                    }
                }
                dynamic latest_release = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(json);

                return new Version(latest_release["tag_name"].Replace("v", ""));
            }
            catch (Exception e)
            {
                logger.LogError("Error fetching most recent github release: " + e.Message + "\n" + e.StackTrace);
                return new Version();
            }
        }
    }
}
