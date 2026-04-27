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

        private readonly BoxView[] _bars;
        private readonly double[] _barHeights = { 10, 20, 30, 16, 36, 22, 40, 18, 32, 14, 26, 10, 20, 34, 12 };
        private readonly Random _rng = new();

        // 0.0 = silence, 1.0 = full speech — decays naturally between events
        private double _speechLevel = 0.0;

        public VoiceRecordingPopup(ISpeechToText speechToText)
        {
            InitializeComponent();
            _speechToText = speechToText;
            _bars = new[] { Bar1, Bar2, Bar3, Bar4, Bar5, Bar6, Bar7, Bar8, Bar9, Bar10, Bar11, Bar12, Bar13, Bar14, Bar15 };
            Opened += OnPopupOpened;
        }

        private async void OnPopupOpened(object? sender, EventArgs e)
        {
            Opened -= OnPopupOpened;
            StartPulseAnimation();
            StartWaveformAnimation();
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
                StatusLabel.Text = ex.Message;
                TranscriptionLabel.TextColor = Color.FromArgb("#EF4444");
            }
        }

        private void OnPartialResult(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs e)
        {
            _speechLevel = 1.0; // speech detected — spike the bars
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
                            PulseRing.ScaleTo(1.3, 800, Easing.SinOut),
                            PulseRing.FadeTo(0, 800, Easing.SinIn)
                        );
                    });
                    await Task.Delay(200);
                }
            });
        }

        private void StartWaveformAnimation()
        {
            _ = Task.Run(async () =>
            {
                while (!_isCancelled)
                {
                    // Decay speech level toward silence each frame (~2s to fully settle)
                    _speechLevel = Math.Max(0.0, _speechLevel - 0.12);

                    var level = _speechLevel;
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        for (int i = 0; i < _bars.Length; i++)
                        {
                            var bar = _bars[i];
                            // Silence: 3–8 px  |  Full speech: 6–42 px
                            double minH = 3.0 + level * 5.0;
                            double maxH = 8.0 + level * 34.0;
                            var target = minH + _rng.NextDouble() * (maxH - minH);
                            var from = bar.HeightRequest;
                            bar.Animate($"bar{i}",
                                new Animation(v => bar.HeightRequest = v, from, target),
                                length: 200, easing: Easing.SinInOut);
                        }
                    });
                    await Task.Delay(250);
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    for (int i = 0; i < _bars.Length; i++)
                        _bars[i].HeightRequest = _barHeights[i];
                });
            });
        }
    }
}
