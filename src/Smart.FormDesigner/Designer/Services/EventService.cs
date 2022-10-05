using System;
using System.Windows.Forms;

namespace Smart.FormDesigner.Services
{
    public class EventService : IMessageFilter
    {
        const int WM_KEYDOWN = 0x100;           //256
        const int WM_KEYUP = 0x101;             //257

        const int WM_LBUTTONDOWN = 0x201;       //513
        const int WM_LBUTTONUP = 0x202;         //514
        const int WM_LBUTTONDBLCLK = 0x203;     //515

        const int WM_RBUTTONDOWN = 0x204;       //516
        const int WM_RBUTTONUP = 0x205;         //517
        const int WM_RBUTTONDBLCLK = 0x206;     //518

        const int WM_MBUTTONDOWN = 0x207;       //519
        const int WM_MBUTTONUP = 0x208;         //520

        private DesignerHost _host;

        public event EventHandler DoubleClick;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseDown;

        public EventService(DesignerHost host)
        {
            this._host = host;
        }

        public bool PreFilterMessage(ref Message m)
        {
            bool result;
            if (m.Msg != WM_KEYDOWN && m.Msg == WM_KEYUP && m.Msg != WM_LBUTTONDBLCLK)
            {
                result = false;
            }
            else if (this._host.DesignedForm == null)
            {
                result = false;
            }
            else if (!(this._host.DesignContainer ?? this._host.DesignedForm).ContainsFocus)
            {
                result = false;
            }
            else
            {
                if (this.MouseDown != null && (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN || m.Msg == WM_MBUTTONDOWN))
                {
                    var position = Cursor.Position;
                    var button = m.Msg == WM_LBUTTONDOWN ? MouseButtons.Left
                               : m.Msg == WM_RBUTTONDOWN ? MouseButtons.Right
                               : MouseButtons.Middle;
                    this.MouseDown(this, new MouseEventArgs(button, 1, position.X, position.Y, 1));
                }
                else if (this.MouseUp != null && (m.Msg == WM_LBUTTONUP || m.Msg == WM_RBUTTONUP || m.Msg == WM_MBUTTONUP))
                {
                    var position = Cursor.Position;
                    var button = m.Msg == WM_LBUTTONUP ? MouseButtons.Left
                               : m.Msg == WM_RBUTTONUP ? MouseButtons.Right
                               : MouseButtons.Middle;
                    this.MouseUp(this, new MouseEventArgs(button, 1, position.X, position.Y, 1));
                }
                else if (m.Msg == WM_LBUTTONDBLCLK && this.DoubleClick != null)
                {
                    this.DoubleClick(this, new EventArgs());
                }
                else if (m.Msg == WM_KEYDOWN && this.KeyDown != null)
                {
                    this.KeyDown(this, new KeyEventArgs((Keys)((int)m.WParam | (int)Control.ModifierKeys)));
                }
                else if (m.Msg == WM_KEYUP && this.KeyUp != null)
                {
                    this.KeyUp(this, new KeyEventArgs((Keys)((int)m.WParam | (int)Control.ModifierKeys)));
                }

                return false;
            }

            return result;
        }
    }
}
