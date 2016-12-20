using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MainWebApp.Startup))]
namespace MainWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
