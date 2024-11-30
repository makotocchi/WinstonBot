namespace WinstonBot.Exceptions;

internal class NullVideoException : Exception
{
    public NullVideoException() : base("The video is null.")
    {
    }
}
