using System;
using CloudinaryDotNet;

namespace server.Common.Models;

public static class ImageQualityPresets
{
    // Avatar/thumbnail - small size, fast load 
    public static Transformation Thumbnail => new Transformation()
        .Width(200).Height(200).Crop("fill").Gravity("face")
        .Quality("auto:low")
        .FetchFormat("auto");

    // Image for blog post - medium size, good quality
    public static Transformation BlogPost => new Transformation()
        .Width(1200).Height(1200).Crop("limit")
        .Quality("auto:good")
        .FetchFormat("auto");

    // Product image - large size, best quality
    public static Transformation Product => new Transformation()
        .Width(2048).Height(2048).Crop("limit")
        .Quality("auto:best")
        .FetchFormat("auto");

    // Background image - full screen, low quality
    public static Transformation Background => new Transformation()
        .Width(1920).Height(1080).Crop("fill")
        .Quality(60)
        .FetchFormat("auto");
}
