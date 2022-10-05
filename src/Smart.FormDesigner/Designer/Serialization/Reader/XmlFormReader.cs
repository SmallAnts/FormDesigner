using System.IO;
using System.Xml;

namespace Smart.FormDesigner.Serialization
{
    public class XmlFormReader : ReaderBase<XmlReader>
    {
        private bool isEmptyValue = false;

        #region 构造函数

        public XmlFormReader(string fileName)
        {
            this.reader = new XmlTextReader(fileName);
        }
        public XmlFormReader(XmlReader stream)
        {
            this.reader = stream;
        }
        public XmlFormReader(Stream stream)
        {
            this.reader = new XmlTextReader(stream);
        }

        #endregion

        public override bool Read()
        {
            if (this.State == ReaderState.Error || this.State == ReaderState.EOF || this.reader.ReadState != ReadState.Initial && this.reader.ReadState != ReadState.Interactive)
            {
                return false;
            }
            if (this.State == ReaderState.Initial && !this.ReadUntil(XmlNodeType.Element))
            {
                this.State = ReaderState.Error;
                return false;
            }

            if (this.isEmptyValue)
            {
                this.isEmptyValue = false;
                this.ReadNext();
            }

            if (this.reader.NodeType == XmlNodeType.Element)
            {
                this.State = ReaderState.StartElement;
                this.Name = this.reader.Name;
                this.ReadAttributes();

                this.isEmptyValue = this.reader.IsEmptyElement;
                if (this.isEmptyValue)
                {
                    this.Value = "";
                    this.State = ReaderState.Value;
                    return true;
                }
            }
            else if (this.reader.NodeType == XmlNodeType.EndElement)
            {
                this.State = ReaderState.EndElement;
                this.Name = "";
            }
            else
            {
                this.State = ReaderState.Value;
            }

            if (!this.ReadNext())
            {
                this.State = this.reader.ReadState == ReadState.EndOfFile ? ReaderState.EOF : ReaderState.Error;
                return false;
            }

            if (this.reader.NodeType == XmlNodeType.Text)
            {
                this.Value = this.reader.Value;
                if (!this.ReadNext() || this.reader.NodeType != XmlNodeType.EndElement)
                {
                    this.State = ReaderState.Error;
                    return false;
                }
                this.State = ReaderState.Value;
                this.ReadNext();
            }
            else if (this.reader.NodeType == XmlNodeType.EndElement && this.reader.Name == this.Name)
            {
                this.Value = "";
                this.State = ReaderState.Value;
                this.ReadNext();
            }
            return true;
        }

        private void ReadAttributes()
        {
            this.Attributes.Clear();
            while (this.reader.MoveToNextAttribute())
            {
                this.Attributes[this.reader.Name] = this.reader.Value;
            }
        }
        private bool ReadUntil(XmlNodeType nodeType)
        {
            while (this.ReadNext())
            {
                if (this.reader.NodeType == nodeType)
                    return true;
            }
            return false;
        }
        private bool ReadNext()
        {
            while (this.reader.Read())
            {
                switch (this.reader.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.Text:
                    case XmlNodeType.EndElement:
                        return true;
                    default:
                        continue;
                }
            }
            return false;
        }

        public override void Dispose()
        {
            this.reader.Close();
        }

    }
}
