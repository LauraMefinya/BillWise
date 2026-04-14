using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;

namespace BillWise
{
    [Activity(
        Theme = "@style/Maui.MainTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges =
            ConfigChanges.ScreenSize |
            ConfigChanges.Orientation |
            ConfigChanges.UiMode |
            ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize |
            ConfigChanges.Density
    )]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            WindowCompat.SetDecorFitsSystemWindows(Window!, false);
            Window?.DecorView?.Post(CollapseShellAppBar);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Window?.DecorView?.Post(CollapseShellAppBar);
        }

        private void CollapseShellAppBar()
        {
            if (Window?.DecorView is ViewGroup root)
                CollapseAppBarInView(root);
        }

        // Walk the view tree and force any AppBarLayout to Gone so it
        // takes no space even when the MAUI Shell toolbar is "hidden".
        private static void CollapseAppBarInView(ViewGroup group)
        {
            for (int i = 0; i < group.ChildCount; i++)
            {
                var child = group.GetChildAt(i);
                if (child is null) continue;

                if (child.Class?.SimpleName?.Contains("AppBarLayout") == true)
                    child.Visibility = ViewStates.Gone;

                if (child is ViewGroup vg)
                    CollapseAppBarInView(vg);
            }
        }
    }
}