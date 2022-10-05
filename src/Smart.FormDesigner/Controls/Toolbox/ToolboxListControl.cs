using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Smart.FormDesigner.Toolbox
{
    internal class ToolboxListControl : ListBox
    {
        private int underMouseItemIndex = -1;
        private Point mouseClickOrigin;
        private readonly int DragDistance = 3;

        public event EventHandler<ToolboxItemDragEventArgs> ItemDrag;

        [Category("Appearance")]
        [DefaultValue("ActiveCaption")]
        public Color ItemHoverBackColor { get; set; } = SystemColors.ActiveCaption;

        [Category("Appearance")]
        [DefaultValue("GradientActiveCaption")]
        public Color SelectedItemHoverBackColor { get; set; } = SystemColors.GradientActiveCaption;

        [Category("Appearance")]
        [DefaultValue("GradientInactiveCaption")]
        public Color SelectedItemBackColor { get; set; } = SystemColors.GradientInactiveCaption;

        [Category("Appearance")]
        [DefaultValue("ActiveBorder")]
        public Color SelectedItemBorderColor { get; set; } = SystemColors.ActiveBorder;

        [Category("Appearance")]
        [DefaultValue("Window")]
        public Color GroupBackColor { get; set; } = SystemColors.Window;

        [Category("Appearance")]
        public ImageList Images { get; set; }

        [Browsable(false)]
        [DefaultValue(null)]
        public new ToolboxBaseItem SelectedItem
        {
            get => (_selectedIndex >= 0) ? Items[_selectedIndex] as ToolboxBaseItem : null;
            set
            {
                int index = -1;
                if (value != null)
                {
                    index = this.Items.IndexOf(value);
                }
                if (index != _selectedIndex)
                {
                    ChangeSelection(index);
                }
            }
        }

        private int _selectedIndex = -1;
        public override int SelectedIndex
        {
            get => _selectedIndex;
            set => ChangeSelection(value);
        }


        private void ChangeSelection(int newSelectedIndex)
        {
            if (newSelectedIndex == _selectedIndex)
                return;

            if (_selectedIndex >= 0)
            {
                int saveSelected = _selectedIndex;

                _selectedIndex = -1;
                PaintItem(saveSelected, false, null);
            }

            if (newSelectedIndex >= 0)
            {
                _selectedIndex = newSelectedIndex;
                PaintItem(_selectedIndex, false, null);
            }
        }

        private int GetItemIndex(Point pt)
        {
            int index = TopIndex, count = Items.Count;

            while (index < count)
            {
                var bounds = this.GetItemRectangle(index);
                if (bounds.Contains(pt))
                    return index;
                index++;
            }
            return -1;
        }

        protected void PaintItem(int index, bool hover, Graphics graphics)
        {
            var g = (graphics == null) ? Graphics.FromHwnd(this.Handle) : graphics;

            var bounds = GetItemRectangle(index);

            bool isSelected = (index == _selectedIndex);
            var tbItem = Items[index] as ToolboxBaseItem;
            string text = tbItem.Text;
            hover = (hover && !tbItem.IsGroup);

            var backColor = this.BackColor;
            if (tbItem.IsGroup && isSelected)
                backColor = this.SelectedItemBackColor;
            else if (tbItem.IsGroup)
                backColor = this.GroupBackColor;
            else if (hover && isSelected)
                backColor = this.SelectedItemHoverBackColor;
            else if (hover)
                backColor = this.ItemHoverBackColor;
            else if (isSelected)
                backColor = this.SelectedItemBackColor;

            var backBrush = new SolidBrush(backColor);
            var foreBrush = new SolidBrush(this.ForeColor);

            g.FillRectangle(backBrush, bounds);
            if (hover || isSelected)
            {
                using (var pen = new Pen(SelectedItemBorderColor, 1))
                {
                    bounds.Size = new Size(bounds.Size.Width - 1, bounds.Size.Height - 1);
                    g.DrawRectangle(pen, bounds);
                }
            }
            bounds = GetItemRectangle(index);

            int imageHeight = 0;
            int imageWidth = 0;

            if (this.Images != null)
            {
                imageHeight = this.Images.ImageSize.Height;
                imageWidth = this.Images.ImageSize.Width;

                int offset = tbItem.IsGroup ? 2 : 4;
                int imgLeft = bounds.Left + offset;
                if (RightToLeft == RightToLeft.Yes)
                    imgLeft = bounds.Right - offset - imageWidth;

                if (tbItem.ImageIndex >= 0)
                    this.Images.Draw(g, imgLeft + 4, bounds.Top + offset, imageWidth, imageHeight, tbItem.ImageIndex);
            }

            var size = g.MeasureString(text, this.Font);
            int vOffset = (bounds.Height - (int)size.Height) / 2;
            int hOffset = 1;

            var textFont = this.Font;
            if (tbItem.IsGroup)
                textFont = new Font(textFont.FontFamily, textFont.Size, FontStyle.Bold);

            var sf = new StringFormat();
            if (RightToLeft == RightToLeft.Yes)
            {
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                bounds.Size = new Size(bounds.Width - imageWidth - 2, bounds.Height);
            }
            else
            {
                bounds.Location = new Point(bounds.Left + imageWidth + 10, bounds.Top);
            }

            bounds.Inflate(-hOffset, -vOffset);
            g.DrawString(text, textFont, foreBrush, bounds, sf);

            foreBrush.Dispose();
            backBrush.Dispose();

            if (tbItem.IsGroup)
                textFont.Dispose();

            if (graphics == null)
                g.Dispose();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != 0)
            {
                if (_selectedIndex >= 0 && mouseClickOrigin.Distance(Control.MousePosition) > DragDistance)
                {
                    if (ItemDrag != null)
                    {
                        var dItem = Items[_selectedIndex] as ToolboxBaseItem;
                        ItemDrag(this, new ToolboxItemDragEventArgs(dItem));
                    }
                }
                return;
            }

            var mousePoint = PointToClient(Control.MousePosition);
            int itemIndex = GetItemIndex(mousePoint);

            if (itemIndex != -1 && underMouseItemIndex != itemIndex)
            {
                if (underMouseItemIndex != -1)
                    PaintItem(underMouseItemIndex, false, null);

                underMouseItemIndex = itemIndex;
                PaintItem(underMouseItemIndex, true, null);
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (underMouseItemIndex != -1)
            {
                PaintItem(underMouseItemIndex, false, null);
                underMouseItemIndex = -1;
            }
            base.OnMouseLeave(e);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count)
                return;

            Point mousePosition = this.PointToClient(Control.MousePosition);
            Rectangle bounds = GetItemRectangle(e.Index);

            bool underMouse = bounds.Contains(mousePosition) & (Control.MouseButtons == 0);
            PaintItem(e.Index, underMouse, e.Graphics);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            mouseClickOrigin = Control.MousePosition;
            int index = this.GetItemIndex(this.PointToClient(Control.MousePosition));
            if (index >= 0)
                ChangeSelection(index);

            base.OnMouseDown(e);
        }



    }

}
