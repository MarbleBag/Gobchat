using System;
using System.Globalization;

namespace Gobchat
{
    public class PlayerPosition
    {
        public string Target { get; internal set; }
        public float X { get; internal set; }
        public float Y { get; internal set; }
        public float Z { get; internal set; }

        public PlayerPosition(string target, float x, float y, float z)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            var className = nameof(PlayerPosition);
            return $"{className} => target:{Target} | x:{X} | y:{Y} | z:{Z}";
        }
    }

    public class PlayerPositionParser : ACTLogLineHandler
    {
        private readonly char[] DELIMITER = new char[] { ':' };

        public delegate void OnParse(PlayerPosition update);

        private readonly Logger logger;
        private readonly OnParse onParse;

        public PlayerPositionParser(Logger logger, OnParse dele)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.onParse = dele ?? throw new ArgumentNullException(nameof(dele));
        }

        public void Handle(ReaderIndex index, ACTLogLine entry)
        {
            var relevantPart = entry.Message.Substring(index.Value);
            var data = relevantPart.Split(DELIMITER);

            //106E41FD:[name]:94211:94211:10000:10000:0:0:-658.313:-752.0111:1.293421E-05:1.897132:
            //[0]     :[1]          :[2]  :[3]  :[4]  :[5]  : : :[8]     :[9]      :[10]        :[11]    :[12]

            //0[ID] 1[Target name] 2[?number?] 3[?number?] 4[?number?] 5[?number?] 6[?0?] 7[?0?] 8[position] 9[position] 10[position] 11[float something] 12[???]
            if (data.Length != 13) throw new ACTParseException($"PositionUpdate: Entry contains not enough data. Expected 13, was {data.Length}"); //something is fishy
            if (data[0].Length < 2 || !data[0].StartsWith("10")) return; //not a player

            var playerName = data[1].Trim();
            if (playerName.Length == 0) return; //can happen if player names aren't loaded yet throw new ACTParseException($"PositionUpdate: Entry has player id set, but no player name"); //something is fishy

            if (!float.TryParse(data[8].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                throw new ACTParseException($"PositionUpdate: Unable to parse X | {data[8]}");
            if (!float.TryParse(data[9].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                throw new ACTParseException($"PositionUpdate: Unable to parse Y | {data[9]}");
            if (!float.TryParse(data[10].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                throw new ACTParseException($"PositionUpdate: Unable to parse Z | {data[10]}");

            onParse.Invoke(new PlayerPosition(playerName,x,y,z));
        }
    }
}
