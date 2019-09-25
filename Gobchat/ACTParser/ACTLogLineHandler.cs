namespace Gobchat
{
    public interface ACTLogLineHandler
    {
        void Handle(ReaderIndex index, ACTLogLine entry);
    }
}
