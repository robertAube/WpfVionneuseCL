using Microsoft.Win32;
using MirzaMediaPlayer.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MirzaMediaPlayer {

    public partial class MainWindow : Window {
        internal static AppConfig AppConfig { get; private set; }
        public MainWindow() {
            InitializeComponent();
            AppConfig = new AppConfig();

            _playListContainer = TryFindResource("playListContainer") as PlayListContainer;
            setVideoDepart();

            this.Loaded += MainWindow_Loaded;

            sliderDuration.AddHandler(MouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(Slider_MouseLeftButtonDown), true);
        }

        private void setVideoDepart() {
            GestionVideo gv = new GestionVideo(_playListContainer);
        }

        #region private properties
        private TimeSpan _totalTimer, _progressTimer;
        private bool _isDragging = false;  // pour ne pas écraser le drag par le timer

        private DispatcherTimer _timer;
        private PlayListContainer _playListContainer;
        private Uri _playUri = new Uri(@"Icons\Play.png", UriKind.Relative);
        private Uri _pauseUri = new Uri(@"Icons\Pause.png", UriKind.Relative);
        private int _currentSelectedIndex = 0;
        private bool _isPaused = false;
        private bool _mediaCanSeek = false;
        private string _currentlyPlayedFileName = "";

        //variables de gestion du slider
        private bool _mouseOneSlider = false;
        private int slowInt = 0;
        private static readonly int SLOW_INT_MAX = 10;
        private static readonly int TIMER_INTERVAL = 50;
        #endregion

        #region private methods
        private void _timer_Tick(object sender, EventArgs e) {
            //_progressTimer = mediaElementMain.Position;
            //if (_progressTimer.TotalSeconds <= _totalTimer.TotalSeconds) {
            //    sliderDuration.Value = _progressTimer.TotalSeconds;
            //    textBlockProgress.Text = string.Format("{0:hh\\:mm\\:ss}", _progressTimer);
            //}

            // Met à jour la barre seulement si on n’est pas en train de la déplacer
            var pos = mediaElementMain.Position;
            textBlockProgress.Text = FormatTime(pos);
            //            if (!_isDragging && !_mouseOneSlider && mediaElementMain.NaturalDuration.HasTimeSpan) {
            if (mediaElementMain.NaturalDuration.HasTimeSpan) {
                if (_mouseOneSlider) {
                    slowInt++;
                    if (slowInt > SLOW_INT_MAX) {
                        sliderDuration.Value = pos.TotalMilliseconds;
                        slowInt = 0;
                    }
                }
                else if (!_isDragging) {
                    sliderDuration.Value = pos.TotalMilliseconds;
                    slowInt = 0;
                }
            }
        }
        private Task<bool> DetectTimespan() {
            bool hasTimespan = false;
            while (true) {
                if (mediaElementMain.NaturalDuration.HasTimeSpan) {
                    hasTimespan = true;
                    break;
                }
            }
            return Task.FromResult(hasTimespan);
        }
        private async void PlayMedia(PlayList mediaInfo) {
            string fileName;
            try {
                if (!_isPaused && mediaInfo != null) {
                    _currentlyPlayedFileName = mediaInfo.FullName;
                    mediaElementMain.Source = new Uri(mediaInfo.FullName, UriKind.Absolute);
                    sliderDuration.Value = 0;
                }
                if (!sliderDuration.IsEnabled)
                    sliderDuration.IsEnabled = true;
                mediaElementMain.Play();
                if (await DetectTimespan()) {
                    _timer.Start();

                    _totalTimer = mediaElementMain.NaturalDuration.TimeSpan;
                    _mediaCanSeek = mediaElementMain.CanSeek();
                    sliderDuration.Maximum = _totalTimer.TotalMilliseconds;
                    if (!mediaElementMain.HasVideo) {
                        imageAudio.Visibility = Visibility.Visible;
                    }
                    else if (mediaElementMain.HasVideo) {
                        imageAudio.Visibility = Visibility.Hidden;
                    }
                    textBlockDuration.Text = string.Format("{0:hh\\:mm\\:ss}",
                        mediaElementMain.NaturalDuration.TimeSpan);
                    fileName = (mediaInfo == null) ? "" : mediaInfo.Name;
                    textBlockMediaStatus.Text = $"Playing {fileName}";
                    ellipseStatus.Fill = Brushes.Lime;
                }
            }
            catch (Exception ex) {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }



        //private void Player_MediaOpened(object sender, RoutedEventArgs e) {
        //    if (Player.NaturalDuration.HasTimeSpan) {
        //        SeekBar.Maximum = Player.NaturalDuration.TimeSpan.TotalMilliseconds;
        //        SeekBar.IsEnabled = true;
        //        _timer.Start();
        //    }
        //    else {
        //        SeekBar.IsEnabled = false; // flux non seekable
        //    }
        //}



        private void PauseMedia() {
            if (mediaElementMain.CanPause) {
                try {

                    mediaElementMain.Pause();

                    if (mediaElementMain.NaturalDuration.HasTimeSpan) {
                        _timer.IsEnabled = false;
                        _timer.Stop();
                    }
                    ellipseStatus.Fill = Brushes.RoyalBlue;
                    textBlockMediaStatus.Text = $"Paused";
                }
                catch (Exception ex) {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
        private async void StopMedia() {
            try {

                mediaElementMain.Stop();
                _currentlyPlayedFileName = "";
                if (await DetectTimespan()) {
                    _timer.IsEnabled = false;
                    _timer.Stop();
                }
                sliderDuration.IsEnabled = false;
                mediaElementMain.Position = TimeSpan.FromSeconds(0);
                sliderDuration.Value = 0;
                ellipseStatus.Fill = Brushes.Gray;
                textBlockProgress.Text = "00:00:00";
                textBlockMediaStatus.Text = $"Stopped";
            }
            catch (Exception ex) {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private PlayList GetNextMediaFileName(bool next = false) {
            PlayList fileName = null;
            //if next equals to false, it'll get the current selected index
            if (next) {
                if (_currentSelectedIndex + 1 < _playListContainer.PlayListData.Count)
                    _currentSelectedIndex++;
                else
                    _currentSelectedIndex = 0;
            }
            fileName = _playListContainer.PlayListData[_currentSelectedIndex];
            listBoxPlaylist.SelectedItem = _currentSelectedIndex;
            return fileName;
        }
        private PlayList GetPrevMediaFileName() {
            PlayList fileName = null;
            if (_currentSelectedIndex - 1 >= 0)
                _currentSelectedIndex--;
            else
                _currentSelectedIndex = _playListContainer.PlayListData.Count - 1;
            fileName = _playListContainer.PlayListData[_currentSelectedIndex];
            listBoxPlaylist.SelectedItem = _currentSelectedIndex;
            return fileName;
        }
        #endregion

        #region main events
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            initTimerInterval(TIMER_INTERVAL);
        }


        private void initTimerInterval(int nbMilliseconde) {
            _timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(nbMilliseconde) // Timer UI: met à jour le slider ~30 fois par seconde
            };

            _timer.Tick += _timer_Tick;
        }

        /*
        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            mediaElementMain.Volume = sliderVolume.Value;
        }

        private void sliderBalance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            mediaElementMain.Balance = sliderBalance.Value;
        }
        */
        private void sliderSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            mediaElementMain.SpeedRatio = sliderSpeed.Value / 4;
        }

        private void mediaElementMain_MediaEnded(object sender, RoutedEventArgs e) {

            _timer.Stop();
            if (_playListContainer.PlayListData.Count > 0) {
                PlayMedia(GetNextMediaFileName(false));
            }
            else
                StopMedia();
        }


        private void lbSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (listBoxPlaylist.SelectedItem != null) {
                manage_ListBoxSelectMedia();

            }
        }

        private void manage_ListBoxSelectMedia() {
            _currentSelectedIndex = listBoxPlaylist.SelectedIndex;
            if (_currentSelectedIndex >= 0) {
                PlayMedia(GetNextMediaFileName());
                BitmapImage image = null;
                _isPaused = false;
                try {
                    image = new BitmapImage(_pauseUri);
                    imagePlayPause.Source = image;
                }
                catch { buttonPlayPause.Content = "Pause (CTRL+P)"; }
                buttonPlayPause.ToolTip = "Pause (CTRL+P)";
            }
            else {

            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            int clickCount = e.ClickCount;
            if (clickCount > 0) {
                manage_ListBoxSelectMedia();
            }
        }

        //private void sliderDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        //    if (mediaElementMain.Source != null) {
        //        if (mediaElementMain.NaturalDuration.HasTimeSpan) {
        //            _progressTimer = TimeSpan.FromSeconds(sliderDuration.Value);
        //            mediaElementMain.Position = _progressTimer;
        //        }
        //    }
        //}


        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_playListContainer.PlayListData.Count > 0) {
                    PlayMedia(GetNextMediaFileName(false));
                }
                // Code à exécuter après affichage complet
                //MessageBox.Show("Exécution après affichage via Dispatcher !");
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        #endregion
        #region seekBar

        private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _isDragging = true;
        }



        private void Slider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _isDragging = true;
        }

        private void Slider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

            // Appliquer le seek à la position voulue
            if (mediaElementMain.NaturalDuration.HasTimeSpan && _mediaCanSeek) {
                var target = TimeSpan.FromMilliseconds(sliderDuration.Value);
                _isDragging = false;
                mediaElementMain.Position = target;
                textBlockProgress.Text = FormatTime(target);
            }
            _isDragging = false;
        }

        //private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
        //    // Pendant le drag, permettre le "scrubbing" visuel fluide si ScrubbingEnabled, sinon on set à la fin
        //    if (_isDragging && mediaElementMain.NaturalDuration.HasTimeSpan) {
        //        // Optionnel: scrubbing en temps réel (peut être coûteux pour certains formats)
        //        //mediaElementMain.Position = TimeSpan.FromMilliseconds(e.NewValue);
        //        textBlockProgress.Text = FormatTime(TimeSpan.FromMilliseconds(e.NewValue));
        //    }
        //}

        private void Slider_MouseEnter(object sender, MouseEventArgs e) {
            _mouseOneSlider = true;
        }

        private void Slider_MouseLeave(object sender, MouseEventArgs e) {
            _mouseOneSlider = false;
        }


        private static string FormatTime(TimeSpan ts) {
            return ts.ToString(ts.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
        }

        #endregion


        #region Commands
        private void cmdLoad_CanExecute(object sender, CanExecuteRoutedEventArgs e) {

            e.CanExecute = true;
        }

        private void cmdLoad_Executed(object sender, ExecutedRoutedEventArgs e) {
            try {
                OpenFileDialog fileDlg = new OpenFileDialog
                {
                    FileName = "",
                    Filter = "Audio Files (*.mp3)|*.mp3|Video Files (*.mp4;*.3gp)|*.mp4;*.3gp",
                    Title = "Choose Media",
                    Multiselect = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    ReadOnlyChecked = true
                };
                if (fileDlg.ShowDialog().Value) {
                    foreach (string file in fileDlg.FileNames) {
                        FileInfo fi = new FileInfo(file);
                        PlayList newList = new PlayList
                        {
                            Name = fi.Name,
                            FullName = fi.FullName
                        };
                        if (fi.Extension.ToLower().Contains("mp3")) {
                            newList.Icon = @"Icons\Music.ico";
                        }
                        else if (fi.Extension.ToLower().Contains("mp4") || fi.Extension.ToLower().Contains("3gp")) {
                            newList.Icon = @"Icons\Video.ico";
                        }
                        _playListContainer.PlayListData.Add(newList);

                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdPlayPause_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = _playListContainer == null ? false : _playListContainer.PlayListData.Count > 0;
        }

        private void cmdPlayPause_Executed(object sender, ExecutedRoutedEventArgs e) {
            BitmapImage image = null;
            if (buttonPlayPause.ToolTip.ToString() == "Play (CTRL+P)") {
                if (_isPaused)
                    PlayMedia(null);
                else {
                    PlayMedia(GetNextMediaFileName());
                }
                _isPaused = false;
                try {
                    image = new BitmapImage(_pauseUri);
                    imagePlayPause.Source = image;
                }
                catch { buttonPlayPause.Content = "Pause (CTRL+P)"; }
                buttonPlayPause.ToolTip = "Pause (CTRL+P)";
            }
            else if (buttonPlayPause.ToolTip.ToString() == "Pause (CTRL+P)") {
                _isPaused = true;
                PauseMedia();

                try {
                    image = new BitmapImage(_playUri);
                    imagePlayPause.Source = image;
                }
                catch { buttonPlayPause.Content = "Play (CTRL+P)"; }
                buttonPlayPause.ToolTip = "Play (CTRL+P)";
            }
        }

        private void cmdPrevious_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = _playListContainer == null ? false : _playListContainer.PlayListData.Count > 0;
        }

        private void cmdPrevious_Executed(object sender, ExecutedRoutedEventArgs e) {
            BitmapImage image = null;
            PlayMedia(GetPrevMediaFileName());
            _isPaused = false;
            try {
                image = new BitmapImage(_pauseUri);
                imagePlayPause.Source = image;
            }
            catch { buttonPlayPause.Content = "Pause (CTRL+P)"; }
            buttonPlayPause.ToolTip = "Pause (CTRL+P)";
        }

        private void cmdNext_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = _playListContainer == null ? false : _playListContainer.PlayListData.Count > 0;
        }

        private void cmdNext_Executed(object sender, ExecutedRoutedEventArgs e) {
            BitmapImage image = null;
            PlayMedia(GetNextMediaFileName(true));
            _isPaused = false;
            try {
                image = new BitmapImage(_pauseUri);
                imagePlayPause.Source = image;
            }
            catch { buttonPlayPause.Content = "Pause (CTRL+P)"; }
            buttonPlayPause.ToolTip = "Pause (CTRL+P)";
        }

        private void cmdStop_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void cmdStop_Executed(object sender, ExecutedRoutedEventArgs e) {
            StopMedia();
            BitmapImage image = null;
            try {
                image = new BitmapImage(_playUri);
                imagePlayPause.Source = image;
            }
            catch { buttonPlayPause.Content = "Play (CTRL+P)"; }
            buttonPlayPause.ToolTip = "Play (CTRL+P)";
            _isPaused = false;
        }

        private void cmdMute_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void cmdMute_Executed(object sender, ExecutedRoutedEventArgs e) {
            mediaElementMain.IsMuted = !mediaElementMain.IsMuted;
        }

        private void cmdRemoveItems_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = _playListContainer == null ? false : _playListContainer.PlayListData.Count > 0 && listBoxPlaylist.SelectedItems.Count > 0;
        }

        private void cmdRemoveItems_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (MessageBox.Show("Are you sure want to remove selected items?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question)
                 == MessageBoxResult.Yes) {
                for (int i = 0; i < listBoxPlaylist.SelectedItems.Count; i++) {
                    PlayList playlist = listBoxPlaylist.SelectedItems[i] as PlayList;
                    if (playlist != null) {
                        try {
                            if (_currentlyPlayedFileName == playlist.FullName) {
                                StopMedia();
                            }
                            _playListContainer.PlayListData.Remove(playlist);
                            i--;
                        }
                        catch (Exception ex) {
                            MessageBox.Show("Error removing items", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
        }

        private void cmdClearAll_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = _playListContainer == null ? false : _playListContainer.PlayListData.Count > 0;
        }

        private void cmdClearAll_Executed(object sender, ExecutedRoutedEventArgs e) {
            try {
                if (MessageBox.Show("Are you sure want to clear all items?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question)
                    == MessageBoxResult.Yes) {
                    _playListContainer.PlayListData.Clear();
                    if (textBlockMediaStatus.Text.StartsWith("Playing") ||
                        textBlockMediaStatus.Text.StartsWith("Paused")) { StopMedia(); }
                }
            }
            catch (Exception ex) {

                MessageBox.Show("Error clearing items", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex);
            }
        }
        #endregion
    }

    static class MediaElementExtensions {
        public static bool CanSeek(this System.Windows.Controls.MediaElement me) {
            // Heuristique: seek possible si durée connue ET non stream (WPF n’expose pas explicitement IsSeekable)
            return me.NaturalDuration.HasTimeSpan;
        }
    }

}
