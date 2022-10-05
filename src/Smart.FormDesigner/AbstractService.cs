using System;

namespace Smart.FormDesigner
{
    public abstract class AbstractService : Disposable
    {
        protected IServiceProvider ServiceProvider { get; private set; }

        public AbstractService()
        {

        }

        public AbstractService(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }
    }
}
