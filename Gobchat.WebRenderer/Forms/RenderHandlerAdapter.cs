using CefSharp;
using CefSharp.Enums;
using CefSharp.Structs;
using System;

namespace Gobchat.UI.Forms.Helper
{
    internal class RenderHandlerAdapter : CefSharp.OffScreen.IRenderHandler
    {
        public event EventHandler<PaintEventArgs> Paint;
        public event EventHandler<CursorChangeEventArgs> CursorChange;

        public void Dispose()
        {

        }

        public ScreenInfo? GetScreenInfo()
        {
            return null;
        }

        public bool GetScreenPoint(int viewX, int viewY, out int screenX, out int screenY)
        {
            screenX = 0;
            screenY = 0;
            return false;
        }

        public Rect GetViewRect()
        {
            return new Rect();
        }

        public void OnAcceleratedPaint(PaintElementType type, Rect dirtyRect, IntPtr sharedHandle)
        {

        }

        public void OnCursorChange(IntPtr cursor, CursorType type, CursorInfo customCursorInfo)
        {
            CursorChange?.Invoke(this, new CursorChangeEventArgs(cursor, type, customCursorInfo));
        }

        public void OnImeCompositionRangeChanged(Range selectedRange, Rect[] characterBounds)
        {

        }

        public void OnPaint(PaintElementType type, Rect dirtyRect, IntPtr buffer, int width, int height)
        {
            System.Diagnostics.Debug.WriteLine("CALLED!");
            Paint?.Invoke(this, new PaintEventArgs(type, dirtyRect, buffer, width, height));
        }

        public void OnPopupShow(bool show)
        {

        }

        public void OnPopupSize(Rect rect)
        {

        }

        public void OnVirtualKeyboardRequested(IBrowser browser, TextInputMode inputMode)
        {

        }

        public bool StartDragging(IDragData dragData, DragOperationsMask mask, int x, int y)
        {
            return false;
        }

        public void UpdateDragCursor(DragOperationsMask operation)
        {

        }

        public class PaintEventArgs : EventArgs
        {
            public PaintElementType Type { get; }
            public Rect DirtyRect { get; }

            public IntPtr Buffer { get; }
            public int Width { get; }
            public int Height { get; }

            public PaintEventArgs(PaintElementType type, Rect dirtyRect, IntPtr buffer, int width, int height)
            {
                this.Type = type;
                this.DirtyRect = dirtyRect;
                this.Buffer = buffer;
                this.Width = width;
                this.Height = height;
            }
        }

        public class CursorChangeEventArgs : EventArgs
        {
            public IntPtr Cursor { get; }
            public CursorType Type { get; }
            public CursorInfo CustomCursorInfo { get; }

            public CursorChangeEventArgs(IntPtr cursor, CursorType type, CursorInfo customCursorInfo)
            {
                Cursor = cursor;
                Type = type;
                CustomCursorInfo = customCursorInfo;
            }
        }
    }


}
