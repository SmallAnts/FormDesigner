using System;
using System.Collections;
using System.Data;
using System.Windows.Forms;

namespace Smart.FormDesigner
{
    internal static class TypeExtensions
    {
        private const string INFRAGISTICS = "Infragistics";
        public static bool IsDataCollection(this Type type)
        {
            bool result;
            if (type.FullName.IndexOf(INFRAGISTICS) >= 0)
            {
                result = typeof(ICollection).IsAssignableFrom(type);
            }
            else
            {
                result = (typeof(InternalDataCollectionBase).IsAssignableFrom(type) || typeof(BaseCollection).IsAssignableFrom(type));
            }
            return result;
        }


    }
}
