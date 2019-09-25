using System;
using System.Collections.Generic;
using System.Globalization;

namespace Gobchat
{
    public class ReaderIndex
    {
        public int Value { get; set; }
    }

    public class ACTLogLineDelegator
    {
        public delegate void UnhandledLogLine(int? code, ReaderIndex index, ACTLogLine entry);

        private Dictionary<int, ACTLogLineHandler> handlesByCode = new Dictionary<int, ACTLogLineHandler>();

        private readonly Logger logger;
        private readonly UnhandledLogLine unhandledEntry;

        public ACTLogLineDelegator(Logger logger, UnhandledLogLine unhandledEntry = null)
        {
            this.logger = logger;
            this.unhandledEntry = unhandledEntry;
        }

        public void ProcessRecord(ACTLogLine record)
        {
            var index = new ReaderIndex();
            object something = ExtractTimeStamp(index, record);
            int? code = ExtractLogLineCode(index, record);

            if (code.HasValue)
            {
                var handle = GetHandle(code.GetValueOrDefault(-1));
                if (handle != null)
                {
                    try
                    {
                        handle.Handle(index, record);
                    }catch(ACTParseException e)
                    {
                        logger.LogWarning($"Exception in LogLine type {code}: {e}");
                    }
                    return;
                }
            }

            unhandledEntry?.Invoke(code, index, record);
        }

        private static readonly int TimeStampLength = "[HH:MM:SS.sss]".Length;

        private object ExtractTimeStamp(ReaderIndex index, ACTLogLine entry)
        {
            //the timestamp comes in the format of '[HH:MM:SS.sss]'
            var timestamp = entry.Message.Substring(0, TimeStampLength);
            index.Value += TimeStampLength;
            return null; //not used yet
        }

        private int? ExtractLogLineCode(ReaderIndex index, ACTLogLine entry)
        {
            string fragment = ACTLogLineUtil.ExtractNextFragmentFromLog(":", index, entry.Message);
            if (fragment == null)
                return null; //some lines don't have a code, which is worth a bug imho

            if (Int32.TryParse(fragment, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int result))
                return result;
            return null; //normally, this is worth an exception, but for now, it can't be handled in a meaningful way anyroads
        }

        private ACTLogLineHandler GetHandle(int eventCode)
        {
            if (handlesByCode.TryGetValue(eventCode, out ACTLogLineHandler parser))
                return parser;
            return null;
        }

        public void SetHandle(int eventCode, ACTLogLineHandler handle)
        {
            if (handle == null)
                throw new ArgumentNullException("handle");
            handlesByCode.Add(eventCode, handle);
        }
    }
}
