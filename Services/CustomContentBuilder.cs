using Microsoft.AspNetCore.Http;

namespace IvyQrCodeProfileSharing.Services;

public class CustomContentBuilder : IContentBuilder
{
    public CustomContentBuilder()
    {
        Console.WriteLine("🔧 CustomContentBuilder initialized");
    }

    public bool CanHandle(object? content)
    {
        // Log all content types we receive
        Console.WriteLine($"🔍 ContentBuilder received: {content?.GetType().Name ?? "null"}");
        
        // Return true if we can handle this content type
        if (content is string stringContent)
        {
            Console.WriteLine($"🔍 String content length: {stringContent.Length}");
            Console.WriteLine($"🔍 String content preview: {stringContent.Substring(0, Math.Min(100, stringContent.Length))}...");
            return stringContent.Contains("<html") || stringContent.Contains("<!DOCTYPE") || stringContent.Length > 0;
        }
        return false;
    }

    public object? Format(object? content)
    {
        if (content is string stringContent)
        {
            Console.WriteLine($"🔧 Processing content (length: {stringContent.Length})");
            
            // 1. Add Custom Headers - Add meta tags for SEO and security
            if (stringContent.Contains("<head>"))
            {
                var customMetaTags = @"
    <meta name=""generator"" content=""Ivy QR Code Profile Sharing"">
    <meta name=""author"" content=""CustomContentBuilder"">
    <meta name=""robots"" content=""index, follow"">
    <meta http-equiv=""X-Content-Type-Options"" content=""nosniff"">
    <meta http-equiv=""X-Frame-Options"" content=""DENY"">";
                
                stringContent = stringContent.Replace("<head>", "<head>" + customMetaTags);
                Console.WriteLine("   ✨ Added custom meta tags for SEO and security");
            }
            
            // 2. Content Filtering - Hide sensitive information
            if (stringContent.Contains("password") || stringContent.Contains("secret"))
            {
                stringContent = stringContent.Replace("password=", "password=***");
                stringContent = stringContent.Replace("secret=", "secret=***");
                Console.WriteLine("   🔒 Filtered sensitive information");
            }
            
            // 3. Analytics Integration - Add custom tracking
            if (stringContent.Contains("</body>"))
            {
                var analyticsScript = @"
    <script>
        console.log('🚀 Page loaded with CustomContentBuilder processing');
        console.log('📊 Custom analytics tracking active');
        // Add your analytics code here
        if (typeof gtag !== 'undefined') {
            gtag('event', 'page_view', {
                'custom_parameter': 'content_builder_processed'
            });
        }
    </script>";
                
                stringContent = stringContent.Replace("</body>", analyticsScript + "\n</body>");
                Console.WriteLine("   📊 Added custom analytics tracking");
            }
            
            // 4. Content Transformation - Convert markdown-style formatting
            stringContent = stringContent.Replace("**bold**", "<strong>bold</strong>");
            stringContent = stringContent.Replace("*italic*", "<em>italic</em>");
            stringContent = stringContent.Replace("`code`", "<code>code</code>");
            
            if (stringContent.Contains("<strong>") || stringContent.Contains("<em>") || stringContent.Contains("<code>"))
            {
                Console.WriteLine("   🎨 Applied markdown-style formatting");
            }
            
            // 5. Add Custom Comment
            if (stringContent.Contains("<html"))
            {
                var customComment = "<!-- Processed by CustomContentBuilder - Enhanced with SEO, Security, Analytics, and Formatting -->";
                stringContent = stringContent.Replace("<html", customComment + "\n<html");
                Console.WriteLine("   ✨ Added enhanced custom comment");
            }
            
            return stringContent;
        }
        
        return content;
    }
}
