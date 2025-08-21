using System.Diagnostics;
using System.Text.Json;
using Microsoft.Maui.Handlers;
#if IOS || MACCATALYST
using Foundation;
#endif

namespace SlideshowApp;

public sealed partial class MainPage
{
    private readonly SlideshowService slideshowService;

    public MainPage()
    {
        slideshowService = new SlideshowService();
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object? sender, EventArgs e)
    {
        try
        {
            // Set up the WebView source
            var htmlSource = new HtmlWebViewSource
            {
                Html = await LoadHtmlContent(),
            };

            SlideshowWebView.Source = htmlSource;

            // Handle navigation for API calls
            SlideshowWebView.Navigating += OnWebViewNavigating;

            // Also handle Navigated event for debugging
            SlideshowWebView.Navigated += OnWebViewNavigated;

            // Enable WebView debugging after the WebView is loaded
            SlideshowWebView.Loaded += OnWebViewLoaded;
        }
        catch (Exception ex)
        {
            // Log error but continue
            var errorMsg = $"Failed to initialize WebView: {ex.Message}\n{ex.StackTrace}";
            Debug.WriteLine(errorMsg);
            Console.WriteLine(errorMsg);
        }
    }


    private void OnWebViewLoaded(object? sender, EventArgs e)
    {
        try
        {
            EnableWebViewDebugging();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to enable WebView debugging: {ex.Message}");
        }
    }

    private void EnableWebViewDebugging()
    {
        try
        {
            #if ANDROID
            // Enable WebView debugging on Android
            Android.Webkit.WebView.SetWebContentsDebuggingEnabled(true);
            System.Diagnostics.Debug.WriteLine("Android WebView debugging enabled");
            #elif IOS || MACCATALYST
            // For iOS/macOS, attempt to enable web inspector
            Debug.WriteLine("Attempting to enable web inspector for iOS/macOS");

            // The handler should be available after the WebView is loaded
            WebViewHandler.Mapper.AppendToMapping(
                "EnableDebugging",
                static (handler, _) =>
                {
                    if (handler.PlatformView is  { } wkWebView)
                    {
                        try
                        {
                            // Enable web inspector if running in debug mode or simulator
                            if (Debugger.IsAttached)
                            {
                                wkWebView.SetValueForKey(NSObject.FromObject(true), new NSString("inspectable"));
                                Debug.WriteLine("Web inspector enabled for WKWebView");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to enable web inspector: {ex.Message}");
                        }
                    }
                }
            );
            #endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error enabling WebView debugging: {ex.Message}");
        }
    }

    private async Task<string> LoadHtmlContent()
    {
        try
        {
            // Try to read from WebAssets first
            await using var stream = await FileSystem.OpenAppPackageFileAsync("WebAssets/index.html");
            using var reader = new StreamReader(stream);
            var htmlContent = await reader.ReadToEndAsync();

            // Load CSS
            await using var cssStream = await FileSystem.OpenAppPackageFileAsync("WebAssets/styles.css");
            using var cssReader = new StreamReader(cssStream);
            var cssContent = await cssReader.ReadToEndAsync();

            // Load JavaScript
            await using var jsStream = await FileSystem.OpenAppPackageFileAsync("WebAssets/script.js");
            using var jsReader = new StreamReader(jsStream);
            var jsContent = await jsReader.ReadToEndAsync();

            // Embed CSS and JS directly into HTML
            htmlContent = htmlContent.Replace(
                "<link rel=\"stylesheet\" href=\"styles.css\">",
                $"<style>{cssContent}</style>"
            );
            htmlContent = htmlContent.Replace(
                "<script src=\"script.js\"></script>",
                $"<script>{jsContent}</script>"
            );

            return htmlContent;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error loading HTML content: {ex.Message}\n{ex.StackTrace}";
            Debug.WriteLine(errorMsg);
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
        try
        {
            Debug.WriteLine($"WebView navigating to: {e.Url}");

            // Intercept custom slideshow-api protocol
            if (e.Url.StartsWith("slideshow-api:", StringComparison.Ordinal))
            {
                e.Cancel = true;

                // Extract the API path from the custom URL
                var apiPath = e.Url.Substring("slideshow-api:".Length);
                Debug.WriteLine($"Extracted API path: {apiPath}");

                if (!string.IsNullOrEmpty(apiPath))
                {
                    await HandleApiRequest(apiPath);
                }
            }
            // Also check for direct API calls as fallback
            else if (e.Url.Contains("/api/"))
            {
                e.Cancel = true;

                // Extract the API path from the URL
                var apiPath = ExtractApiPath(e.Url);
                if (!string.IsNullOrEmpty(apiPath))
                {
                    await HandleApiRequest(apiPath);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnWebViewNavigating: {ex.Message}");
            Console.WriteLine($"Handling API request: {ex.Message}");
        }
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        Debug.WriteLine($"WebView navigated to: {e.Url}");
    }

    private string ExtractApiPath(string url)
    {
        try
        {
            // Handle various URL formats that might contain API paths
            if (url.Contains("/api/config"))
            {
                return "/api/config";
            }

            if (url.Contains("/api/files"))
            {
                return "/api/files";
            }

            // Generic extraction for other API paths
            var apiIndex = url.IndexOf("/api/", StringComparison.Ordinal);
            if (apiIndex >= 0)
            {
                var apiPath = url.Substring(apiIndex);
                // Remove any query parameters or fragments
                var queryIndex = apiPath.IndexOf('?');
                if (queryIndex >= 0)
                {
                    apiPath = apiPath.Substring(0, queryIndex);
                }

                var fragmentIndex = apiPath.IndexOf('#');
                if (fragmentIndex >= 0)
                {
                    apiPath = apiPath.Substring(0, fragmentIndex);
                }

                return apiPath;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error extracting API path from {url}: {ex.Message}");
            return string.Empty;
        }
    }

    private async Task HandleApiRequest(string url)
    {
        Debug.WriteLine($"Handling API request: {url}");
        Console.WriteLine($"Handling API request: {url}");

        try
        {
            string jsonResponse;

            if (url == "/api/config")
            {
                Debug.WriteLine("Processing /api/config request");
                var config = slideshowService.Config;
                jsonResponse = JsonSerializer.Serialize(
                    config,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    }
                );
                Debug.WriteLine($"Config response: {jsonResponse}");
            }
            else if (url == "/api/files")
            {
                Debug.WriteLine("Processing /api/files request");
                var files = await slideshowService.GetMediaFilesAsync();
                jsonResponse = JsonSerializer.Serialize(
                    files,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    }
                );
                Debug.WriteLine($"Files response: {jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length))}...");
            }
            else
            {
                Debug.WriteLine($"Unknown API endpoint: {url}");
                return;
            }

            // Inject the response into JavaScript
            var script = $"window.handleApiResponse('{url}', {jsonResponse});";

            Debug.WriteLine($"Executing JavaScript: {script.Substring(0, Math.Min(150, script.Length))}...");

            // Execute the script in the WebView
            await SlideshowWebView.EvaluateJavaScriptAsync(script);

            Debug.WriteLine("JavaScript execution completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error handling API request {url}: {ex.Message}\n{ex.StackTrace}");
            Console.WriteLine($"Error handling API request {url}: {ex.Message}");
        }
    }
}