using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace server.Services.Templates;

public static class EmailTemplateManager
{
    private static string _templateFolderPath = string.Empty;

    /// <summary>
    /// Initializes the EmailTemplateManager with the application's environment to locate the template folder.
    /// This method MUST be called once at application startup (e.g., in Program.cs).
    /// Khởi tạo EmailTemplateManager với môi trường của ứng dụng để xác định vị trí thư mục template.
    /// Phương thức này BẮT BUỘC phải được gọi một lần khi ứng dụng khởi động (ví dụ: trong Program.cs).
    /// </summary>
    /// <param name="env">The IWebHostEnvironment instance from the application's service provider.</param>
    public static void Initialize(IWebHostEnvironment env)
    {
        // Path is now relative to the project's wwwroot folder, making it more portable.
        _templateFolderPath = Path.Combine(env.ContentRootPath, "wwwroot", "Resource");
    }

    /// <summary>
    /// A private helper method to asynchronously load the raw content of a template file.
    /// Phương thức helper private để tải nội dung thô của một tệp mẫu một cách bất đồng bộ.
    /// </summary>
    /// <param name="templateFileName">The name of the HTML file (e.g., "PasswordReset.html").</param>
    /// <returns>A Task representing the asynchronous operation, with the string content of the file.</returns>
    private static async Task<string> LoadTemplateAsync(string templateFileName)
    {
        if (string.IsNullOrEmpty(_templateFolderPath))
        {
            throw new InvalidOperationException("EmailTemplateManager has not been initialized. Call Initialize() at startup.");
        }

        try
        {
            var filePath = Path.Combine(_templateFolderPath, templateFileName);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: Email template not found at '{filePath}'");
                throw new FileNotFoundException($"Email template not found at {filePath}");
            }

            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while loading the email template '{templateFileName}': {ex.Message}");
            // Re-throw the exception to be handled by the calling service.
            throw;
        }
    }

    /// <summary>
    /// Gets the Password Reset email template and populates it with dynamic data.
    /// Lấy mẫu email Đặt lại mật khẩu và điền dữ liệu động vào đó.
    /// </summary>
    /// <param name="userEmail">The recipient's email address.</param>
    /// <param name="resetCode">The password reset code.</param>
    /// <returns>The complete HTML body for the email.</returns>
    public static async Task<string> GetPasswordResetEmailAsync(string userEmail, string resetCode)
    {
        string templateContent = await LoadTemplateAsync("EmailSendOTPCode.html");

        // Standardized placeholders to {{...}}
        templateContent = templateContent.Replace("{{Email}}", userEmail)
                                           .Replace("{{ResetCode}}", resetCode)
                                           .Replace("{{AppName}}", "Loopy");

        return templateContent;
    }

    /// <summary>
    /// Gets the Welcome email template.
    /// </summary>
    /// <param name="userEmail">The new user's email.</param>
    /// <param name="userName">The new user's name.</param>
    /// <returns>The complete HTML body for the email.</returns>
    public static async Task<string> GetWelcomeEmailAsync(string userEmail, string userName)
    {
        // Assumes your welcome email is named "WelcomeEmail.html"
        // Giả sử email chào mừng của bạn có tên là "WelcomeEmail.html"
        string templateContent = await LoadTemplateAsync("WelcomeEmail.html");

        templateContent = templateContent.Replace("{{Email}}", userEmail)
                                           .Replace("{{Name}}", userName ?? "User")
                                           .Replace("{{AppName}}", "Loopy");

        return templateContent;
    }
}

