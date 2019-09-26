using System;
using System.Collections.Generic;

namespace Gobchat
{
    public class EmptyParser : ACTLogLineHandler
    {
        public delegate void OnParse(string msg);

        private OnParse onParse;

        public EmptyParser(OnParse onParse)
        {
            this.onParse = onParse;
        }

        public void Handle(ReaderIndex index, ACTLogLine entry)
        {
            onParse?.Invoke(entry.Message.Substring(index.Value));
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
            delegator.SetHandle(ACTLogLineCode.PositionUpdate, new PlayerPositionParser(logger,OnPositionUpdate));
            //delegator.SetHandle(ACTLogLineCode.ChangePrimaryPlayer, new PrimaryPlayerParser(OnPrimaryPlayerUpdate));

            delegator.SetHandle(ACTLogLineCode.AddCombatant, new EmptyParser(null));
            delegator.SetHandle(ACTLogLineCode.RemoveCombatant, new EmptyParser(null));
            delegator.SetHandle(ACTLogLineCode.HPUpdate, new EmptyParser(null));

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
            //eventDispatcher.DispatchEventToOverlay(new JavascriptEvents.PlayerNameEvent(msg));
        }

        private void OnPositionUpdate(PlayerPosition positionUpdate)
        {
            if (config.IsDebug)
                logger.LogInfo($"PositionUpdate: {positionUpdate}");
            eventDispatcher.DispatchEventToOverlay(new JavascriptEvents.PlayerPositionEvent(positionUpdate));
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
