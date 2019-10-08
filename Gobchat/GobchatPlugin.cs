using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using RainbowMage.OverlayPlugin;


namespace Gobchat
{
    public class GobchatOverlay : OverlayBase<GobchatOverlayConfig>
    {
        private readonly Object lockObj = new Object();

        //private readonly SynchronizationContext mainThreadSync;
        private readonly Logger logger;

       // private System.Timers.Timer updateTimer;
        private ACTLogLineManager logManager;

        private bool isInitialized = false;

        public GobchatOverlay(GobchatOverlayConfig config)
            : base(config, config.Name)
        {
            var mainThreadSync = System.Windows.Forms.WindowsFormsSynchronizationContext.Current;
            logger = new MainLogger((level, format, args) => { mainThreadSync.Post((state) => this.Log(level, format, args), null); });
        }

        public Logger getLogger()
        {
            return logger;
        }

        public void Initialize()
        {
            if (isInitialized)
                return;
            isInitialized = true;

            InitializeLogManager();
            InitializeConfigListener();
            //InitializeUpdateTimer();
            //startUpdateTimer();

            CheckPluginVersion();
        }

        private void CheckPluginVersion()
        {
            var versionFetcher = new VersionFetcher(getLogger());

            Version localVersion = versionFetcher.GetLocalVersion();
            Version remoteVersion = versionFetcher.GetRemoteVersion();

            var logger = getLogger();
            logger.LogInfo($"Gobchat version: {localVersion}");


            if(remoteVersion.Major == 00 && remoteVersion.Minor == 0)
            {
                string msg = $"Unable to check Github version.\nYour version: {localVersion}\n\nVisit {VersionFetcher.RELEASE_URL}";
                logger.LogError(msg);
                var result = System.Windows.Forms.MessageBox.Show(Overlay, msg + "\n\nOpen Github?", "Manual update check", System.Windows.Forms.MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
                    System.Diagnostics.Process.Start(VersionFetcher.RELEASE_URL);
                return;
            }

            if(localVersion < remoteVersion)
            {
                string msg = $"A newer version of Gobchat is available on Github.\nYour version: {localVersion}\nNewer version: {remoteVersion}\n\nVisit {VersionFetcher.RELEASE_URL}";
                logger.LogError(msg);
                var result = System.Windows.Forms.MessageBox.Show(Overlay, msg + "\n\nOpen Github?", "Update available", System.Windows.Forms.MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
                    System.Diagnostics.Process.Start(VersionFetcher.RELEASE_URL);
                return;
            }
        }

        public override void Dispose()
        {
            if (!isInitialized)
                return;
            isInitialized = false;

            Advanced_Combat_Tracker.ActGlobals.oFormActMain.OnLogLineRead -= OnACTLogLineRead;
            base.Dispose();
        }

        private void InitializeLogManager()
        {
            logManager = new ACTLogLineManager(logger,this);
            Advanced_Combat_Tracker.ActGlobals.oFormActMain.OnLogLineRead += OnACTLogLineRead;
        }

        private void InitializeConfigListener()
        {
           
        }

        private void OnACTLogLineRead(bool isImport, Advanced_Combat_Tracker.LogLineEventArgs logInfo)
        {
            if (logInfo != null && Config.IsPluginActive)
            {
                if (isImport) //TODO
                {
                    //logger.LogInfo(logInfo.ToString());
                    logger.LogInfo($"Imported: Time: {logInfo.detectedTime} |Zone: {logInfo.detectedZone} |Type: {logInfo.detectedType} |Line: {logInfo.logLine} |Original: {logInfo.originalLogLine}");
                }
                else
                {
                    logManager.EnqueueLogLine(new ACTLogLine(logInfo.detectedTime, logInfo.logLine));
                }
            }
        }

        protected override void Update() //called by act, roughly once per second
        {
            Initialize();

            try
            {
                UpdateInternal();
            }
            catch (Exception e)
            {
                logger.LogError("Exception in Update: " + e.Message);
                logger.LogError("Exception in Update: " + e.StackTrace);
                logger.LogError("Exception in Update: " + e.Source);
            }
        }

        private void UpdateInternal()
        {
            logManager.Update();
        }

        //incoming message from javascript
        public override void OverlayMessage(string message)
        {
            var reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(message));
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var obj = serializer.Deserialize<Dictionary<string, string>>(reader);

            try
            {
                if (obj.ContainsKey("event"))
                {
                    var eventName = obj["event"];
                    //logger.LogInfo("Event: " + eventName);
                    //logger.LogInfo("Data: " + message.Replace("{", "{{").Replace("}", "}}"));

                    if ("SaveGobchatConfig".Equals(eventName))
                    {
                        if (obj.ContainsKey("detail"))
                        {
                            Config.OverlayConfigData = obj["detail"];
                        }
                        else
                        {
                            logger.LogError("SaveGobchatConfig is empty");
                        }                        
                    }
                    else if ("LoadGobchatConfig".Equals(eventName))
                    {
                        JavascriptDispatcher dispatcher = new JavascriptDispatcher(this);
                        dispatcher.DispatchEventToOverlay(new JavascriptEvents.LoadGobchatConfigEvent(Config.OverlayConfigData));
                    }
                }
            }catch(Exception e)
            {
                logger.LogError("Exception in OverlayMessage: " + e.Message);
                logger.LogError("Exception in OverlayMessage: " + e.StackTrace);
                logger.LogError("Exception in OverlayMessage: " + e.Source);
            }
            //TODO find out how to do a ping on mention
            //Advanced_Combat_Tracker.ActGlobals.oFormActMain.TTS(obj["cmd"]);
        }


    }

    public class GobchatOverlayConfig : OverlayConfigBase
    {
        public bool IsDebug
        {
            get { return this.debug; }
            set
            {
                this.debug = value;

                var handler = DebugChanged;
                if(handler!=null)
                    handler.Invoke(this, new DebugeChangedEventArgs(debug));
            }
        }

        public bool IsPluginActive
        {
            get { return this.pluginActive; }
            set
            {
                this.pluginActive = value;

                var handler = PluginActiveChanged;
                if (handler != null)
                    handler.Invoke(this, new PluginActiveChangedEventArgs(pluginActive));
            }
        }

        public override Type OverlayType
        {
            get { return typeof(GobchatOverlay); }
        }

        public string OverlayConfigData {
            get
            {
                return overlayConfigData;
            }
            set
            {
                overlayConfigData = value;
                var handler = OverlayConfigDataChanged;
                if (handler != null)
                    handler.Invoke(this, new OverlayConfigDataChangedEventArgs(overlayConfigData));
            }
        }

        private bool debug;
        private bool pluginActive;
        private string overlayConfigData;

        public event EventHandler<DebugeChangedEventArgs> DebugChanged;
        public event EventHandler<PluginActiveChangedEventArgs> PluginActiveChanged;
        public event EventHandler<OverlayConfigDataChangedEventArgs> OverlayConfigDataChanged;

        public GobchatOverlayConfig(string name) : base(name)
        {

        }

        public GobchatOverlayConfig() : base(null)
        {

        }

    }

    public class DebugeChangedEventArgs
    {
        public bool IsDebug { get; }
        public DebugeChangedEventArgs(bool debug) { IsDebug = debug; }
    }

    public class PluginActiveChangedEventArgs
    {
        public bool IsPluginActive { get; }
        public PluginActiveChangedEventArgs(bool active) { IsPluginActive = active; }
    }

    public class OverlayConfigDataChangedEventArgs
    {
        public string OverlayConfigData { get; }
        public OverlayConfigDataChangedEventArgs(string overlayConfigData) { OverlayConfigData = overlayConfigData; }
    }

    public class HideServerChangedEventArgs
    {
        public bool IsHideServer { get; }
        public HideServerChangedEventArgs(bool hideServer) { IsHideServer = hideServer; }
    }

    public class GobchatPlugin : IOverlayAddon
    {
        public string Name
        {
            get { return "Gobchat"; }
        }

        public string Description{
            get { return "Chat overlay with QoL features for roleplay"; }
        }

        public Type OverlayType
        {
            get { return typeof(GobchatOverlay); }
        }

        public Type OverlayConfigType
        {
            get { return typeof(GobchatOverlayConfig); }
        }

        public Type OverlayConfigControlType
        {
            get { return typeof(GobchatOverlayControlPanel); }
        }

        public Control CreateOverlayConfigControlInstance(IOverlay overlay)
        {
            return new GobchatOverlayControlPanel((GobchatOverlay)overlay);            
        }

        public IOverlayConfig CreateOverlayConfigInstance(string name)
        {
            return new GobchatOverlayConfig(name);
        }

        public IOverlay CreateOverlayInstance(IOverlayConfig config)
        {
            config.MaxFrameRate = 60;
            return new GobchatOverlay((GobchatOverlayConfig)config);
        }

        public void Dispose()
        {
            
        }
    }

}
