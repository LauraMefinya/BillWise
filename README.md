# BillWise 

BillWise is a smart, cross-platform personal finance and invoice management application built with [.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui). It helps users track, manage, and analyze their bills and invoices seamlessly across Android, Windows, and iOS devices.

---

##  Core Features

- **Invoice Management**: Complete CRUD operations for invoices. Easily track statuses (Pending, Paid, Overdue).
- **Cloud Sync & Authentication**: Powered by [Supabase](https://supabase.com/) for real-time database synchronization and secure user authentication (login, registration, reset password).
- **Smart OCR Capabilities**: Automatically extract text and data from physical invoices using device cameras.
- **Statistics & Insights**: Interactive and visually appealing charts using `LiveChartsCore` to analyze expenses over time.
- **PDF Export**: Generate and export your invoices to a clean PDF format.
- **Smart Reminders**: Local device notifications to ensure you never miss a payment deadline.
- **Multi-language Support**: Easily switch between languages with a custom `LocalizationResourceManager`.
- **Modern Adaptive UI**: Full support for both Light and Dark themes, utilizing a highly structured MVVM architecture and customized borderless UI components.

##  Technology Stack

- **Framework**: .NET MAUI (Multi-platform App UI) targeting .NET 10.0 (as configured in `.csproj`)
- **Architecture**: MVVM (Model-View-ViewModel) via `CommunityToolkit.Mvvm`
- **Backend / BaaS**: Supabase (PostgreSQL, GoTrue Auth)
- **Local Storage**: `sqlite-net-pcl` for local caching and offline capabilities.
- **Key NuGet Packages**:
  - `CommunityToolkit.Maui` & `CommunityToolkit.Mvvm` (UI & Architecture)
  - `supabase-csharp` (Database & Authentication)
  - `LiveChartsCore.SkiaSharpView.Maui` (Data Analytics / Charts)
  - `Plugin.Maui.OCR` (Optical Character Recognition)
  - `PdfSharpCore` (PDF Generation)
  - `Plugin.LocalNotification` (Local Alerts)
  - `Microsoft.CognitiveServices.Speech` (Speech-to-Text / Voice capabilities)

##  Project Structure

- **`Models/`**: Contains core data entities (`Invoice`, `UserProfile`, `Category`, `Payment`) that map directly to the Supabase database.
- **`Models/Services/`**: Contains application logic services (`AuthService`, `SessionService`, `InvoiceService`, `PdfExportService`, `NotificationService`).
- **`ViewModels/`**: Contains the business logic and state management for each screen, strictly following the MVVM pattern (e.g., `HomeViewModel`, `AddInvoiceViewModel`).
- **`Views/`**: The XAML-based UI pages containing the presentation layer (`HomePage`, `InvoicesPage`, `StatisticsPage`).
- **`Resources/`**: Contains images, vector icons (`.svg`), fonts, splash screens, and translation string dictionaries.

---

##  Getting Started

###  Prerequisites
Before running the application, ensure you have the following installed:
- **IDE**: Visual Studio 2022 (with the **.NET Multi-platform App UI development** workload installed) OR Visual Studio Code with the .NET MAUI extension.
- **SDK**: .NET 8.0 SDK or later.
- **Emulators**: Android SDK and an Android Emulator configured, or Developer Mode enabled for Windows deployment.

###  Configuration & Secrets
The application is pre-configured to connect to a Supabase backend.
- The connection strings (`url` and `key`) are initialized in **`MauiProgram.cs`**.
- If you intend to use your own Supabase project, you must replace these values with your own Project URL and `anon`/`public` key.
- **Note**: Ensure your Supabase database has the correct tables (e.g., `invoices`, `user_profiles`) with Row Level Security (RLS) policies configured to match the application's models.

###  Running the Application
1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/BillWise.git
   ```
2. **Open the Solution**: Double-click `BillWise.sln` to open it in Visual Studio.
3. **Restore Packages**: NuGet packages should restore automatically. If not, right-click the solution in the Solution Explorer and select **"Restore NuGet Packages"**.
4. **Select Target Device**:
   - For **Android**: Select an Android Emulator from the drop-down run menu, or plug in a physical device with USB Debugging enabled.
   - For **Windows**: Select "Windows Machine". Note: You may need to enable Developer Mode in your Windows settings.
5. **Launch**: Press **F5** or click the **Run** button to compile and launch the app.

---

## Troubleshooting & Known Issues

If you encounter issues while running or building the app, try these common fixes:

- **Stale Builds / XAML Errors**: .NET MAUI occasionally caches old build artifacts. If you see unexplainable XAML errors, manually delete the `bin/` and `obj/` folders inside the project directory, then Clean and Rebuild the solution.
- **Notifications Not Firing (Android)**: Ensure the application has the `POST_NOTIFICATIONS` permission granted in the device settings if running on Android 13 (API 33) or higher. The `Plugin.LocalNotification` handles this, but emulator states can sometimes block it.
- **Android UI Glitches**: The project uses custom Handlers in `MauiProgram.cs` to remove underlines from native Android controls (like `Entry` and `Picker`). If elements appear transparent, verify your Android version supports `ColorStateList.ValueOf(Transparent)`.
- **Authentication Failures**: The app requires an active internet connection to communicate with Supabase. If login or registration fails, verify your network connection and ensure the Supabase keys in `MauiProgram.cs` are active.
