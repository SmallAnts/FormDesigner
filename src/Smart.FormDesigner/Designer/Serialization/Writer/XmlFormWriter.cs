using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace Smart.FormDesigner.Serialization
{
    public class XmlFormWriter : IWriter, IDisposable
    {
        #region 私有字段

        private XmlWriter writer;
        private XmlWriter curWriter;

        #endregion

        #region 构造函数

        public XmlFormWriter(string fileName)
        {
            this.writer = new XmlTextWriter(fileName, Encoding.UTF8);
            this.curWriter = this.writer;
        }
        public XmlFormWriter(XmlWriter stream)
        {
            this.writer = stream;
            this.curWriter = this.writer;
        }
        public XmlFormWriter(Stream stream)
        {
            this.writer = new XmlTextWriter(stream, Encoding.UTF8);
            this.curWriter = this.writer;
        }

        #endregion

        #region IWriter 接口成员

        public virtual void WriteStartElement(string name, Hashtable attributes)
        {
            this.curWriter.WriteStartElement(name);
            if (attributes != null)
            {
                foreach (DictionaryEntry dictionaryEntry in attributes)
                {
                    if (dictionaryEntry.Value != null)
                    {
                        this.curWriter.WriteAttributeString(dictionaryEntry.Key.ToString(), dictionaryEntry.Value.ToString());
                    }
                }
            }
        }
        public virtual void WriteEndElement(string name)
        {
            this.curWriter.WriteEndElement();
        }
        public virtual void WriteValue(string name, string value, Hashtable attributes)
        {
            this.WriteStartElement(name, attributes);
            this.curWriter.WriteString(value);
            this.curWriter.WriteEndElement();
        }
        public void Flush()
        {
            this.writer.Flush();
        }

        #endregion

        #region IDisposable 接口成员

        public void Dispose()
        {
            this.writer.Close();
        }

        #endregion


    }
}
