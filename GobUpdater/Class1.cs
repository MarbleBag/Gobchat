using System.Diagnostics;
using System.Reflection;

namespace GobUpdater
{
    public sealed class Manager
    {
        public static Manager Instance { get; } = new Manager();

        public Manager() {
            Debug.WriteLine(Assembly.GetExecutingAssembly().FullName);
            Debug.WriteLine(Assembly.GetCallingAssembly().FullName);
            Debug.WriteLine(Assembly.GetEntryAssembly()?.FullName);
            Debug.WriteLine(Assembly.GetAssembly(typeof(Manager))?.FullName);
        }

        public void CheckForUpdates()
        {

        }

        public void IsUpdatePending()
        {

        }

        public void Update()
        {

        }


    }
}