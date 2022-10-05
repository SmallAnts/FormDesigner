using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Smart.FormDesigner.Toolbox
{
    public class ToolboxCategoryCollection : ReadOnlyCollection<ToolboxCategoryItem>
    {
        public ToolboxCategoryCollection(IList<ToolboxCategoryItem> list) : base(list)
        {
        }
    }

}
