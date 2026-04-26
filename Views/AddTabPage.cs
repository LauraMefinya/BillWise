namespace BillWise.Views;

public partial class AddTabPage : ContentPage
{
    private bool _isNavigatingToAdd = false;

    public AddTabPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        try
        {
            if (!_isNavigatingToAdd)
            {
                _isNavigatingToAdd = true;
                await Shell.Current.GoToAsync(nameof(AddInvoicePage));
            }
            else
            {
                _isNavigatingToAdd = false;
                await Shell.Current.GoToAsync("//HomePage");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AddTabPage] Navigation error: {ex.Message}");
            _isNavigatingToAdd = false;
        }
    }
}
