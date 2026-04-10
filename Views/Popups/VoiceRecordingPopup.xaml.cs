using BillWise.Resources.Strings;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Maui.Views;
using System.Globalization;

namespace BillWise.Views.Popups
{
    public partial class VoiceRecordingPopup : Popup<string>
    {
        private readonly ISpeechToText _speechToText;
        private string _currentText = string.Empty;
        private bool _isCancelled = false;

        public VoiceRecordingPopup(ISpeechToText speechToText)
        {
            InitializeComponent();
            _speechToText = speechToText;
            Opened += OnPopupOpened;
        }

        private async void OnPopupOpened(object? sender, EventArgs e)
        {
            Opened -= OnPopupOpened;
            StartPulseAnimation();
            await StartListeningAsync();
        }

        private async Task StartListeningAsync()
        {
            try
            {
                _speechToText.RecognitionResultUpdated += OnPartialResult;
                _speechToText.RecognitionResultCompleted += OnFinalResult;

                await _speechToText.StartListenAsync(
                    new SpeechToTextOptions { Culture = CultureInfo.CurrentUICulture });
            }
            catch (Exception ex)
            {
                StatusLabel.Text = LocalizationResourceManager.Instance["VoiceError"];
                TranscriptionLabel.Text = ex.Message;
                TranscriptionLabel.TextColor = Color.FromArgb("#EF4444");
            }
        }

        private void OnPartialResult(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _currentText = e.RecognitionResult;
                UpdateTranscriptionUI(_currentText);
            });
        }

        private void OnFinalResult(object? sender, SpeechToTextRecognitionResultCompletedEventArgs e)
        {
            _speechToText.RecognitionResultUpdated -= OnPartialResult;
            _speechToText.RecognitionResultCompleted -= OnFinalResult;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (e.RecognitionResult.IsSuccessful && !string.IsNullOrWhiteSpace(e.RecognitionResult.Text))
                {
                    _currentText = e.RecognitionResult.Text;
                    UpdateTranscriptionUI(_currentText);
                    StatusLabel.Text = LocalizationResourceManager.Instance["VoiceDone"];
                }
                else if (!_isCancelled)
                {
                    StatusLabel.Text = LocalizationResourceManager.Instance["VoiceError"];
                    TranscriptionLabel.TextColor = Color.FromArgb("#EF4444");
                }
            });
        }

        private void UpdateTranscriptionUI(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            TranscriptionLabel.Text = text;
            TranscriptionLabel.TextColor = Color.FromArgb("#111827");
            ValidateButton.IsEnabled = true;
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            _isCancelled = true;
            await StopListening();
            await CloseAsync(string.Empty);
        }

        private async void OnValidateClicked(object? sender, EventArgs e)
        {
            await StopListening();
            await CloseAsync(_currentText);
        }

        private async Task StopListening()
        {
            try
            {
                _speechToText.RecognitionResultUpdated -= OnPartialResult;
                _speechToText.RecognitionResultCompleted -= OnFinalResult;
                await _speechToText.StopListenAsync();
            }
            catch { }
        }

        // Pulse animation: ring expands and fades in a loop while listening
        private void StartPulseAnimation()
        {
            _ = Task.Run(async () =>
            {
                while (!_isCancelled)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        PulseRing.Opacity = 0.8;
                        PulseRing.Scale = 0.7;
                        await Task.WhenAll(
                            PulseRing.ScaleTo(1.2, 800, Easing.SinOut),
                            PulseRing.FadeTo(0, 800, Easing.SinIn)
                        );
                    });
                    await Task.Delay(200);
                }
            });
        }
    }
}
