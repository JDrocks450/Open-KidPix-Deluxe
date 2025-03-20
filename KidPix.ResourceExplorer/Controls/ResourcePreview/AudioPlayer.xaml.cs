using KidPix.API.Importer;
using KidPix.API.Importer.tWAV;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KidPix.ResourceExplorer.Controls.ResourcePreview
{
    public interface IResourcePreviewControl : IDisposable
    {
        public void AttachResource(KidPixResource? Resource);
    }

    /// <summary>
    /// Interaction logic for AudioPlayer.xaml
    /// </summary>
    public partial class AudioPlayer : UserControl, INotifyPropertyChanged, IDisposable, IResourcePreviewControl
    {
        private WaveOutEvent? currentVoice;
        private RawSourceWaveStream? currentSoundData;

        private WAVResource? AudioSample;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ResourceName => AudioSample?.FileName ?? (AudioSample == null ? "None selected" : $"WaveFile_{AudioSample.ID}.wav");
        public AudioPlayer()
        {
            InitializeComponent();

            //UNLOAD
            Unloaded += delegate
            {
                Dispose();
            };
        }

        public AudioPlayer(WAVResource? resource) : this()
        {
            Loaded += delegate
            {
                AttachResource(resource);
            };
        }

        ~AudioPlayer()
        {
            Dispose();
        }

        public void AttachResource(KidPixResource? Resource)
        {
            AudioSample?.Dispose();
            AudioSample = null;
            CleanUpAudioResources();
            if (Resource is not WAVResource SoundEffect) return;

            AudioSample = SoundEffect;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ResourceName)));            
        }

        /// <summary>
        /// Cleans up resources used in the previous Audio session: <see cref="currentVoice"/>, <see cref="currentSoundData"/>
        /// </summary>
        void CleanUpAudioResources()
        {
            currentVoice?.Stop();
            currentVoice?.Dispose();
            currentSoundData?.Dispose();
            currentSoundData = null;
            currentVoice = null;
            Dispatcher.Invoke(delegate
            {
                StopIcon.Visibility = Visibility.Collapsed;
                PlayIcon.Visibility = Visibility.Visible;
            });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSafe())
            {
                MessageBox.Show("This audio file cannot be played as the WAVEData chunk does not appear to have loaded correctly.");
                return;
            }
            if (currentVoice != null) // stop sound
            {
                CleanUpAudioResources();
                return;
            }

            currentSoundData?.Dispose();
            currentSoundData = null;
            currentSoundData = GetSoundDefinition();
            currentVoice = new WaveOutEvent()
            {
                Volume = (float)VolumeSlider.Value
            };
            currentVoice.Init(currentSoundData);
            currentVoice.Play();
            Dispatcher.Invoke(delegate
            {
                StopIcon.Visibility = Visibility.Visible;
                PlayIcon.Visibility = Visibility.Collapsed;
            });
            currentVoice.PlaybackStopped += delegate
            {
                CleanUpAudioResources();
            };
        }

        /// <summary>
        /// Saves to a Temporary directory and places the FileDropList onto the Clipboard for sharing
        /// </summary>
        public void CopyToClipboard()
        {
            if (!IsSafe()) return;

            var file = System.IO.Path.GetTempPath() + $"\\{ResourceName}.wav";
            ExportOne(file, AudioSample);
            var collect = new System.Collections.Specialized.StringCollection { file };
            Clipboard.SetFileDropList(collect);
        }

        private RawSourceWaveStream? GetSoundDefinition()
        {
            if (!IsSafe()) return null;
            AudioSample.WaveData.AudioDataStream.Seek(0, SeekOrigin.Begin);
            return new RawSourceWaveStream(AudioSample.WaveData.AudioDataStream,
                new WaveFormat(AudioSample.WaveData.SampleRate, AudioSample.WaveData.Channels));
        }

        private void ExportOne(string FileName, WAVResource AudioSample)
        {
            if (!IsSafe()) return;
            WaveFileWriter.CreateWaveFile(FileName, GetSoundDefinition());
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        private void ExportWaveDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSafe()) return;
            AudioSample.WaveData.AudioDataStream.Seek(0, SeekOrigin.Begin);
            using var fs = File.Create("AudioDump.bin");
            AudioSample.WaveData.AudioDataStream.CopyTo(fs);
        }

        private bool IsSafe() => AudioSample?.WaveData?.AudioDataStream != null;

        /// <summary>
        /// Disposes all resources attached to this object
        /// </summary>
        public void Dispose()
        {
            CleanUpAudioResources();
            AudioSample?.Dispose();
            AudioSample = null;
        }
    }
}
