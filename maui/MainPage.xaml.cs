using System.Text;
using System.Text.Json;

namespace SlideshowApp;

public partial class MainPage : ContentPage
{
    private readonly SlideshowService _slideshowService;

    public MainPage(SlideshowService slideshowService)
    {
        Console.WriteLine("MainPage constructor called");
        InitializeComponent();
        Console.WriteLine("InitializeComponent completed");
        _slideshowService = slideshowService;
        Console.WriteLine("MainPage constructor finished");

        // Initialize WebView after the page is loaded
        this.Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        Console.WriteLine("Page loaded, initializing WebView");
        await InitializeWebView();
    }

    private async Task InitializeWebView()
    {
        try
        {
            Console.WriteLine("Initializing WebView...");
            
            // Set up the WebView source
            var htmlSource = new HtmlWebViewSource
            {
                Html = await LoadHtmlContent()
            };

            SlideshowWebView.Source = htmlSource;
            
            // Handle navigation for API calls
            SlideshowWebView.Navigating += OnWebViewNavigating;
            
            // Handle WebView loaded event
            SlideshowWebView.Navigated += (s, e) =>
            {
                Console.WriteLine("WebView navigation completed");
            };
            
            Console.WriteLine("WebView initialized successfully");
        }
        catch (Exception ex)
        {
            // Log error but continue
            var errorMsg = $"Failed to initialize WebView: {ex.Message}\n{ex.StackTrace}";
            System.Diagnostics.Debug.WriteLine(errorMsg);
            Console.WriteLine(errorMsg);
        }
    }

    private async Task<string> LoadHtmlContent()
    {
        try
        {
            Console.WriteLine("Loading HTML content...");

            // Try to read from WebAssets first
            using var stream = await FileSystem.OpenAppPackageFileAsync("WebAssets/index.html");
            using var reader = new StreamReader(stream);
            var htmlContent = await reader.ReadToEndAsync();

            Console.WriteLine($"Loaded HTML: {htmlContent.Length} characters");

            // Load CSS
            using var cssStream = await FileSystem.OpenAppPackageFileAsync("WebAssets/styles.css");
            using var cssReader = new StreamReader(cssStream);
            var cssContent = await cssReader.ReadToEndAsync();

            Console.WriteLine($"Loaded CSS: {cssContent.Length} characters");

            // Load JavaScript
            using var jsStream = await FileSystem.OpenAppPackageFileAsync("WebAssets/script.js");
            using var jsReader = new StreamReader(jsStream);
            var jsContent = await jsReader.ReadToEndAsync();

            Console.WriteLine($"Loaded JS: {jsContent.Length} characters");

            // Embed CSS and JS directly into HTML
            htmlContent = htmlContent.Replace("<link rel=\"stylesheet\" href=\"styles.css\">",
                $"<style>{cssContent}</style>");
            htmlContent = htmlContent.Replace("<script src=\"script.js\"></script>",
                $"<script>{jsContent}</script>");

            Console.WriteLine("HTML content assembled successfully");
            return htmlContent;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error loading HTML content: {ex.Message}\n{ex.StackTrace}";
            System.Diagnostics.Debug.WriteLine(errorMsg);
            Console.WriteLine(errorMsg);
            return GetFallbackHtml();
        }
    }

    private string GetFallbackHtml()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Slideshow</title>
    <style>
        body { margin: 0; padding: 0; background: black; color: white; font-family: Arial, sans-serif; }
        .error { text-align: center; padding: 50px; }
    </style>
</head>
<body>
    <div class='error'>
        <h1>Error Loading Slideshow</h1>
        <p>Could not load slideshow content. Check the configuration and media files.</p>
    </div>
</body>
</html>";
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        // Intercept API calls
        if (e.Url.StartsWith("/api/"))
        {
            e.Cancel = true;
            await HandleApiRequest(e.Url);
        }
    }

    private async Task HandleApiRequest(string url)
    {
        try
        {
            string jsonResponse = string.Empty;

            if (url == "/api/config")
            {
                var config = _slideshowService.GetConfig();
                jsonResponse = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            else if (url == "/api/files")
            {
                var files = await _slideshowService.GetMediaFilesAsync();
                jsonResponse = JsonSerializer.Serialize(files, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            // Inject the response into JavaScript
            var script = $"if (window.handleApiResponse) window.handleApiResponse('{url}', {jsonResponse});";
            // await SlideshowWebView.EvaluateJavaScriptAsync(script); // Temporarily disabled
            Console.WriteLine($"Would execute script: {script}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling API request {url}: {ex.Message}");
        }
    }
}
