using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.LogConverter
{
    public static class GobchatContext
    {
        public static string ResourceLocation
        {
            get { return System.IO.Path.Combine(ApplicationLocation, @"resources"); }
        }

        public static string UserDataLocation
        {
            get { return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gobchat"); }
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

        public static Version InnerApplicationVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
        }
    }
}