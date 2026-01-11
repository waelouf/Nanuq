namespace Nanuq.Common.Exceptions;

public class EncryptionException : Exception
{
    public EncryptionException(string message) : base(message) { }

    public EncryptionException(string message, Exception inner) : base(message, inner) { }
}
