using Gobchat.Core.Chat;

namespace Gobchat.Module.Actor
{
    public interface IActorManager
    {
        bool IsEnabled { get; }

        int GetPlayerCount();

        float GetDistanceToPlayerWithName(string name);

        float GetFastDistanceToPlayerWithName(string name);

        string[] GetPlayersInArea();
    }
}