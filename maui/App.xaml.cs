#if MACCATALYST
using UIKit;
#endif

namespace SlideshowApp;

public sealed partial class App
{
	public App()
	{
		InitializeComponent();

		this.MainPage = new AppShell();
	}
}
