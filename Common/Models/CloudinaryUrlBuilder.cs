using System;
using server.Application.Enums;

namespace server.Common.Models;

public class CloudinaryUrlBuilder
{
    public static string GetOptimizedUrl(string baseUrl, ImageSize size)
    {
        // Cloudinary URL pattern: 
        // https://res.cloudinary.com/{cloud_name}/image/upload/{transformations}/{public_id}

        var transformations = size switch
        {
            ImageSize.Thumbnail => "w_200,h_200,c_fill,g_face,q_auto:low,f_auto",
            ImageSize.Small => "w_400,h_400,c_limit,q_auto:low,f_auto",
            ImageSize.Medium => "w_800,h_800,c_limit,q_auto:good,f_auto",
            ImageSize.Large => "w_1200,h_1200,c_limit,q_auto:good,f_auto",
            ImageSize.Original => "q_auto:good,f_auto",
            _ => "q_auto,f_auto"
        };

        // Insert transformations vào URL
        return baseUrl.Replace("/upload/", $"/upload/{transformations}/");
    }
}
