﻿/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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

using CefSharp;
using Gobchat.UI.Forms.Extension;
using Gobchat.UI.Forms.Helper;
using Gobchat.UI.Web;
using NLog;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Gobchat.UI.Forms
{
    public partial class CefOverlayForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // used in CreateParams
        private const int WS_EX_TOPMOST = 0x00000008;

        // Allows the use of layered windows functions
        private const int WS_EX_LAYERED = 0x00080000;

        private const int CP_NOCLOSE_BUTTON = 0x200;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private readonly FormDragMoveHelper _formMover;
        private readonly FormResizeHelper _formResizer;
        private readonly FormEnsureTopmostHelper _formEnsureTopmost;
        private readonly FormMessageToCefKeyEventConverter _formKeyConverter;
        private readonly Web.JavascriptAndJsonBuilder _jsBuilder = new Web.JavascriptAndJsonBuilder();

        private DeviceIndependentBitmap _colorBuffer;

        public IManagedWebBrowser Browser { get; private set; }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | WS_EX_TOPMOST | WS_EX_LAYERED; //form should be topmost and layered
                cp.ClassStyle = cp.ClassStyle | CP_NOCLOSE_BUTTON;
                return cp;
            }
        }

        public CefOverlayForm()
        {
            InitializeComponent();
            KeyPreview = true;

            _formMover = new FormDragMoveHelper(this);
            _formResizer = new FormResizeHelper(this, 16);
            _formEnsureTopmost = new FormEnsureTopmostHelper(this, 1000);
            _formKeyConverter = new FormMessageToCefKeyEventConverter();

            _formMover.FormMove += (sender, e) => Browser.SendMoveOrResizeStartedEvent();

            SetStyle(ControlStyles.ResizeRedraw, true); // May not be needed anymore

            var browser = new ManagedWebBrowser(form: this);
            Browser = browser;
            Browser.OnBrowserConsoleLog += (s, e) => logger.Info(() => $"Browser Console Log ${e.Line} in ${e.Source}\n=> {e.Message}");
            Browser.OnBrowserError += (s, e) => logger.Error(() => $"[{e.ErrorCode}] {e.ErrorText})");

            //seems this is still not implemented by CefSharp, no events are fired
            /*
            var renderHandler = new Gobchat.UI.Forms.Helper.RenderHandlerAdapter();
            renderHandler.CursorChange += (sender, e) => Cursor = new Cursor(e.Cursor);
            renderHandler.Paint += (sender, e) => OnBrowserRequestsPainting(e.DirtyRect, e.Buffer, e.Width, e.Height);
            Browser.RenderHandler = renderHandler;
            */

            this.Resize += (sender, e) => Browser.Size = new System.Drawing.Size(Width, Height);
            this.MinimumSize = new System.Drawing.Size(200, 200);
            browser.StartBrowser(this.Width, this.Height);
        }

        private void DisposeForm(bool disposing)
        {
            logger.Debug("Disposing cef overlay");

            _formEnsureTopmost?.Dispose();
            _formMover?.Dispose();

            // Browser?.CloseBrowser(true);
            Browser?.Dispose();
            Browser = null;

            _colorBuffer?.Dispose();
            _colorBuffer = null;
        }

        public void InvokeAsyncOnUI(Action<CefOverlayForm> action)
        {
            UIExtensions.InvokeAsyncOnUI(this, action);
        }

        public TOut InvokeSyncOnUI<TOut>(Func<CefOverlayForm, TOut> action)
        {
            return UIExtensions.InvokeSyncOnUI(this, action);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (_formResizer.ProcessFormMessage(ref m))
                return; //done

            if (_formKeyConverter.IsProcessableKeyEvent(ref m))
            {
                var cefKeyEvent = _formKeyConverter.ProcessKeyEvent(ref m);
                Browser.SendKeyEvent(cefKeyEvent);
                return; //done
            }
        }

        public void SetAcceptFocus(bool accept)
        {
            int ex = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
            if (accept)
                ex &= ~WS_EX_NOACTIVATE;
            else
                ex |= WS_EX_NOACTIVATE;
            NativeMethods.SetWindowLongA(Handle, NativeMethods.GWL_EXSTYLE, (IntPtr)ex);
        }

        public void Reload()
        {
            Browser.Reload();
        }

        public void SetFormMouseClickThrough(bool makeClickthrough)
        {
            var style = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
            var isAlreadyClickthrough = (style & NativeMethods.WS_EX_TRANSPARENT) != 0;

            if (makeClickthrough && !isAlreadyClickthrough)
                NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE, style | NativeMethods.WS_EX_TRANSPARENT);
            if (!makeClickthrough && isAlreadyClickthrough)
                NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE, style & ~NativeMethods.WS_EX_TRANSPARENT);
        }

        private void OnForm_Resize(object sender, EventArgs e)
        {
            Browser.Size = new System.Drawing.Size(Width, Height);
        }

        private void OnEvent_Form_MouseUp(object sender, MouseEventArgs e)
        {
            _formMover.OnMouseUp(e);
            if (_formMover.AllowToMove)
                return;

            Browser.SendMouseKeyEvent(e.X, e.Y, GetMouseButtonType(e), false);
        }

        private void OnEvent_Form_MouseDown(object sender, MouseEventArgs e)
        {
            _formMover.OnMouseDown(e);
            if (_formMover.AllowToMove)
                return;

            Browser.SendMouseKeyEvent(e.X, e.Y, GetMouseButtonType(e), true);
        }

        private void OnEvent_Form_MouseMove(object sender, MouseEventArgs e)
        {
            _formMover.OnMouseMove(e);
            if (_formMover.AllowToMove)
                return;

            Browser.SendMouseMoveEvent(e.X, e.Y, GetMouseButtonType(e));
        }

        private void OnEvent_Form_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_formMover.AllowToMove)
                return;

            Browser.SendMouseWheeleEvent(e.X, e.Y, e.Delta, true);
        }

        private MouseButtonType GetMouseButtonType(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left: return CefSharp.MouseButtonType.Left;
                case System.Windows.Forms.MouseButtons.Right: return CefSharp.MouseButtonType.Right;
                case System.Windows.Forms.MouseButtons.Middle: return CefSharp.MouseButtonType.Middle;
                default: //CEF doesn't support more than that
                    return CefSharp.MouseButtonType.Left;
            }
        }

        private void OnEvent_Form_Load(object sender, EventArgs e)
        {
        }

        private void OverlayForm_MouseEnter(object sender, EventArgs e)
        {
            // logger.Trace("Overlay mouse enter");
        }

        private void OverlayForm_MouseLeave(object sender, EventArgs e)
        {
            // logger.Trace("Overlay mouse leave");
        }

        private void OnEvent_Form_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers.HasFlag(Keys.Control) || e.KeyCode == Keys.ControlKey)
            {
                logger.Trace("Deactivate Overlay move");
                _formMover.AllowToMove = false;
                _formResizer.AllowToResize = false;

                var script = _jsBuilder.BuildCustomEventDispatcher(new Web.JavascriptEvents.OverlayStateUpdateEvent(true));
                Browser.ExecuteScript(script); //TODO
            }
        }

        private void OnEvent_Form_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Modifiers.HasFlag(Keys.Control) || e.KeyCode == Keys.Control) && !_formMover.AllowToMove)
            {
                logger.Trace("Activate Overlay move");
                _formMover.AllowToMove = true;
                _formResizer.AllowToResize = true;

                var script = _jsBuilder.BuildCustomEventDispatcher(new Web.JavascriptEvents.OverlayStateUpdateEvent(false));
                Browser.ExecuteScript(script); //TODO
            }
        }

        #region rendering

        internal void OnBrowserRequestsPainting(CefSharp.Structs.Rect dirtyRect, IntPtr buffer, int width, int height)
        {
            if (_colorBuffer == null || _colorBuffer.Width != width || _colorBuffer.Height != height)
            {
                _colorBuffer?.Dispose();
                _colorBuffer = new DeviceIndependentBitmap(width, height);
            }

            _colorBuffer.CopyFromBuffer(buffer, (uint)(width * height * 4 /*RGBA*/ ));

            var blend = new NativeMethods.BlendFunction
            {
                BlendOp = NativeMethods.AC_SRC_OVER,
                BlendFlags = 0,
                SourceConstantAlpha = 255,
                AlphaFormat = NativeMethods.AC_SRC_ALPHA
            };
            var windowPosition = new NativeMethods.Point
            {
                X = this.Left,
                Y = this.Top
            };
            var surfaceSize = new NativeMethods.Size
            {
                Width = _colorBuffer.Width,
                Height = _colorBuffer.Height
            };
            var surfacePosition = new NativeMethods.Point
            {
                X = 0,
                Y = 0
            };

            try
            {
                IntPtr handle = this.InvokeSyncOnUI((f) => f.Handle); //IntPtr.Zero;
                                                                      // this.InvokeSyncOnUIThread(() => handle = this.Handle);

                NativeMethods.UpdateLayeredWindow(
                            handle,
                            IntPtr.Zero,
                            ref windowPosition,
                            ref surfaceSize,
                            _colorBuffer.DeviceContext,
                            ref surfacePosition,
                            0,
                            ref blend,
                            NativeMethods.ULW_ALPHA);
            }
            catch (ObjectDisposedException ex)
            {
                //can happen, when the form gets killed by the ui thread while we try to render CEF stuff on it
                _colorBuffer?.Dispose();
            }
        }

        #endregion rendering
    }
}