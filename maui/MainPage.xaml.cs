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

    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

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


    private static void OnWebViewLoaded(object? sender, EventArgs e)
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

    private static void EnableWebViewDebugging()
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

    private static async Task<string> LoadHtmlContent()
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

    private static string GetFallbackHtml()
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

    private static void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        Debug.WriteLine($"WebView navigated to: {e.Url}");
    }

    private static string ExtractApiPath(string url)
    {
        try
        {
            // Handle various URL formats that might contain API paths
            if (url.Contains("/api/config"))
            {
                return "/api/config";
            }

            if (url.Contains("/api/filenames"))
            {
                return "/api/filenames";
            }

            if (url.Contains("/api/fileinfo"))
            {
                // Extract the entire URL with query parameters for fileinfo
                var apiIndex = url.IndexOf("/api/fileinfo", StringComparison.Ordinal);
                if (apiIndex >= 0)
                {
                    return url.Substring(apiIndex);
                }
            }

            // Generic extraction for other API paths
            var genericApiIndex = url.IndexOf("/api/", StringComparison.Ordinal);
            if (genericApiIndex >= 0)
            {
                var apiPath = url.Substring(genericApiIndex);
                // Remove any fragments but keep query parameters
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
                jsonResponse = JsonSerializer.Serialize(
                    slideshowService.Config,
                    jsonSerializerOptions
                );
            }
            else if (url == "/api/filenames")
            {
                var fileNames = slideshowService.GetMediaFileNames();
                jsonResponse = JsonSerializer.Serialize(
                    fileNames,
                    jsonSerializerOptions
                );
            }
            else if (url.StartsWith("/api/fileinfo", StringComparison.Ordinal))
            {
                var uri = new Uri("http://dummy" + url); // Add dummy scheme for Uri parsing
                var query = uri.Query;
                string? fileName = null;

                if (!string.IsNullOrEmpty(query))
                {
                    // Parse query string manually
                    var queryParts = query.TrimStart('?').Split('&');
                    foreach (var part in queryParts)
                    {
                        var keyValue = part.Split('=');
                        if (keyValue.Length == 2 && keyValue[0] == "name")
                        {
                            fileName = Uri.UnescapeDataString(keyValue[1]);
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    Debug.WriteLine("Missing filename parameter");
                    jsonResponse = JsonSerializer.Serialize(
                        new { error = "Missing filename parameter" },
                        jsonSerializerOptions
                    );
                }
                else
                {
                    var fileInfo = await slideshowService.GetMediaFileAsync(fileName);
                    if (fileInfo == null)
                    {
                        Debug.WriteLine($"File not found: {fileName}");
                        jsonResponse = JsonSerializer.Serialize(
                            new { error = "File not found" },
                            jsonSerializerOptions
                        );
                    }
                    else
                    {
                        jsonResponse = JsonSerializer.Serialize(
                            fileInfo,
                            jsonSerializerOptions
                        );
                        Debug.WriteLine($"File info response for {fileName}: {jsonResponse.Substring(0, Math.Min(100, jsonResponse.Length))}...");
                    }
                }
            }
            else
            {
                Debug.WriteLine($"Unknown API endpoint: {url}");
                return;
            }

            // Inject the response into JavaScript
            var script = $"window.handleApiResponse('{url}', {jsonResponse});";
            await SlideshowWebView.EvaluateJavaScriptAsync(script);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error handling API request {url}: {ex.Message}\n{ex.StackTrace}");
            Console.WriteLine($"Error handling API request {url}: {ex.Message}");
        }
    }
}