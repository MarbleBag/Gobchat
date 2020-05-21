/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Gobchat.UI.Forms
{
    //TODO clean up, maybe move each part in its own class
    internal static class NativeMethods
    {
        #region WndProcMessage 

        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_CHAR = 0x0102;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;
        public const int WM_SYSCHAR = 0x0106;

        #endregion


        /// <summary>
        /// Use pblend as the blend function. If the display mode is 256 colors or less, the effect of this value is the same as the effect of ULW_OPAQUE. 
        /// </summary>
        public const int ULW_ALPHA = 0x00000002;
        /// <summary>
        /// Use crKey as the transparency color.  
        /// </summary>
        public const int ULW_COLORKEY = 0x00000001;
        /// <summary>
        /// Draw an opaque layered window. 
        /// </summary>
        public const int ULW_OPAQUE = 0x00000004;
        /// <summary>
        /// Force the UpdateLayeredWindowIndirect function to fail if the current window size does not match the size specified in the psize. 
        /// </summary>
        public const int ULW_EX_NORESIZE = 0x00000008;

        public const byte AC_SRC_ALPHA = 1;
        public const byte AC_SRC_OVER = 0;

        public struct BlendFunction
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        public struct Point
        {
            public int X;
            public int Y;
        }

        public struct Size
        {
            public int Width;
            public int Height;
        }

        #region user32.dll

        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;


        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        public const int KEY_PRESSED = 0x8000;
        public const int KEY_TOGGLED = 0x0001;

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);

        private static int ToIntPtr32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        public static IntPtr SetWindowLongA(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;

            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                Int32 result32 = SetWindowLong(hWnd, nIndex, ToIntPtr32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(result32);
            }
            else
            {
                result = SetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
                throw new System.ComponentModel.Win32Exception(error);

            return result;
        }

        /// <summary>
        /// Updates the position, size, shape, content, and translucency of a layered window.
        /// If hdcSrc is NULL, dwFlags should be zero.
        /// </summary>
        /// <param name="hWnd">Type: HWND. A handle to a layered window. A layered window is created by specifying WS_EX_LAYERED when creating the window with the CreateWindowEx function.</param>
        /// <param name="hdcDst">Type: HDC A handle to a DC for the screen.This handle is obtained by specifying NULL when calling the function. It is used for palette color matching when the window contents are updated.If hdcDst isNULL, the default palette will be used. If hdcSrc is NULL, hdcDst must be NULL.</param>
        /// <param name="pptDst">A pointer to a structure that specifies the new screen position of the layered window. If the current position is not changing, pptDst can be NULL.</param>
        /// <param name="pSize">A pointer to a structure that specifies the new size of the layered window. If the size of the window is not changing, psize can be NULL. If hdcSrc is NULL, psize must be NULL.</param>
        /// <param name="hdcSrc">A handle to a DC for the surface that defines the layered window. This handle can be obtained by calling the CreateCompatibleDC function. If the shape and visual context of the window are not changing, hdcSrc can be NULL.</param>
        /// <param name="pptSrc">A pointer to a structure that specifies the location of the layer in the device context. If hdcSrc is NULL, pptSrc should be NULL.</param>
        /// <param name="crKey">A structure that specifies the color key to be used when composing the layered window. To generate a COLORREF, use the RGB macro.</param>
        /// <param name="pBlend">A pointer to a structure that specifies the transparency value to be used when composing the layered window.</param>
        /// <param name="dwFlags">This parameter can be one of the following values.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
        [DllImport("user32")]
        public static extern bool UpdateLayeredWindow(
           IntPtr hWnd,
           IntPtr hdcDst,
           [In] ref Point pptDst,
           [In] ref Size pSize,
           IntPtr hdcSrc,
           [In] ref Point pptSrc,
           int crKey,
           [In] ref BlendFunction pBlend,
           uint dwFlags);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        #endregion

        #region kernel32

        [DllImport("kernel32")]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        #endregion

        #region gdi32

        [DllImport("gdi32")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32")]
        public static extern bool DeleteDC(IntPtr hdc);

        public const int DIB_RGB_COLORS = 0x0000;

        [DllImport("gdi32")]
        public static extern IntPtr CreateDIBSection(
            IntPtr hdc,
            [In] ref BitmapInfo pbmi,
            uint iUsage,
            out IntPtr ppvBits,
            IntPtr hSection,
            uint dwOffset);

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct BitmapInfo
        {
            public BitmapInfoHeader bmiHeader;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
            public RgbQuad[] bmiColors;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct BitmapInfoHeader
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public BitmapCompressionMode biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;

            public void Init()
            {
                biSize = (uint)Marshal.SizeOf(this);
            }
        }

        public enum BitmapCompressionMode : uint
        {
            BI_RGB = 0,
            BI_RLE8 = 1,
            BI_RLE4 = 2,
            BI_BITFIELDS = 3,
            BI_JPEG = 4,
            BI_PNG = 5
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct RgbQuad
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        #endregion

    }
}
