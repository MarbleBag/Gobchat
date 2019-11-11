using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Updater
{
    // This class can determine the current plugin version, as well as the latest version
    // released of the plugin on GitHub.
    // This version is currently identical to quisquous work in cactbot
    // https://github.com/quisquous/cactbot/blob/master/CactbotOverlay/VersionChecker.cs
    internal class VersionChecker
    {
        public const string RELEASE_API_ENDPOINT_URL = @"https://api.github.com/repos/marblebag/gobchat/releases/latest";
        public const string RELEASE_URL = "https://github.com/marblebag/gobchat/releases/latest";
        public const string ISSUE_URL = "https://github.com/marblebag/gobchat/issues";
    }
}