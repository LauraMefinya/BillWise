using BillWise.Models.Entities;
using BillWise.Models.Services;
using BillWise.Resources.Strings;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BillWise.ViewModels
{
    [QueryProperty(nameof(Invoice), "Invoice")]
    public partial class EditInvoiceViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;

        public EditInvoiceViewModel(InvoiceService invoiceService)
        {
            Title = "Edit Invoice";
            _invoiceService = invoiceService;
        }

        private string _customCategoryIcon = "📄";

        private Invoice? _invoice;
        public Invoice? Invoice
        {
            get => _invoice;
            set
            {
                if (SetProperty(ref _invoice, value) && _invoice != null)
                    LoadInvoiceData();
            }
        }

        [ObservableProperty] private string _invoiceName = string.Empty;
        [ObservableProperty] private string _amountText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DueDateText))]
        [NotifyPropertyChangedFor(nameof(DueDateColor))]
        private DateTime _dueDate = DateTime.Today;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DueDateText))]
        [NotifyPropertyChangedFor(nameof(DueDateColor))]
        private bool _isDateSelected = true;

        public string DueDateText => IsDateSelected ? DueDate.ToString("dd / MM / yyyy") : "jj / mm / aaaa";
        public Color DueDateColor => IsDateSelected ? Colors.Black : Color.FromArgb("#9CA3AF");

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedCategoryLabel))]
        private CategoryType _selectedCategory = CategoryType.Other;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedCategoryLabel))]
        private string _customCategoryName = string.Empty;

        public string SelectedCategoryLabel
        {
            get
            {
                var L = LocalizationResourceManager.Instance;
                return SelectedCategory switch
                {
                    CategoryType.Electricity  => $"⚡ {L["Electricity"]}",
                    CategoryType.Water        => $"💧 {L["Water"]}",
                    CategoryType.Internet     => $"🌐 {L["Internet"]}",
                    CategoryType.Rent         => $"🏠 {L["Rent"]}",
                    CategoryType.Subscription => $"📺 {L["Subscription"]}",
                    _ => string.IsNullOrWhiteSpace(CustomCategoryName)
                            ? $"📄 {L["Other"]}"
                            : $"{_customCategoryIcon} {CustomCategoryName}"
                };
            }
        }
        [ObservableProperty] private string _paymentMethod = "Bank Transfer";
        [ObservableProperty] private string _notes = string.Empty;

        private void LoadInvoiceData()
        {
            if (Invoice == null) return;

            InvoiceName = Invoice.Name;
            AmountText = Invoice.Amount.ToString();
            DueDate = Invoice.DueDate.Date;
            SelectedCategory = Invoice.Category;

            // Decode custom category metadata stored in Notes
            const char sep = '';
            var rawNotes = Invoice.Notes ?? string.Empty;
            if (rawNotes.Length > 0 && rawNotes[0] == sep)
            {
                var parts = rawNotes.Split(sep, 4);
                CustomCategoryName = parts.Length >= 2 ? parts[1] : string.Empty;
                _customCategoryIcon = parts.Length >= 3 ? parts[2] : "📄";
                Notes = parts.Length >= 4 ? parts[3] : string.Empty;
            }
            else
            {
                CustomCategoryName = string.Empty;
                _customCategoryIcon = "📄";
                Notes = rawNotes;
            }

            PaymentMethod = Invoice.PaymentMethod switch
            {
                Models.Entities.PaymentMethod.PayPal       => "PayPal",
                Models.Entities.PaymentMethod.GooglePay    => "Google Pay",
                Models.Entities.PaymentMethod.Cash         => "Cash",
                Models.Entities.PaymentMethod.CardPayment  => "Card Payment",
                Models.Entities.PaymentMethod.DirectDebit  => "Direct Debit",
                _                                          => "Bank Transfer"
            };
        }

        [RelayCommand]
        public async Task OpenCategoryPickerAsync()
        {
            var L = LocalizationResourceManager.Instance;
            var custom = CategoryService.GetCustomCategories();

            var addNewLabel = L["AddNewCategory"];
            var otherLabel  = L["Other"];

            var builtInMap = new Dictionary<string, CategoryType>
            {
                { $"⚡ {L["Electricity"]}",  CategoryType.Electricity  },
                { $"💧 {L["Water"]}",        CategoryType.Water        },
                { $"🌐 {L["Internet"]}",     CategoryType.Internet     },
                { $"🏠 {L["Rent"]}",         CategoryType.Rent         },
                { $"📺 {L["Subscription"]}", CategoryType.Subscription },
            };

            var customMap = custom.ToDictionary(c => $"{c.Icon} {c.Name}");

            var options = builtInMap.Keys.ToList();
            options.AddRange(customMap.Keys);
            options.Add($"📄 {otherLabel}");
            options.Add($"➕ {addNewLabel}");

            string? choice = await Shell.Current.DisplayActionSheetAsync(
                L["SelectCategory"], L["Cancel"], null, options.ToArray());

            if (string.IsNullOrEmpty(choice) || choice == L["Cancel"]) return;

            if (builtInMap.TryGetValue(choice, out var cat))
            {
                SelectedCategory = cat;
                CustomCategoryName = string.Empty;
                _customCategoryIcon = "📄";
                return;
            }

            if (choice == $"➕ {addNewLabel}")
            {
                var iconResult = await Shell.Current.ShowPopupAsync(
                    new BillWise.Views.Popups.IconPickerPopup(), new PopupOptions());
                var icon = iconResult is IPopupResult<string> r && !string.IsNullOrEmpty(r.Result)
                    ? r.Result : "📄";

                var name = await Shell.Current.DisplayPromptAsync(
                    L["AddNewCategory"], L["EnterCategoryName"],
                    L["Save"], L["Cancel"], maxLength: 30);
                if (string.IsNullOrWhiteSpace(name)) return;
                name = name.Trim();
                CategoryService.SaveCustomCategory(name, icon);
                _customCategoryIcon = icon;
                SelectedCategory = CategoryType.Other;
                CustomCategoryName = name;
                return;
            }

            if (choice == $"📄 {otherLabel}")
            {
                var name = await Shell.Current.DisplayPromptAsync(
                    L["Other"], L["EnterCategoryName"],
                    "OK", L["Cancel"], maxLength: 30);
                SelectedCategory = CategoryType.Other;
                _customCategoryIcon = "📄";
                CustomCategoryName = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
                return;
            }

            if (customMap.TryGetValue(choice, out var saved))
            {
                SelectedCategory = CategoryType.Other;
                CustomCategoryName = saved.Name;
                _customCategoryIcon = saved.Icon;
            }
        }

        [RelayCommand]
        public async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

        [RelayCommand]
        public async Task SaveInvoiceAsync()
        {
            var L = LocalizationResourceManager.Instance;
            if (IsBusy) return;
            if (Invoice == null) return;

            if (string.IsNullOrWhiteSpace(InvoiceName) || string.IsNullOrWhiteSpace(AmountText))
            {
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], L["FillNameAndAmount"], "OK");
                return;
            }

            if (!decimal.TryParse(AmountText, out decimal amount))
            {
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], L["InvalidAmount"], "OK");
                return;
            }

            try
            {
                IsBusy = true;

                const char sep = '';
                var encodedNotes = !string.IsNullOrEmpty(CustomCategoryName)
                    ? $"{sep}{CustomCategoryName}{sep}{_customCategoryIcon}{sep}{Notes}"
                    : Notes;

                Invoice.Name = InvoiceName;
                Invoice.Amount = amount;
                Invoice.DueDate = DateTime.SpecifyKind(DueDate.Date, DateTimeKind.Utc);
                Invoice.Category = SelectedCategory;
                Invoice.Notes = encodedNotes;
                Invoice.PaymentMethod = Enum.TryParse<Models.Entities.PaymentMethod>(
                    PaymentMethod?.Replace(" ", ""), true, out var pm)
                    ? pm : Models.Entities.PaymentMethod.BankTransfer;

                var success = await _invoiceService.UpdateInvoiceAsync(Invoice);
                if (success)
                {
                    var parameters = new Dictionary<string, object> { { "Invoice", Invoice } };
                    await Shell.Current.GoToAsync("..", parameters);
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], L["FailedUpdateInvoice"], "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Shell.Current.DisplayAlertAsync(
                    LocalizationResourceManager.Instance["ErrorTitle"], ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
