namespace Smart.FormDesigner.Toolbox
{
    public class ToolboxPointerItem : ToolboxBaseItem
    {
        public ToolboxPointerItem(string text) : this(text, 2)
        {
        }
        public ToolboxPointerItem(int imageIndex) : this("指针", imageIndex)
        {
        }
        public ToolboxPointerItem(string text, int imageIndex) : base(text, imageIndex, false)
        {
        }
    }
}
