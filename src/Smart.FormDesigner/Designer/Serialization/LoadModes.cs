namespace Smart.FormDesigner.Serialization
{
    /// <summary>
    /// 表单加载方式
    /// </summary>
    public enum LoadModes
    {
        /// <summary>
        /// 默认的
        /// </summary>
        Default,
        /// <summary>
        /// 允许重复的
        /// </summary>
        Duplicate,
        /// <summary>
        /// 删除表单
        /// </summary>
        EraseForm,
        /// <summary>
        /// 修改现有的
        /// </summary>
        ModifyExisting,
    }
}
