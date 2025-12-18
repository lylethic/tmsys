namespace server.Common.Models
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string CloudinaryURL { get; set; } = string.Empty;
        public string Preset_Name { get; set; } = string.Empty;
        public string Folder_Name { get; set; } = string.Empty;
        public string CloudinaryPath { get; set; } = string.Empty;
    }

    // Results of an image after upload
    public class CloudinaryUploadResult
    {
        public string PublicId { get; set; } = string.Empty;     // ID để quản lý ảnh trên Cloudinary
        public string Url { get; set; } = string.Empty;           // URL đầy đủ của ảnh
        public string SecureUrl { get; set; } = string.Empty;     // URL HTTPS của ảnh
        public string Format { get; set; } = string.Empty;          // Định dạng ảnh (jpg, png...)
        public int Width { get; set; }              // Chiều rộng
        public int Height { get; set; }             // Chiều cao
        public long Bytes { get; set; }             // Kích thước file (bytes)
        public string ResourceType { get; set; } = string.Empty;    // Loại resource (image, video...)
        public DateTime CreatedAt { get; set; }     // Thời gian tạo
    }

    // Details of an uploaded image
    public class CloudinaryImageInfo
    {
        public string PublicId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string SecureUrl { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public long Bytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, string> ImageMetadata { get; set; } = [];
    }
}
