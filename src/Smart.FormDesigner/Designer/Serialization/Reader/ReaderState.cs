namespace Smart.FormDesigner.Serialization
{
    public enum ReaderState
    {
        Initial,
        /// <summary>
        /// 开始读取节点
        /// </summary>
        StartElement,
        /// <summary>
        /// 开始读取节点值
        /// </summary>
        Value,
        /// <summary>
        /// 读取节点完成
        /// </summary>
        EndElement,
        /// <summary>
        /// 已成功到达末尾。
        /// </summary>
        EOF,
        /// <summary>
        /// 将出现错误，以防止读取的操作继续进行。
        /// </summary>
        Error
    }
}
