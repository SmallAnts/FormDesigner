using System;
using System.Drawing.Design;

namespace Smart.FormDesigner.Toolbox
{
    /// <summary>
    /// 工具箱接口类
    /// </summary>
    public interface IToolbox
    {
        /// <summary>
        /// 开始拖放控件事件
        /// </summary>
        event EventHandler BeginDragAndDrop;

        /// <summary>
        /// 控件拖放完成事件
        /// </summary>
        event EventHandler DropControl;

        /// <summary>
        /// 获取工具箱分类集合
        /// </summary>
        ToolboxCategoryCollection Items { get; }

        /// <summary>
        /// 当前选中的控件
        /// </summary>
        ToolboxItem SelectedItem { get; set; }

        /// <summary>
        /// 当前选择中控件的分类
        /// </summary>
        string SelectedCategory { get; set; }

        /// <summary>
        /// 向工具箱添加控件分类
        /// </summary>
        /// <param name="text"></param>
        void AddCategory(string text);

        /// <summary>
        /// 向工具箱添加控件
        /// </summary>
        /// <param name="item">控件信息对象</param>
        /// <param name="category">分类</param>
        void AddItem(ToolboxItem item, string category = null);

        /// <summary>
        /// 从工具箱移除指定控件
        /// </summary>
        /// <param name="item">控件信息对象</param>
        /// <param name="category">分类</param>
        void RemoveItem(ToolboxItem item, string category = null);

        /// <summary>
        /// 刷新工具箱的状态
        /// </summary>
        void Refresh();
    }

}
