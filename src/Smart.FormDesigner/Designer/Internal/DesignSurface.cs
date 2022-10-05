using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace Smart.FormDesigner.Internal
{
    internal class DesignSurface : UserControl //System.ComponentModel.Design.DesignSurface
    {
        private Control _savedParent;

        private Control _designedControl;
        internal Control DesignedControl
        {
            set
            {
                if (_designedControl != null)
                {
                    _designedControl.BackColorChanged -= new EventHandler(FormBackColorChanged);
                    _designedControl.BackgroundImageChanged -= new EventHandler(FormBackgroundImageChanged);
                    _designedControl.BackgroundImageLayoutChanged -= new EventHandler(FormBackgroundImageLayoutChanged);
                    _designedControl.FontChanged -= new EventHandler(FormFontChanged);
                    _designedControl.ForeColorChanged -= new EventHandler(FormForeColorChanged);

                    if (_designedControl is ScrollableControl scrollableControl)
                    {
                        scrollableControl.AutoScroll = AutoScroll;
                    }
                }

                if (value != null)
                {
                    Control control = value;
                    while (control != null && control.BackColor == Color.Transparent)
                    {
                        control = control.Parent;
                    }

                    if (control != null)
                    {
                        BackColor = control.BackColor;
                    }
                    else
                    {
                        BackColor = SystemColors.Control;
                    }
                    BackgroundImage = value.BackgroundImage;
                    BackgroundImageLayout = value.BackgroundImageLayout;
                    Font = value.Font;
                    ForeColor = value.ForeColor;
                    value.BackColorChanged += new EventHandler(FormBackColorChanged);
                    value.BackgroundImageChanged += new EventHandler(FormBackgroundImageChanged);
                    value.BackgroundImageLayoutChanged += new EventHandler(FormBackgroundImageLayoutChanged);
                    value.FontChanged += new EventHandler(FormFontChanged);
                    value.ForeColorChanged += new EventHandler(FormForeColorChanged);
                    if (value is ScrollableControl scrollableControl)
                    {
                        AutoScroll = scrollableControl.AutoScroll;
                        scrollableControl.AutoScroll = false;
                    }
                }
                _designedControl = value;
            }
        }

        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                if (value != null)
                {
                    if (value.GetService(typeof(IDesignerHost)) is IDesignerHost designerHost)
                    {
                        designerHost.AddService(typeof(DesignSurface), this);
                    }
                }
                else if (base.Site != null)
                {
                    if (base.Site.GetService(typeof(IDesignerHost)) is IDesignerHost designerHost)
                    {
                        designerHost.RemoveService(typeof(DesignSurface));
                    }
                }
                base.Site = value;
            }
        }

        public DesignSurface()
        {
            _savedParent = Parent;
            if (_savedParent != null)
            {
                _savedParent.SizeChanged += new EventHandler(OnParentResize);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_savedParent != null)
            {
                _savedParent.SizeChanged -= new EventHandler(OnParentResize);
            }
            base.Dispose(disposing);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if (!AutoScroll && (Left != 0 || Top != 0))
            {
                Location = new Point(0, 0);
                if (Parent is ScrollableControl scrollableControl
                    && (scrollableControl.AutoScrollPosition.X != 0 || scrollableControl.AutoScrollPosition.Y != 0))
                {
                    scrollableControl.AutoScrollPosition = new Point(0, 0);
                }
            }
            base.OnLocationChanged(e);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            if (_savedParent != null)
            {
                _savedParent.SizeChanged -= new EventHandler(OnParentResize);
            }
            _savedParent = Parent;
            if (Parent != null)
            {
                Parent.SizeChanged += new EventHandler(OnParentResize);
                if (Parent is ScrollableControl scrollableControl)
                {
                    scrollableControl.AutoScroll = false;
                }
            }
            base.OnParentChanged(e);
        }

        private void FormForeColorChanged(object sender, EventArgs e)
        {
            ForeColor = _designedControl.ForeColor;
        }

        private void FormFontChanged(object sender, EventArgs e)
        {
            Font = _designedControl.Font;
        }

        private void FormBackgroundImageChanged(object sender, EventArgs e)
        {
            BackgroundImage = _designedControl.BackgroundImage;
        }

        private void FormBackgroundImageLayoutChanged(object sender, EventArgs e)
        {
            BackgroundImageLayout = _designedControl.BackgroundImageLayout;
        }

        private void FormBackColorChanged(object sender, EventArgs e)
        {
            BackColor = _designedControl.BackColor;
        }

        private void OnParentResize(object o, EventArgs e)
        {
            SuspendLayout();
            var selectionService = Site.GetService<ISelectionService>();
            var selectedComponents = selectionService.GetSelectedComponents();
            selectionService.SetSelectedComponents(null);
            Size = ((Control)o).ClientSize;
            Location = new Point(0, 0);
            selectionService.SetSelectedComponents(selectedComponents);
            ResumeLayout();
        }

    }

}
