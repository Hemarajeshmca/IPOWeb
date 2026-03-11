namespace STARegistration.Middleware
{
    public class SessionCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var basePath = context.Request.PathBase;

            if (context.Request.Path!= basePath &&  context.Request.Path != "/" && !context.Request.Path.StartsWithSegments("/Login/GetChngPwdFlag") && !context.Request.Path.StartsWithSegments("/Login/Login") && !context.Request.Path.StartsWithSegments("/Login/SessionExpired"))
            {
                if (string.IsNullOrEmpty(context.Session.GetString("user_id")))
                {
                   
                    context.Response.Redirect(basePath + "/Login/SessionExpired");
                    return;
                }
            }

            await _next(context);
        }
    }

}
