using System.Web;

namespace PSOK.Kernel
{
    internal class ApplicationInit : IHttpModule
    {
        void IHttpModule.Init(HttpApplication httpApplication)
        {
            Application.Start(httpApplication);
        }

        void IHttpModule.Dispose()
        {
        }
    }
}