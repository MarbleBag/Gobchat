namespace Gobchat
{
    public class MainEntry
    {
     
        static void Main(string[] args)
        {
            using (var app = new Core.App())
            {
                app.Run();
            }
        }
    }
}
