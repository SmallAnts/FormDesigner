using System.ComponentModel.Design;

namespace Smart.FormDesigner.Internal
{
    /// <inheritdoc/>
    public class DefaultDesignerTransaction : DesignerTransaction
    {
        private DesignerHost _host;

        public DefaultDesignerTransaction(DesignerHost host)
        {
            this._host = host;
        }

        public DefaultDesignerTransaction(DesignerHost host, string name) : base(name)
        {
            this._host = host;
        }

        protected override void Dispose(bool disposing)
        {
            if (!base.Committed && !base.Canceled)
            {
                base.Cancel();
            }
            base.Dispose(disposing);
        }

        protected override void OnCommit()
        {
            this._host.TransactionCommiting(this);
        }

        protected override void OnCancel()
        {
            this._host.TransactionCanceling(this);
        }
    }
}
