using System;
using System.Runtime.InteropServices;

namespace Gobchat.UI.Forms
{
    internal class DeviceIndependentBitmap : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IntPtr BitmapHandle { get; private set; }
        public IntPtr Handle { get; private set; }
        public IntPtr DeviceContext { get; private set; }
        public bool IsDisposed { get; private set; }

        public DeviceIndependentBitmap(int width, int height)
        {
            Width = width;
            Height = height;

            var hScreenDC = NativeMethods.CreateCompatibleDC(IntPtr.Zero);
            DeviceContext = NativeMethods.CreateCompatibleDC(hScreenDC);

            var pbmi = new NativeMethods.BitmapInfo();
            pbmi.bmiHeader.biSize = (uint)Marshal.SizeOf(pbmi);
            pbmi.bmiHeader.biBitCount = 32;
            pbmi.bmiHeader.biPlanes = 1;
            pbmi.bmiHeader.biWidth = width;
            pbmi.bmiHeader.biHeight = -height;

            Handle = NativeMethods.CreateDIBSection(
                DeviceContext,
                ref pbmi,
                NativeMethods.DIB_RGB_COLORS,
                out IntPtr ppvBits,
                IntPtr.Zero,
                0);

            BitmapHandle = ppvBits;
            NativeMethods.SelectObject(DeviceContext, Handle);
        }

        public void CopyFromBuffer(IntPtr buffer, uint byteCount)
        {
            NativeMethods.CopyMemory(BitmapHandle, buffer, byteCount);
        }

        public void Dispose()
        {
            if (this.Handle != IntPtr.Zero)
            {
                NativeMethods.DeleteObject(Handle);
            }
            if (this.DeviceContext != IntPtr.Zero)
            {
                NativeMethods.DeleteDC(DeviceContext);
            }

            this.IsDisposed = true;
        }
    }
}
