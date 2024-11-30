namespace WinstonBot.Exceptions;

internal class NullPlayerException : Exception
{
    public NullPlayerException() : base("The player is null.")
    {
    }
}
