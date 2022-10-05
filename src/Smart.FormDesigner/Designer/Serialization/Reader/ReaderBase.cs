using System;
using System.Collections;
using System.Collections.Generic;

namespace Smart.FormDesigner.Serialization
{
    public abstract class ReaderBase<T> : IReader where T : IDisposable
    {
        protected T reader;

        #region IReader 接口成员

        public string Name { get; protected set; } = string.Empty;
        public string Value { get; protected set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; protected set; } = new Dictionary<string, string>();
        public ReaderState State { get; protected set; } = ReaderState.Initial;

        public abstract bool Read();

        #endregion

        #region IDisposable 接口成员

        public virtual void Dispose()
        {
            reader?.Dispose();
        }

        #endregion

    }
}
