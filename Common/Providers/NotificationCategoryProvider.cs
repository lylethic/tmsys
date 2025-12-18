using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using server.Common.Models;

namespace server.Common.Providers;

public class NotificationCategoryProvider : INotificationCategoryProvider
{
    private readonly Dictionary<string, NotificationCategoryDefinition> _categories;
    private readonly Dictionary<string, NotificationSubCategoryDefinition> _subCategories;

    public NotificationCategoryProvider(IWebHostEnvironment hostEnvironment, ILogger<NotificationCategoryProvider> logger)
    {
        Config = LoadConfig(hostEnvironment, logger);
        Catalog = Config.Notification ?? new NotificationCatalog();
        _categories = new(StringComparer.OrdinalIgnoreCase);
        _subCategories = new(StringComparer.OrdinalIgnoreCase);

        if (Catalog.Categories is null)
            return;

        foreach (var category in Catalog.Categories.Where(c => !string.IsNullOrWhiteSpace(c.Code)))
        {
            _categories[category.Code] = category;

            if (category.SubCategories is null)
                continue;

            foreach (var sub in category.SubCategories.Where(s => !string.IsNullOrWhiteSpace(s.Code)))
            {
                sub.ParentCategory = category;
                _subCategories[sub.Code] = sub;
            }
        }
    }

    public NotificationCategoryConfig Config { get; }

    public NotificationCatalog Catalog { get; }

    public NotificationCategoryDefinition? GetCategoryByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        return _categories.TryGetValue(code, out var category) ? category : null;
    }

    public NotificationSubCategoryDefinition? GetSubCategoryByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        return _subCategories.TryGetValue(code, out var subCategory) ? subCategory : null;
    }

    private static NotificationCategoryConfig LoadConfig(IWebHostEnvironment env, ILogger logger)
    {
        try
        {
            var filePath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "notification-categories.json");

            if (!File.Exists(filePath))
            {
                logger.LogWarning("Notification categories file not found at {Path}", filePath);
                return new NotificationCategoryConfig();
            }

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            return JsonSerializer.Deserialize<NotificationCategoryConfig>(json, options) ?? new NotificationCategoryConfig();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to load notification category configuration.");
            return new NotificationCategoryConfig();
        }
    }
}
