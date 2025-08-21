#if MACCATALYST
using UIKit;
#endif

namespace SlideshowApp;

public partial class App : Application
{
	public App()
	{
		Console.WriteLine("App constructor called");
		InitializeComponent();
		Console.WriteLine("App InitializeComponent completed");
		
		// Create services and main page directly
		var slideshowService = new SlideshowService();
		MainPage = new MainPage(slideshowService);
		Console.WriteLine("App constructor completed");
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		Console.WriteLine("CreateWindow called");
		var window = base.CreateWindow(activationState);
		
		// Set window properties to ensure it's visible
		window.Title = "Slideshow App";
		window.Width = 1024;
		window.Height = 768;
		
		Console.WriteLine($"Window created: {window.Title}");
		return window;
	}
}
