using System;
using System.Collections.Generic;

namespace Gobchat.Memory
{
    public class FFXIVMemoryProcessor
    {

        private readonly FFXIVProcessFinder processFinder = new FFXIVProcessFinder();
        private readonly Chat.ChatlogReader chatlogProcessor = new Chat.ChatlogReader();
        private readonly Chat.ChatlogBuilder chatlogBuilder = new Chat.ChatlogBuilder();

        public bool FFXIVProcessValid { get { return processFinder.FFXIVProcessValid; } }
        public int FFXIVProcessId { get { return processFinder.FFXIVProcessId; } }

        public event EventHandler<ProcessChangeEvent> ProcessChangeEvent;
        public event EventHandler<Chat.ChatlogEvent> ChatlogEvent;

        public void Initialize()
        {
            /* MemoryHandler.Instance.SignaturesFoundEvent += delegate (object sender, Sharlayan.Events.SignaturesFoundEvent e)
             {
                 foreach (KeyValuePair<string, Signature> kvp in e.Signatures)
                 {
                     Debug.WriteLine($"{kvp.Key} => {kvp.Value.GetAddress():X}");
                 }
             };

             this.ProcessChangeEvent += (object sender, ProcessChangeEvent e) => Debug.WriteLine($"FFXIV Process changed! {e.IsProcessValid} {e.ProcessId}");*/
        }

        public void Update()
        {
            CheckProcess();
            if (FFXIVProcessValid)
            {
                var logs = chatlogProcessor.Query();
                if (logs.Count > 0 && ChatlogEvent != null)
                {
                    List<Chat.ChatlogItem> items = new List<Chat.ChatlogItem>();
                    foreach (var item in logs)
                        items.Add(chatlogBuilder.Build(item));
                    ChatlogEvent.Invoke(this, new Chat.ChatlogEvent(items));
                }
            }

            // List<ChatlogItem> items = new List<ChatlogItem>();
            // items.Add(chatlogBuilder.Build());
            // ChatlogEvent?.Invoke(this, new ChatlogEvent(items));
        }

        private void CheckProcess()
        {
            //TODO MUTEX
            var processChanged = processFinder.CheckProcess();
            if (!processChanged)
                return; //nothing to do

            ProcessChangeEvent?.Invoke(this, new ProcessChangeEvent(FFXIVProcessValid, FFXIVProcessId));
        }

    }
}


