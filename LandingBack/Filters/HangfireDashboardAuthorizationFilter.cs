// Hangfire is commented out for now - uncomment when Hangfire is enabled
/*
using Hangfire.Dashboard;

namespace LandingBack.Filters
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // En desarrollo, permitir acceso libre
            if (context.GetHttpContext().RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
                return true;

            // En producción, verificar autenticación y rol de Admin
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity?.IsAuthenticated == true &&
                   httpContext.User.IsInRole("Admin");
        }
    }
}
*/
