using BillWise.Models.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace BillWise.Views.Popups
{
    public class IconPickerPopup : Popup<string>
    {
        public IconPickerPopup()
        {
            CanBeDismissedByTappingOutsideOfPopup = true;

            var mainStack = new VerticalStackLayout
            {
                Spacing = 0,
                Padding = new Thickness(0, 0, 0, 16)
            };

            // Handle bar
            mainStack.Children.Add(new BoxView
            {
                WidthRequest = 36, HeightRequest = 4,
                BackgroundColor = Color.FromArgb("#D1D5DB"),
                CornerRadius = 2,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 12, 0, 8)
            });

            // Title
            mainStack.Children.Add(new Label
            {
                Text = "Choose an icon",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#111827"),
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 16)
            });

            // Icon groups inside a ScrollView
            var groupStack = new VerticalStackLayout { Spacing = 12, Padding = new Thickness(16, 0) };

            foreach (var group in IconLibrary.Groups)
            {
                groupStack.Children.Add(new Label
                {
                    Text = group.GroupName,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#6B7280"),
                    Margin = new Thickness(0, 4, 0, 6)
                });

                var flex = new FlexLayout
                {
                    Wrap = FlexWrap.Wrap,
                    JustifyContent = FlexJustify.Start,
                    Direction = FlexDirection.Row
                };

                foreach (var icon in group.Icons)
                {
                    var capturedIcon = icon;

                    var iconLabel = new Label
                    {
                        Text = capturedIcon,
                        FontSize = 22,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    };

                    var border = new Border
                    {
                        BackgroundColor = Color.FromArgb("#F3F4F6"),
                        StrokeThickness = 0,
                        StrokeShape = new RoundRectangle { CornerRadius = 10 },
                        WidthRequest = 50,
                        HeightRequest = 50,
                        Margin = new Thickness(3),
                        Content = iconLabel
                    };

                    var tap = new TapGestureRecognizer();
                    tap.Tapped += async (s, e) => await CloseAsync(capturedIcon);
                    border.GestureRecognizers.Add(tap);

                    flex.Children.Add(border);
                }

                groupStack.Children.Add(flex);
            }

            var scrollView = new ScrollView
            {
                Content = groupStack,
                MaximumHeightRequest = 420
            };

            mainStack.Children.Add(scrollView);

            Content = new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20, 20, 0, 0) },
                Content = mainStack,
                VerticalOptions = LayoutOptions.End
            };
        }
    }
}
