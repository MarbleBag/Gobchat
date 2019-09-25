namespace Gobchat
{
    public class ACTLogLineUtil
    {
        public static string ExtractNextFragmentFromLog(string delimiter, ReaderIndex index, string logLine)
        {
            int marker = logLine.IndexOf(delimiter, index.Value);
            if (marker < 0)
            {
                return null;
            }
            string fragment = logLine.Substring(index.Value, marker - index.Value);
            index.Value = marker + delimiter.Length;
            return fragment;
        }

        public static string ExtractNextFragmentFromLog(string delimiter, ReaderIndex index, string logLine, int count)
        {
            int marker = logLine.IndexOf(delimiter, index.Value, count);
            if (marker < 0)
            {
                return null;
            }
            string fragment = logLine.Substring(index.Value, marker - index.Value);
            index.Value = marker + delimiter.Length;
            return fragment;
        }
    }
}
