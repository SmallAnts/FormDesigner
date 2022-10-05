using System;
using System.Drawing.Design;

namespace Smart.FormDesigner.Toolbox
{
    public class ToolboxItemUsedArgs : EventArgs
    {
        public ToolboxItem UsedItem { get; set; }

        public ToolboxItemUsedArgs(ToolboxItem usedItem)
        {
            this.UsedItem = usedItem;
        }
    }
}
