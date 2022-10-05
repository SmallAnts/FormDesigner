using System;

namespace Smart.FormDesigner
{
    /// <summary>
    /// 对齐方式
    /// </summary>
    [Flags]
    public enum AlignType
    {
        /// <summary>
        /// 左对齐
        /// </summary>
        Left = 1,
        /// <summary>
        /// 右对齐
        /// </summary>
        Right = 2,
        /// <summary>
        /// 水平居中
        /// </summary>
        Center = 4,
        /// <summary>
        /// 顶端对齐
        /// </summary>
        Top = 8,
        /// <summary>
        /// 垂直居中
        /// </summary>
        Middle = 16,
        /// <summary>
        /// 底端对齐
        /// </summary>
        Bottom = 32,
    }

}
