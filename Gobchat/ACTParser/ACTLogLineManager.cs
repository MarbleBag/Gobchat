using System;
using System.Collections.Generic;

namespace Gobchat
{
    public class PositionUpdateParser : ACTLogLineHandler
    {
        public delegate void OnMessage(string message);

        private readonly Logger logger;
        private readonly OnMessage dele;

        public PositionUpdateParser(Logger logger, OnMessage dele)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dele = dele ?? throw new ArgumentNullException(nameof(dele));
        }

        public void Handle(ReaderIndex index, ACTLogLine entry)
        {
            //[01:44:08.757] 27:10736CCB:Senshir Vairemont:93837:93837:10000:10000:0:0:1.728958:0.6658493:0.02892041:1.496085:
            //[01:44:32.744] 27:10736CCB:Senshir Vairemont:93837:93837:10000:10000:0:0:3.128052:-2.426208:0.02875674:-0.3738658:
            //[01:44:23.696] 27:10736CCB:Senshir Vairemont:93837:93837:10000:10000:0:0:-0.1042926:-0.2504024:-0.01531982:-2.525041:
            //[01:44:26.697] 27:10736CCB:Senshir Vairemont:93837:93837:10000:10000:0:0:2.792297:-1.510742:0.02881104:-0.09525228:

            dele?.Invoke(entry.Message.Substring(index.Value));
        }
    }

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


    public class ACTLogLineManager
    {
        private readonly Object lockObj = new Object();
        private readonly IList<ACTLogLine> emptyList = new ACTLogLine[0];
        private IList<ACTLogLine> pendingQueue = new List<ACTLogLine>();

        private readonly Logger logger;

        private readonly ACTLogLineDelegator delegator;
        private readonly JavascriptDispatcher eventDispatcher;

        private readonly GobchatOverlayConfig config;

        public ACTLogLineManager(Logger logger, GobchatOverlay chatOverlay)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            delegator = new ACTLogLineDelegator(logger, OnLogLineUnhandled);
            delegator.SetHandle(ACTLogLineCode.GameLogLine, new ChatLogLineParser(logger, OnGameLogLine));
            delegator.SetHandle(ACTLogLineCode.PositionUpdate, new PositionUpdateParser(logger,OnPositionUpdate));
            delegator.SetHandle(ACTLogLineCode.ChangePrimaryPlayer, new PrimaryPlayerParser(OnPrimaryPlayerUpdate));

            eventDispatcher = new JavascriptDispatcher(chatOverlay);
            config = chatOverlay.Config;
        }

        private void OnLogLineUnhandled(int? code, ReaderIndex index, ACTLogLine line)
        {
            if(config.IsDebug)
                logger.LogInfo($"Ignored[{line.TimeStamp}] [{code}] {line.Message}");
        }

        private void OnPrimaryPlayerUpdate(string msg)
        {
            if (config.IsDebug)
                logger.LogInfo($"PrimaryPlayerUpdate: {msg}");
            eventDispatcher.DispatchEventToOverlay(new JavascriptEvents.PlayerNameEvent(msg));
        }

        private void OnPositionUpdate(string msg)
        {
            if (config.IsDebug)
                logger.LogInfo($"PositionUpdate: {msg}");
        }

        private void OnGameLogLine(ChatMessage message)
        {
            if (config.IsDebug)
                logger.LogInfo($"Dispatch[{message.Timestamp}] [{message.MessageType}] {message.Source}: {message.Message}");
            eventDispatcher.DispatchEventToOverlay(new JavascriptEvents.ChatMessageEvent(message));
        }

        public void EnqueueLogLine(ACTLogLine logLine)
        {
            lock (lockObj)
            {
                pendingQueue.Add(logLine);
            }
        }

        private IList<ACTLogLine> GetAndClearPendingRecords()
        {
            if (pendingQueue.Count == 0)
                return emptyList;

            lock (lockObj)
            {
                var tmp = pendingQueue;
                pendingQueue = new List<ACTLogLine>();
                return tmp;
            }
        }

        public void Update()
        {
            //logger.LogInfo("Run update");

            IList<ACTLogLine> pendingRecords = GetAndClearPendingRecords();
            foreach (var entry in pendingRecords)
            {
                delegator.ProcessRecord(entry);
            }
        }
    }
}
