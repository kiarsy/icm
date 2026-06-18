using System.Net;

namespace ICMarkets.Domain.Common.Exceptions;

public abstract class BaseException : Exception
{
    protected BaseException(
        string message)
        : base(message)
    {
        HttpCode = (int)HttpStatusCode.InternalServerError;
    }

    protected BaseException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        HttpCode = (int)HttpStatusCode.InternalServerError;
    }


    protected BaseException(
        string message,
        HttpStatusCode httpStatusCode,
        Exception innerException)
        : base(message, innerException)
    {
        HttpCode = (int)httpStatusCode;
    }

    public int HttpCode { get; }
}

public class BlockCypherRateLimitException()
    : BaseException("BlockCypher reached rate limit", HttpStatusCode.TooManyRequests, null)
{
}

public class BlockCypherTooManyRequestException()
    : BaseException("BlockCypher has more requests than rate limit", HttpStatusCode.TooManyRequests, null)
{
}

public class ConcurrentException(string entity)
    : BaseException($"A concurrent exception happened when manipulating {entity}", HttpStatusCode.Conflict, null)
{
}