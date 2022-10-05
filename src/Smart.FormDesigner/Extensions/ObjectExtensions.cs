using System.ComponentModel;

namespace Smart.FormDesigner
{
    internal static class ObjectExtensions
    {
        public static void CopyPropertiesTo(this object src, object dest)
        {
            var srcProperties = TypeDescriptor.GetProperties(src);
            var destProperties = TypeDescriptor.GetProperties(dest);
            foreach (PropertyDescriptor srcPropertyDescriptor in srcProperties)
            {
                if (!srcPropertyDescriptor.IsReadOnly && srcPropertyDescriptor.IsBrowsable)
                {
                    var destPropertyDescriptor = destProperties[srcPropertyDescriptor.Name];
                    if (destPropertyDescriptor != null)
                    {
                        object srcValue = srcPropertyDescriptor.GetValue(src);
                        object destValue = destPropertyDescriptor.GetValue(dest);
                        if ((destValue != null && !destValue.Equals(srcValue)) || srcValue != null)
                        {
                            destPropertyDescriptor.SetValue(dest, srcValue);
                        }
                    }
                }
            }
        }

    }
}
