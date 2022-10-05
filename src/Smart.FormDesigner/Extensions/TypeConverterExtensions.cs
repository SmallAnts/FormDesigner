using System;
using System.ComponentModel;

namespace Smart.FormDesigner
{
    internal static class TypeConverterExtensions
    {
        public static bool CanConvert(this TypeConverter cnv, Type type)
        {
            return cnv.CanConvertFrom(type) && cnv.CanConvertTo(type);
        }
    }
}
