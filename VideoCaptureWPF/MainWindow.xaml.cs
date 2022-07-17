using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VideoCaptureWPF.DeviceEnumeration;

namespace VideoCaptureWPF
{
    public partial class MainWindow : System.Windows.Window
    {
        private readonly VideoCapture _capture;
        //private readonly CascadeClassifier cascadeClassifier;
        private readonly BackgroundWorker _bkgWorker;
        private readonly SystemDeviceEnumerator _systemDeviceEnumerator;

        public MainWindow()
        {
            InitializeComponent();

            _capture = new VideoCapture();
            //cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");
            _systemDeviceEnumerator = new SystemDeviceEnumerator();

            _bkgWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
            };
            _bkgWorker.DoWork += Worker_DoWork;
            _bkgWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            DevicesComboBox.SelectionChanged += DevicesComboBox_SelectionChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DevicesComboBox.Items.Add(VideoInputDevice.None);

            foreach (var device in _systemDeviceEnumerator.ListVideoInputDevices())
            {
                DevicesComboBox.Items.Add(device);
            }

            DevicesComboBox.SelectedItem = VideoInputDevice.None;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _bkgWorker.CancelAsync();

            _capture.Dispose();
            //cascadeClassifier.Dispose();
            _systemDeviceEnumerator.Dispose();
        }

        private void DevicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            _bkgWorker.CancelAsync();

            if (e.AddedItems[0] is VideoInputDevice selectedDevice && selectedDevice.Index != VideoInputDevice.None.Index)
            {
                while (_bkgWorker.IsBusy)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                    Thread.Sleep(100);
                }

                if (!_capture.Open(selectedDevice.Index))
                {
                    Close();

                    return;
                }

                var selectedResolution = selectedDevice.SupportedResolutions.FirstOrDefault();
                if (selectedResolution != null)
                {
                    _capture.Set(VideoCaptureProperties.FrameWidth, selectedResolution.Width);
                    _capture.Set(VideoCaptureProperties.FrameHeight, selectedResolution.Height);
                }

                _bkgWorker.RunWorkerAsync();
            }

            Mouse.OverrideCursor = null;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;

                    return;
                }

                var fps = _capture.Fps;

                using (var frameMat = _capture.RetrieveMat())
                {
                    /*
                    var rects = cascadeClassifier.DetectMultiScale(frameMat, 1.1, 5, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(30, 30));

                    foreach (var rect in rects)
                    {
                        Cv2.Rectangle(frameMat, rect, Scalar.Red);
                    }
                    */

                    // Must create and use WriteableBitmap in the same thread (UI Thread).
                    Dispatcher.Invoke(() =>
                    {
                        FramesPerSecondLabel.Text = $"{fps:n1} FPS";
                        FrameImage.Source = frameMat.ToWriteableBitmap();
                    });
                }

                Thread.Sleep((int)Math.Round(1000 / fps));
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FramesPerSecondLabel.Text = null;
            FrameImage.Source = null;

            _capture.Release();
        }
    }
}
