using System.Collections.ObjectModel;
using BillWise.Models.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    public partial class AlertsViewModel : BaseViewModel
    {
        public AlertsViewModel()
        {
            Title = "Notifications";
        }

        public ObservableCollection<AppNotification> Notifications { get; } = new();

        [ObservableProperty]
        private int _unreadCount;

        [RelayCommand]
        public void LoadData()
        {
            if (IsBusy) return;
            IsBusy = true;

            // Example mock data as there is no NotificationService yet
            Notifications.Clear();
            Notifications.Add(new AppNotification
            {
                Title = "Facture en retard",
                Message = "Votre facture CAMWATER est en retard",
                Type = "danger",
                SentAt = DateTime.Now.AddHours(-1)
            });
            Notifications.Add(new AppNotification
            {
                Title = "Facture à payer bientôt",
                Message = "Votre facture Loyer Mensuel expire dans 1 jour",
                Type = "warning",
                SentAt = DateTime.Now.AddHours(-2)
            });

            UnreadCount = Notifications.Count(n => !n.IsRead);

            IsBusy = false;
        }
    }
}
