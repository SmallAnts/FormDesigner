using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleDesignerExample
{
    public partial class CustomDesignerControl : UserControl
    {
        public CustomDesignerControl()
        {
            InitializeComponent();
        }

        #region Browsable false

        [Browsable(false)]
        protected override bool DoubleBuffered { get { return base.DoubleBuffered; } set { base.DoubleBuffered = value; } }
        [Browsable(false)]
        public new string AccessibleDescription { get { return base.AccessibleDescription; } }
        [Browsable(false)]
        public new string AccessibleName { get { return base.AccessibleName; } }
        [Browsable(false)]
        public new AccessibleRole AccessibleRole { get { return base.AccessibleRole; } }
        [Browsable(false)]
        public override bool AllowDrop { get { return base.AllowDrop; } }
        [ReadOnly(true)]
        [Browsable(false)]
        public new AutoScaleMode AutoScaleMode { get; set; }
        [Browsable(false)]
        public override bool AutoScroll { get { return base.AutoScroll; } }
        [Browsable(false)]
        public new Size AutoScrollMargin { get { return base.AutoScrollMargin; } }
        [Browsable(false)]
        public new Size AutoScrollMinSize { get { return base.AutoScrollMinSize; } }
        [Browsable(false)]
        public override bool AutoSize { get { return base.AutoSize; } }
        [Browsable(false)]
        public new AutoSizeMode AutoSizeMode { get { return base.AutoSizeMode; } }
        [Browsable(false)]
        public override AutoValidate AutoValidate { get { return base.AutoValidate; } }
        [Browsable(false)]
        public override Color BackColor { get { return base.BackColor; } }
        [Browsable(false)]
        public override Image BackgroundImage { get { return base.BackgroundImage; } }
        [Browsable(false)]
        public override ImageLayout BackgroundImageLayout { get { return base.BackgroundImageLayout; } }
        [Browsable(false)]
        public new bool CausesValidation { get { return base.CausesValidation; } }
        [Browsable(false)]
        public override ContextMenuStrip ContextMenuStrip { get { return base.ContextMenuStrip; } }
        [Browsable(false)]
        public override Cursor Cursor { get { return base.Cursor; } }
        [Browsable(false)]
        [DefaultValue(null)]
        public new ControlBindingsCollection DataBindings { get { return null; } }
        [Browsable(false)]
        public new bool Enabled { get { return base.Enabled; } }
        [Browsable(false)]
        public override Font Font { get { return base.Font; } }
        [Browsable(false)]
        public override Color ForeColor { get { return base.ForeColor; } }
        [Browsable(false)]
        public new ImeMode ImeMode { get { return base.ImeMode; } }
        [Browsable(false)]
        public new Point Location { get { return base.Location; } set { base.Location = value; } }
        [Browsable(false)]
        public new Padding Margin { get { return base.Margin; } }
        [Browsable(false)]
        public override Size MaximumSize { get { return base.MaximumSize; } }
        [Browsable(false)]
        public override Size MinimumSize { get { return base.MinimumSize; } }
        [Browsable(false)]
        public new Padding Padding { get { return base.Padding; } }
        [Browsable(false)]
        public override RightToLeft RightToLeft { get { return base.RightToLeft; } }
        [Browsable(false)]
        public new object Tag { get { return base.Tag; } }
        [Browsable(false)]
        public new bool UseWaitCursor { get { return base.UseWaitCursor; } }
        #endregion
    }
}
