namespace server.Application.Enums;

public enum ImageSize
{
    Thumbnail,
    Small,
    Medium,
    Large,
    Original
}

public enum ImageUploadQuality
{
    /// <summary>
    /// 200x200, low quality
    /// </summary>
    Thumbnail,
    /// <summary>
    /// 800x800, 60% quality
    /// </summary>
    Low,
    /// <summary>
    /// 1200x1200, auto:good
    /// </summary>
    Standard,
    /// <summary>
    /// 2048x2048, auto:best
    /// </summary>
    High,
    /// <summary>
    /// No resize, auto quality
    /// </summary>
    Original
}
