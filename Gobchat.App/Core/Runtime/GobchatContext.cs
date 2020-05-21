using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Core.Runtime
{
    public static class GobchatContext
    {
        public static string ResourceLocation
        {
            get { return System.IO.Path.Combine(ApplicationLocation, @"resources"); }
        }

        public static string UserDataLocation
        {
#if DEBUG
            get { return System.IO.Path.Combine(ApplicationLocation, "DebugConfig"); }
#else
            get { return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gobchat"); }
#endif
        }

        public static string UserLogLocation
        {
            get { return System.IO.Path.Combine(UserDataLocation, "log"); }
        }

        public static string UserConfigLocation
        {
            get { return System.IO.Path.Combine(UserDataLocation, "config"); }
        }

        public static string ApplicationLocation
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static GobVersion ApplicationVersion
        {
            get { return new GobVersion(InnerApplicationVersion); }
        }

        private static Version InnerApplicationVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
        }
    }
}