using System.Globalization;
using System.Text.RegularExpressions;
using BillWise.Models.Entities;
using BillWise.Models.Services;
using BillWise.Resources.Strings;
using BillWise.Views.Popups;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.Maui.OCR;

namespace BillWise.ViewModels
{
    public partial class AddInvoiceViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;
        private readonly IOcrService _ocrService;
        private readonly ISpeechToText _speechToText;
        private string _customCategoryIcon = "📄";

        public AddInvoiceViewModel(InvoiceService invoiceService, IOcrService ocrService, ISpeechToText speechToText)
        {
            Title = "New Invoice";
            _invoiceService = invoiceService;
            _ocrService = ocrService;
            _speechToText = speechToText;
            DueDate = DateTime.Today;
        }

        [ObservableProperty] private string _invoiceName = string.Empty;
        [ObservableProperty] private string _amountText = string.Empty;
        [ObservableProperty] private string _notes = string.Empty;
        [ObservableProperty] private string _paymentMethod = "Bank Transfer";
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
                    CategoryType.Electricity  => L["Electricity"],
                    CategoryType.Water        => L["Water"],
                    CategoryType.Internet     => L["Internet"],
                    CategoryType.Rent         => L["Rent"],
                    CategoryType.Subscription => L["Subscription"],
                    _ => string.IsNullOrWhiteSpace(CustomCategoryName)
                            ? L["Other"]
                            : CustomCategoryName
                };
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DueDateText))]
        [NotifyPropertyChangedFor(nameof(DueDateColor))]
        private DateTime _dueDate = DateTime.Today;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DueDateText))]
        [NotifyPropertyChangedFor(nameof(DueDateColor))]
        private bool _isDateSelected = false;

        // Voice recording state
        [ObservableProperty] private bool _isListening = false;
        [ObservableProperty] private string _voiceButtonColor = "#E74C3C";

        public string DueDateText => IsDateSelected
            ? DueDate.ToString("dd / MM / yyyy") : "jj / mm / aaaa";
        public Color DueDateColor => IsDateSelected
            ? Colors.Black : Color.FromArgb("#9CA3AF");

        // ── SCAN ─────────────────────────────────────────────────────
        [RelayCommand]
        public async Task ScanAsync()
        {
            var L = LocalizationResourceManager.Instance;
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlertAsync(L["PermissionTitle"],
                        L["CameraPermissionRequired"], "OK");
                    return;
                }

                var action = await Shell.Current.DisplayActionSheetAsync(
                    L["SelectSource"], L["Cancel"], null,
                    L["TakePhoto"], L["ChooseFromGallery"]);

                FileResult? photo = null;

                if (action == L["TakePhoto"])
                    photo = await MediaPicker.Default.CapturePhotoAsync();
                else if (action == L["ChooseFromGallery"])
                    photo = (await MediaPicker.Default.PickPhotosAsync(
                        new MediaPickerOptions { Title = L["SelectInvoicePhoto"] }))
                        ?.FirstOrDefault();

                if (photo == null) return;

                IsBusy = true;

                await _ocrService.InitAsync();

                // CopyToAsync guarantees all bytes are read (avoids CA2022 partial-read warning)
                using var stream = await photo.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();

                var result = await _ocrService.RecognizeTextAsync(bytes, tryHard: true);

                if (!result.Success || string.IsNullOrWhiteSpace(result.AllText))
                {
                    await Shell.Current.DisplayAlertAsync(L["Scan"],
                        L["ScanNoText"], "OK");
                    return;
                }

                // Split into non-empty lines
                var lines = result.AllText
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToArray();

                // Fill invoice name with the first line if not already set
                if (string.IsNullOrWhiteSpace(InvoiceName) && lines.Length > 0)
                    InvoiceName = lines[0];

                // Try to detect an amount (number with optional spaces/commas/dots)
                if (string.IsNullOrWhiteSpace(AmountText))
                {
                    var match = Regex.Match(result.AllText, @"\b(\d[\d\s.,]{0,12}\d)\b");
                    if (match.Success)
                    {
                        var raw = match.Value.Replace(" ", "").Replace(",", "");
                        if (decimal.TryParse(raw, NumberStyles.Any,
                                CultureInfo.InvariantCulture, out _))
                            AmountText = raw;
                    }
                }

                // Append full OCR text to notes for reference
                Notes = string.IsNullOrWhiteSpace(Notes)
                    ? result.AllText
                    : Notes + "\n" + result.AllText;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"], ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ── VOICE ─────────────────────────────────────────────────────
        [RelayCommand]
        public async Task VoiceAsync()
        {
            var L = LocalizationResourceManager.Instance;
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Microphone>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlertAsync(L["PermissionTitle"],
                        L["MicrophonePermissionRequired"], "OK");
                    return;
                }

                // Open the recording popup — it handles start/stop internally
                var popup = new VoiceRecordingPopup(_speechToText);
                var result = await Shell.Current.ShowPopupAsync(popup, new PopupOptions());

                // User validated: fill the invoice name and try to extract an amount
                if (!result.WasDismissedByTappingOutsideOfPopup &&
                    result is IPopupResult<string> typed &&
                    !string.IsNullOrWhiteSpace(typed.Result))
                {
                    InvoiceName = typed.Result;

                    // Try to detect an amount inside the spoken text
                    if (string.IsNullOrWhiteSpace(AmountText))
                    {
                        var match = Regex.Match(typed.Result, @"\b(\d[\d\s]{0,8}\d|\d+)\b");
                        if (match.Success)
                        {
                            var raw = match.Value.Replace(" ", "");
                            if (decimal.TryParse(raw, NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out _))
                                AmountText = raw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync(
                    LocalizationResourceManager.Instance["ErrorTitle"], ex.Message, "OK");
            }
        }

        // ── LOCATION ──────────────────────────────────────────────────
        [RelayCommand]
        public async Task LocationAsync()
        {
            var L = LocalizationResourceManager.Instance;
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlertAsync(L["PermissionTitle"],
                        L["LocationPermissionRequired"], "OK");
                    return;
                }

                IsBusy = true;

                var location = await Geolocation.GetLocationAsync(
                    new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Medium,
                        Timeout = TimeSpan.FromSeconds(10)
                    });

                if (location == null)
                {
                    await Shell.Current.DisplayAlertAsync(L["Location"],
                        L["CouldNotGetLocation"], "OK");
                    return;
                }

                // Reverse geocode coordinates to a readable address
                var placemarks = await Geocoding.GetPlacemarksAsync(
                    location.Latitude, location.Longitude);

                var place = placemarks?.FirstOrDefault();
                string address;

                if (place != null)
                {
                    address = string.Join(", ", new[]
                    {
                        place.Thoroughfare,
                        place.Locality ?? place.SubLocality,
                        place.CountryName
                    }.Where(s => !string.IsNullOrEmpty(s)));
                }
                else
                {
                    address = $"{location.Latitude:F4}, {location.Longitude:F4}";
                }

                // Append location to notes
                Notes = string.IsNullOrWhiteSpace(Notes)
                    ? $"Location: {address}"
                    : Notes + $"\nLocation: {address}";
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync(
                    LocalizationResourceManager.Instance["ErrorTitle"], ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
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
                { L["Electricity"],  CategoryType.Electricity  },
                { L["Water"],        CategoryType.Water        },
                { L["Internet"],     CategoryType.Internet     },
                { L["Rent"],         CategoryType.Rent         },
                { L["Subscription"], CategoryType.Subscription },
            };

            // Map display label → CustomCategory for quick lookup
            var customMap = custom.ToDictionary(c => $"{c.Icon} {c.Name}");

            var options = builtInMap.Keys.ToList();
            options.AddRange(customMap.Keys);
            options.Add(otherLabel);
            options.Add(addNewLabel);

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

            if (choice == addNewLabel)
            {
                // 1. Pick icon
                var iconResult = await Shell.Current.ShowPopupAsync(new IconPickerPopup(), new PopupOptions());
                var icon = iconResult is IPopupResult<string> r && !string.IsNullOrEmpty(r.Result)
                    ? r.Result : "📄";

                // 2. Enter name
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

            if (choice == otherLabel)
            {
                var name = await Shell.Current.DisplayPromptAsync(
                    L["Other"], L["EnterCategoryName"],
                    "OK", L["Cancel"], maxLength: 30);
                SelectedCategory = CategoryType.Other;
                _customCategoryIcon = "📄";
                CustomCategoryName = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
                return;
            }

            // Saved custom category selected
            if (customMap.TryGetValue(choice, out var saved))
            {
                SelectedCategory = CategoryType.Other;
                CustomCategoryName = saved.Name;
                _customCategoryIcon = saved.Icon;
            }
        }

        [RelayCommand]
        public async Task GoBackAsync() =>
            await Shell.Current.GoToAsync("..");

        [RelayCommand]
        public async Task SaveInvoiceAsync()
        {
            var L = LocalizationResourceManager.Instance;
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(InvoiceName) ||
                string.IsNullOrWhiteSpace(AmountText))
            {
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"],
                    L["FillNameAndAmount"], "OK");
                return;
            }

            if (!decimal.TryParse(AmountText, out decimal amount))
            {
                await Shell.Current.DisplayAlertAsync(L["ErrorTitle"],
                    L["InvalidAmount"], "OK");
                return;
            }

            try
            {
                IsBusy = true;

                // Encode custom category name + icon invisibly in Notes
                const char sep = '';
                var encodedNotes = !string.IsNullOrEmpty(CustomCategoryName)
                    ? $"{sep}{CustomCategoryName}{sep}{_customCategoryIcon}{sep}{Notes}"
                    : Notes;

                var invoice = new Invoice
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = InvoiceName,
                    Amount = amount,
                    DueDate = DueDate,
                    Category = SelectedCategory,
                    Notes = encodedNotes,
                    PaymentMethod = Enum.TryParse<Models.Entities.PaymentMethod>(
                        PaymentMethod?.Replace(" ", ""), true, out var pm)
                        ? pm : Models.Entities.PaymentMethod.BankTransfer,
                    Status = InvoiceStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                var success = await _invoiceService.AddInvoiceAsync(invoice);
                if (success)
                {
                    TriggerHaptic();
                    await Shell.Current.GoToAsync("..");
                }
                else
                    await Shell.Current.DisplayAlertAsync(L["ErrorTitle"],
                        L["FailedSaveInvoice"], "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync(
                    LocalizationResourceManager.Instance["ErrorTitle"], ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static void TriggerHaptic()
        {
            if (!Preferences.Default.Get("haptic_enabled", true)) return;
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(60));
        }
    }
}
