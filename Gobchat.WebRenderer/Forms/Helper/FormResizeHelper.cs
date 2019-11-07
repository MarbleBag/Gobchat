using System;
using System.Drawing;
using System.Windows.Forms;

namespace Gobchat.UI.Forms.Helper
{
    internal class FormResizeHelper
    {
        public Form TargetForm { get; set; }
        public bool AllowToResize { get; set; }
        public int GripSize { get; set; }

        public Rectangle Top { get { return new Rectangle(0, 0, TargetForm.ClientSize.Width, GripSize); } }
        public Rectangle Left { get { return new Rectangle(0, 0, GripSize, TargetForm.ClientSize.Height); } }
        public Rectangle Bottom { get { return new Rectangle(0, TargetForm.ClientSize.Height - GripSize, TargetForm.ClientSize.Width, GripSize); } }
        public Rectangle Right { get { return new Rectangle(TargetForm.ClientSize.Width - GripSize, 0, GripSize, TargetForm.ClientSize.Height); } }
        public Rectangle TopLeft { get { return new Rectangle(0, 0, GripSize, GripSize); } }
        public Rectangle TopRight { get { return new Rectangle(TargetForm.ClientSize.Width - GripSize, 0, GripSize, GripSize); } }
        public Rectangle BottomLeft { get { return new Rectangle(0, TargetForm.ClientSize.Height - GripSize, GripSize, GripSize); } }
        public Rectangle BottomRight { get { return new Rectangle(TargetForm.ClientSize.Width - GripSize, TargetForm.ClientSize.Height - GripSize, GripSize, GripSize); } }

        private const int
            WM_NCHITTEST = 0x84,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17;

        public FormResizeHelper(Form target, int gripSize)
        {
            TargetForm = target;
            GripSize = gripSize;
        }

        public bool ProcessFormMessage(ref Message message)
        {
            if (!AllowToResize || message.Msg != WM_NCHITTEST)
                return false;

            int x = (int)(message.LParam.ToInt64() & 0xFFFF);
            int y = (int)(message.LParam.ToInt64() >> 16);
            var cursor = TargetForm.PointToClient(new Point(x, y));

            if (TopLeft.Contains(cursor))
            {
                message.Result = (IntPtr)HTTOPLEFT;
                return true;
            }
            else if (TopRight.Contains(cursor))
            {
                message.Result = (IntPtr)HTTOPRIGHT;
                return true;
            }
            else if (BottomLeft.Contains(cursor))
            {
                message.Result = (IntPtr)HTBOTTOMLEFT;
                return true;
            }
            else if (BottomRight.Contains(cursor))
            {
                message.Result = (IntPtr)HTBOTTOMRIGHT;
                return true;
            }
            else if (Top.Contains(cursor))
            {
                message.Result = (IntPtr)HTTOP;
                return true;
            }
            else if (Left.Contains(cursor))
            {
                message.Result = (IntPtr)HTLEFT;
                return true;
            }
            else if (Right.Contains(cursor))
            {
                message.Result = (IntPtr)HTRIGHT;
                return true;
            }
            else if (Bottom.Contains(cursor))
            {
                message.Result = (IntPtr)HTBOTTOM;
                return true;
            }

            return false;
        }

    }
}
