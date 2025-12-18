using server.Common.Models;

namespace server.Common.Providers;

public interface INotificationCategoryProvider
{
    NotificationCategoryConfig Config { get; }
    NotificationCatalog Catalog { get; }
    NotificationCategoryDefinition? GetCategoryByCode(string code);
    NotificationSubCategoryDefinition? GetSubCategoryByCode(string code);
}
