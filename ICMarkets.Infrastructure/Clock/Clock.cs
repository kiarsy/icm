using ICMarkets.Application.Abstractions;

namespace ICMarkets.Infrastructure.Clock;

public class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;

}