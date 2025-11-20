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
        public MainWindow() {
            InitializeComponent();

            _playListContainer = TryFindResource("playListContainer") as PlayListContainer;
            setVideoDepart();

            this.Loaded += MainWindow_Loaded;
        }

        private void setVideoDepart() {
            GestionVideo gv = new GestionVideo(_playListContainer);
        }

        #region private properties
        private TimeSpan _totalTimer, _progressTimer;
        private DispatcherTimer _timer;
        private PlayListContainer _playListContainer;
        private Uri _playUri = new Uri(@"Icons\Play.png", UriKind.Relative);
        private Uri _pauseUri = new Uri(@"Icons\Pause.png", UriKind.Relative);
        private int _currentSelectedIndex = 0;
        private bool _isPaused = false;
        private string _currentlyPlayedFileName = "";
        #endregion

        #region private methods
        private void _timer_Tick(object sender, EventArgs e) {
            _progressTimer = mediaElementMain.Position;
            if (_progressTimer.TotalSeconds <= _totalTimer.TotalSeconds) {
                sliderDuration.Value = _progressTimer.TotalSeconds;
                textBlockProgress.Text = string.Format("{0:hh\\:mm\\:ss}", _progressTimer);
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
                    sliderDuration.Maximum = _totalTimer.TotalSeconds;
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
            return fileName;
        }
        private PlayList GetPrevMediaFileName() {
            PlayList fileName = null;
            if (_currentSelectedIndex - 1 >= 0)
                _currentSelectedIndex--;
            else
                _currentSelectedIndex = _playListContainer.PlayListData.Count - 1;
            fileName = _playListContainer.PlayListData[_currentSelectedIndex];
            return fileName;
        }
        #endregion

        #region main events
        private void Window_Loaded(object sender, RoutedEventArgs e) {

            _timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
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

        private void sliderDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (mediaElementMain.Source != null) {
                if (mediaElementMain.NaturalDuration.HasTimeSpan) {
                    _progressTimer = TimeSpan.FromSeconds(sliderDuration.Value);
                    mediaElementMain.Position = _progressTimer;
                }
            }
        }


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
}
