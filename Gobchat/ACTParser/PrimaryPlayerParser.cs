using System;

namespace Gobchat
{
    public class PrimaryPlayerParser : ACTLogLineHandler
    {
        private static readonly string Phrase = "Changed primary player to";

        public delegate void OnParse(string playername);

        private readonly OnParse onParse;



        public PrimaryPlayerParser(OnParse onParse)
        {
            this.onParse = onParse ?? throw new ArgumentNullException(nameof(onParse));
        }

        public void Handle(ReaderIndex index, ACTLogLine entry)
        {
            int mark = entry.Message.IndexOf(Phrase);
            if (mark < 0) return;
            int startIndex = mark + Phrase.Length;
            int endIndex = entry.Message.Length - 1; //ends on a point .
            string playername = entry.Message.Substring(startIndex, endIndex-startIndex).Trim();
            onParse.Invoke(playername);
        }
    }
}
