namespace FinoBankApi.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //Clickjacking
            context.Response.Headers["X-Frame-Options"] = "DENY";
            
            //XSS
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

            //MIME-Sniffing
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            //Strict-Transport-Security (HSTS)
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            
            //Content Security Policy (CSP)
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " + 
                "img-src 'self' data: https:; " +
                "script-src 'self'; " + 
                "style-src 'self' 'unsafe-inline'; " + 
                "font-src 'self'; " +
                "frame-ancestors 'none';";

            //Referrer Policy
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            await _next(context);
        }
    }
}