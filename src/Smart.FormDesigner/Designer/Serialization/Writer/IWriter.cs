using System;
using System.Collections;

namespace Smart.FormDesigner.Serialization
{
    public interface IWriter : IDisposable
    {
        void WriteStartElement(string name, Hashtable attributes);
        void WriteValue(string name, string value, Hashtable attributes);
        void WriteEndElement(string name);

        void Flush();
    }
}
