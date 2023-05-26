using System;

namespace Core.Domain.Infrastructure.Services
{
    public interface IDateTimeService
    {
        DateTime Now { get; }
        DateTime Today { get; }
        DateTime Tomorrow { get; }        
        DateTime MaxToday { get; }
        DateTime Yesterday { get; }
        DateTime MaxYesterday { get; }
    }
}