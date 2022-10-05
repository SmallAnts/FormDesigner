using System;

namespace Smart.FormDesigner
{
    /// <summary>
    /// 大小调整方式
    /// </summary>
    [Flags]
    public enum ResizeType
    {
        /// <summary>
        /// 使用相同宽度
        /// </summary>
        SameWidth = 1,

        /// <summary>
        /// 使用相同高度
        /// </summary>
        SameHeight = 2
    }
}
