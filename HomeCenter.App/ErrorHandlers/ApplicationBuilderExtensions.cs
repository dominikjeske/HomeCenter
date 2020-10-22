using Microsoft.AspNetCore.Builder;

namespace HomeCenter.App
{
    public static class ApplicationBuilderExtensions
    {
        public static void RegisterUnhandledHandlers(this IApplicationBuilder app)
        {
            var handler = app.ApplicationServices.GetService(typeof(UnhandledExceptionHandler)) as UnhandledExceptionHandler;
            handler?.Register();
        }
    }
}