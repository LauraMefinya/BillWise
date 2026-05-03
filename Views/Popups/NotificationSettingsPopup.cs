using BillWise.ViewModels;
using BillWise.Resources.Strings;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace BillWise.Views.Popups
{
    public class NotificationSettingsPopup : Popup
    {
        public NotificationSettingsPopup(ProfileViewModel viewModel)
        {
            BindingContext = viewModel;

            CanBeDismissedByTappingOutsideOfPopup = true;

            var mainStack = new VerticalStackLayout
            {
                Spacing = 0,
                Padding = new Thickness(0, 0, 0, 32)
            };

            // Handle bar
            mainStack.Children.Add(new BoxView
            {
                WidthRequest = 36, HeightRequest = 4,
                BackgroundColor = Color.FromArgb("#D1D5DB"),
                CornerRadius = 2,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 12, 0, 16)
            });

            // Title
            var titleLabel = new Label
            {
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#111827"),
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 24)
            };
            titleLabel.SetBinding(Label.TextProperty, new Binding("[Notifications]", source: LocalizationResourceManager.Instance));
            mainStack.Children.Add(titleLabel);

            var contentStack = new VerticalStackLayout { Spacing = 16, Padding = new Thickness(20, 0) };

            // Enable Notifications Grid
            var notifGrid = new Grid 
            { 
                ColumnDefinitions = new ColumnDefinitionCollection 
                { 
                    new ColumnDefinition { Width = GridLength.Auto }, 
                    new ColumnDefinition { Width = GridLength.Star }, 
                    new ColumnDefinition { Width = GridLength.Auto } 
                }, 
                Padding = new Thickness(12) 
            };
            
            var iconBorder1 = new Border { BackgroundColor = Color.FromArgb("#FFEDD5"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 10 }, WidthRequest = 40, HeightRequest = 40, VerticalOptions = LayoutOptions.Center };
            iconBorder1.Content = new Label { Text = "🔔", FontSize = 17, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            notifGrid.Children.Add(iconBorder1);
            Grid.SetColumn(iconBorder1, 0);

            var notifLabel = new Label { TextColor = Color.FromArgb("#374151"), FontSize = 15, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(14, 0, 0, 0) };
            notifLabel.SetBinding(Label.TextProperty, new Binding("[Notifications]", source: LocalizationResourceManager.Instance));
            notifGrid.Children.Add(notifLabel);
            Grid.SetColumn(notifLabel, 1);

            var notifSwitch = new Switch { OnColor = Color.FromArgb("#3498DB"), VerticalOptions = LayoutOptions.Center };
            notifSwitch.SetBinding(Switch.IsToggledProperty, nameof(ProfileViewModel.NotificationsEnabled));
            notifGrid.Children.Add(notifSwitch);
            Grid.SetColumn(notifSwitch, 2);

            contentStack.Children.Add(notifGrid);

            // Separator
            contentStack.Children.Add(new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#F3F4F6") });

            // Reminder Days Grid
            var reminderGrid = new Grid 
            { 
                ColumnDefinitions = new ColumnDefinitionCollection 
                { 
                    new ColumnDefinition { Width = GridLength.Auto }, 
                    new ColumnDefinition { Width = GridLength.Star }, 
                    new ColumnDefinition { Width = GridLength.Auto } 
                }, 
                Padding = new Thickness(12) 
            };
            
            var iconBorder2 = new Border { BackgroundColor = Color.FromArgb("#FFFBEB"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 10 }, WidthRequest = 40, HeightRequest = 40, VerticalOptions = LayoutOptions.Center };
            iconBorder2.Content = new Label { Text = "⏳", FontSize = 17, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            reminderGrid.Children.Add(iconBorder2);
            Grid.SetColumn(iconBorder2, 0);

            var reminderLabel = new Label { TextColor = Color.FromArgb("#374151"), FontSize = 15, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(14, 0, 0, 0) };
            reminderLabel.SetBinding(Label.TextProperty, new Binding("[ReminderDays]", source: LocalizationResourceManager.Instance));
            reminderGrid.Children.Add(reminderLabel);
            Grid.SetColumn(reminderLabel, 1);

            var entryBorder = new Border { Stroke = Color.FromArgb("#E5E7EB"), StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 8 }, BackgroundColor = Colors.White, WidthRequest = 70, HeightRequest = 46, VerticalOptions = LayoutOptions.Center };
            var reminderEntry = new Entry { Keyboard = Keyboard.Numeric, BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#111827"), HorizontalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center };
            reminderEntry.SetBinding(Entry.TextProperty, nameof(ProfileViewModel.ReminderDays));
            reminderEntry.SetBinding(Entry.IsEnabledProperty, nameof(ProfileViewModel.NotificationsEnabled));
            entryBorder.Content = reminderEntry;
            reminderGrid.Children.Add(entryBorder);
            Grid.SetColumn(entryBorder, 2);

            // DataTrigger for Opacity
            var trigger = new DataTrigger(typeof(Grid))
            {
                Binding = new Binding(nameof(ProfileViewModel.NotificationsEnabled)),
                Value = false
            };
            trigger.Setters.Add(new Setter { Property = Grid.OpacityProperty, Value = 0.4 });

            var triggerTrue = new DataTrigger(typeof(Grid))
            {
                Binding = new Binding(nameof(ProfileViewModel.NotificationsEnabled)),
                Value = true
            };
            triggerTrue.Setters.Add(new Setter { Property = Grid.OpacityProperty, Value = 1.0 });

            reminderGrid.Triggers.Add(trigger);
            reminderGrid.Triggers.Add(triggerTrue);

            contentStack.Children.Add(reminderGrid);

            // OK Button
            var okButton = new Button { Text = "OK", BackgroundColor = Color.FromArgb("#3498DB"), TextColor = Colors.White, FontAttributes = FontAttributes.Bold, CornerRadius = 8, Margin = new Thickness(0, 20, 0, 0) };
            okButton.Clicked += async (s, e) => await CloseAsync();
            contentStack.Children.Add(okButton);

            mainStack.Children.Add(contentStack);

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
