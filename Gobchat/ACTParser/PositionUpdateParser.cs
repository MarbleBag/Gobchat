using System;
using System.Globalization;

namespace Gobchat
{
    public class PositionUpdate
    {
        public string Target { get; internal set; }
        public Point3F Position { get; internal set; }
        public PositionUpdate(string target, float x, float y, float z)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Position = new Point3F(x, y, z);
        }

        public override string ToString()
        {
            var className = nameof(PositionUpdate);
            return $"{className} => target:{Target} | position:{Position}";
        }
    }

    public class PositionUpdateParser : ACTLogLineHandler
    {
        private readonly char[] DELIMITER = new char[] { ':' };

        public delegate void OnParse(PositionUpdate update);

        private readonly Logger logger;
        private readonly OnParse onParse;

        public PositionUpdateParser(Logger logger, OnParse dele)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.onParse = dele ?? throw new ArgumentNullException(nameof(dele));
        }

        public void Handle(ReaderIndex index, ACTLogLine entry)
        {
            var relevantPart = entry.Message.Substring(index.Value);
            var data = relevantPart.Split(DELIMITER);
            logger.LogInfo("Parse Position in: " + relevantPart);

            //0[ID] 1[Target name] 2[?number?] 3[?number?] 4[?number?] 5[?number?] 6[?0?] 7[?0?] 8[position] 9[position] 10[position] 11[???]
            if (data.Length != 12) throw new ACTParseException($"PositionUpdate: Entry contains not enough data. Expected 12, was {data.Length}"); //something is fishy
            if (data[0].Length < 2 || !data[0].StartsWith("10")) return; //not a player

            var playerName = data[1].Trim();
            if (playerName.Length == 0) throw new ACTParseException($"PositionUpdate: Entry has player id set, but no player name"); //something is fishy

            if (!float.TryParse(data[8].Replace(',', '.'), out float x))
                throw new ACTParseException($"PositionUpdate: Unable to parse X | {data[8]}");
            if (!float.TryParse(data[9].Replace(',', '.'), out float y))
                throw new ACTParseException($"PositionUpdate: Unable to parse Y | {data[9]}");
            if (!float.TryParse(data[10].Replace(',', '.'), out float z))
                throw new ACTParseException($"PositionUpdate: Unable to parse Z | {data[810]}");

            //107901FB: Shiki Senri:85218:85218:10000:10000:0:0:-147.7774:20.46075:18.2:-1.281229:
            //[01:44:08.757] 27:10736CCB:Senshir Vairemont:93837:93837:10000:10000:0:0:1.728958:0.6658493:0.02892041:1.496085:
            //[01:44:32.744] 27:10736CCB:Senshir Vairemont:93837:93837:10000:10000:0:0:3.128052:-2.426208:0.02875674:-0.3738658:
            //[01:44:23.696] 27:10736CCB:Senshir Vairemont:93837:93837:10000:10000:0:0:-0.1042926:-0.2504024:-0.01531982:-2.525041:
            //[01:44:26.697] 27:10736CCB:Senshir Vairemont:93837:93837:10000:10000:0:0:2.792297:-1.510742:0.02881104:-0.09525228:

            onParse.Invoke(new PositionUpdate(playerName,x,y,z));
        }
    }
}
