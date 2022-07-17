using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VideoCaptureWPF.DeviceEnumeration
{
    /// <summary>
    /// A system device enumerator.
    /// </summary>
    public class SystemDeviceEnumerator : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemDeviceEnumerator"/> class.
        /// </summary>
        public SystemDeviceEnumerator()
        {
        }

        /// <summary>
        /// Lists the video input devices connected to the system.
        /// </summary>
        public IEnumerable<VideoInputDevice> ListVideoInputDevices()
        {
            var devices = new List<VideoInputDevice>();

            foreach (var device in DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice))
            {
                devices.Add(new VideoInputDevice(devices.Count, device.Mon, GetAllAvailableResolutions(device)));
            }

            return devices;

            /*
            var hresult = _systemDeviceEnumerator.CreateClassEnumerator(FilterCategory.VideoInputDevice, out var enumMoniker, 0);
            if (hresult != 0)
            {
                throw new ApplicationException("No devices of the category");
            }

            var moniker = new IMoniker[1];
            var list = new List<VideoInputDevice>();

            while (true)
            {
                hresult = enumMoniker.Next(1, moniker, IntPtr.Zero);
                if (hresult != 0 || moniker[0] == null)
                {
                    break;
                }

                var device = new VideoInputDevice(list.Count, moniker[0]);
                list.Add(device);

                // Release COM object
                Marshal.ReleaseComObject(moniker[0]);
                moniker[0] = null;
            }

            return list;
            */
        }

        /*
        /// <summary>
        /// Frees, releases, or resets unmanaged resources.
        /// </summary>
        /// <param name="disposing"><c>false</c> if invoked by the finalizer because the object is being garbage collected; otherwise, <c>true</c></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_systemDeviceEnumerator != null)
                    {
                        Marshal.ReleaseComObject(_systemDeviceEnumerator);
                        //_systemDeviceEnumerator = null;
                    }
                }

                _disposed = true;
            }
        }
        */

        /// <summary>
        /// Frees, releases, or resets unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private List<BitmapInfoHeader> GetAllAvailableResolutions(DsDevice videoDevice)
        {
            int hr, bitCount = 0;

            var filterGraph = (IFilterGraph2)new FilterGraph();
            hr = filterGraph.AddSourceFilterForMoniker(videoDevice.Mon, null, videoDevice.Name, out var sourceFilter);

            DsError.ThrowExceptionForHR(hr);

            var pRaw2 = DsFindPin.ByCategory(sourceFilter, PinCategory.Capture, 0);

            var videoInfoHeader = new VideoInfoHeader();
            hr = pRaw2.EnumMediaTypes(out var mediaTypeEnum);

            DsError.ThrowExceptionForHR(hr);

            var mediaTypes = new AMMediaType[1];
            var fetched = IntPtr.Zero;

            var availableResolutions = new List<BitmapInfoHeader>();

            do
            {
                hr = mediaTypeEnum.Next(1, mediaTypes, fetched);

                DsError.ThrowExceptionForHR(hr);

                Marshal.PtrToStructure(mediaTypes[0].formatPtr, videoInfoHeader);
                if (videoInfoHeader.BmiHeader.Size != 0 && videoInfoHeader.BmiHeader.BitCount != 0)
                {
                    if (videoInfoHeader.BmiHeader.BitCount > bitCount)
                    {
                        availableResolutions.Clear();
                        bitCount = videoInfoHeader.BmiHeader.BitCount;
                    }

                    availableResolutions.Add(videoInfoHeader.BmiHeader);
                }
            }
            while (fetched != IntPtr.Zero && mediaTypes[0] != null);

            return availableResolutions;
        }
    }
}
