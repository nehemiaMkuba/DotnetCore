using System.ComponentModel;

namespace Core.Domain.Enums
{
    public enum NotificationCategories
    {
        [Description ("Recommended Content")]
        RecommendedContent = 1,
        [Description("Application Updates")]
        ApplicationUpdates,
        [Description("News and Offers")]
        NewsAndOffers
    }
}
