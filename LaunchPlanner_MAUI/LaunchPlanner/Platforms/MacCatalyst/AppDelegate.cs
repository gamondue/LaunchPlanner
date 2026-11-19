using Foundation;
using LaunchPlanner.Services;
using UIKit;
using UserNotifications;

namespace LaunchPlanner
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        // Va impostato prima della fine dell'avvio per ricevere il tocco che ha aperto l'app
        public override bool WillFinishLaunching(UIApplication application, NSDictionary? launchOptions)
        {
            UNUserNotificationCenter.Current.Delegate = new NotificationDelegate();
            return base.WillFinishLaunching(application, launchOptions);
        }
    }
}
