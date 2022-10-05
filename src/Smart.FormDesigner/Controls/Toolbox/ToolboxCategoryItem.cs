using System.Drawing.Design;

namespace Smart.FormDesigner.Toolbox
{
    public class ToolboxCategoryItem
    {
        public string Name { get; set; }

        public ToolboxItemCollection Items { get; set; }

        public ToolboxCategoryItem(string name)
        {
            this.Name = name;
        }

        public ToolboxCategoryItem(string name, ToolboxItemCollection items)
        {
            this.Name = name;
            this.Items = items;
        }
    }

}
