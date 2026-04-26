using System.Collections.ObjectModel;
using BillWise.Models.Entities;
using BillWise.Models.Services;
using BillWise.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class AlertsViewModel : BaseViewModel
    {
        private readonly NotificationService _notificationService;

        public AlertsViewModel(NotificationService notificationService)
        {
            Title = "Notifications";
            _notificationService = notificationService;
        }

        public ObservableCollection<AppNotification> Notifications { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UnreadMessage))]
        [NotifyPropertyChangedFor(nameof(HasUnread))]
        private int _unreadCount;

        public bool HasUnread => UnreadCount > 0;

        public string UnreadMessage =>
            string.Format(LocalizationResourceManager.Instance["UnreadNotificationsMessage"], UnreadCount);

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var items = await _notificationService.GetNotificationsAsync();
                Notifications.Clear();
                foreach (var n in items)
                    Notifications.Add(n);

                UnreadCount = Notifications.Count(n => !n.IsRead);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notifications: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task MarkAsReadAsync(AppNotification notification)
        {
            if (notification.IsRead) return;

            _notificationService.MarkAsRead(notification.Id);
            await LoadDataAsync();
        }

        [RelayCommand]
        public async Task MarkAllAsReadAsync()
        {
            _notificationService.MarkAllAsRead(Notifications.Select(n => n.Id));
            await LoadDataAsync();
        }

        [RelayCommand]
        public async Task GoBackAsync() =>
            await Shell.Current.GoToAsync("..");
    }
}
