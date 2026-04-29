using Foundation;
using BillWise.Models.Services;
using UIKit;

namespace BillWise;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
	{
		var urlString = url.AbsoluteString;
		if (!string.IsNullOrEmpty(urlString) && urlString.StartsWith("billwise://"))
			DeepLinkService.QueueUrl(urlString);
		return true;
	}
}
