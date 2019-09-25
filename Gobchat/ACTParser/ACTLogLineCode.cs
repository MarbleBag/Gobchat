namespace Gobchat
{
    public class ACTLogLineCode
    {
        public static readonly int Unknown = -1;
        public static readonly int GameLogLine = 0x00;
        public static readonly int PositionUpdate = 0x27;
        public static readonly int ChangeZone = 0x01;
        public static readonly int ChangePrimaryPlayer = 0x02;
        public static readonly int AddCombatant = 0x03;
        public static readonly int RemoveCombatant = 0x04;
    }
}
