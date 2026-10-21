using Microsoft.Extensions.Options;

namespace RallyingIsLife.Middleware;

/// <summary>
/// Options for the security headers middleware. Values come from appsettings.json
/// under the "Security" section so source lists can change without a code edit.
/// </summary>
public class SecurityHeadersOptions
{
    public const string SectionName = "Security";

    /// <summary>Extra origins allowed in script-src (Entra auth endpoints).</summary>
    public string[] ScriptSources { get; set; } = Array.Empty<string>();

    /// <summary>Extra origins allowed in style-src (none expected — self-hosted).</summary>
    public string[] StyleSources { get; set; } = Array.Empty<string>();

    /// <summary>Extra origins allowed in connect-src (API + Entra auth endpoints).</summary>
    public string[] ConnectSources { get; set; } = Array.Empty<string>();

    /// <summary>Extra origins allowed in img-src (Azure Blob storage accounts).</summary>
    public string[] ImageSources { get; set; } = Array.Empty<string>();

    /// <summary>Extra origins allowed in frame-src (Entra sign-in iframes).</summary>
    public string[] FrameSources { get; set; } = Array.Empty<string>();

    /// <summary>Origins allowed in form-action (Entra sign-in posts).</summary>
    public string[] FormActionSources { get; set; } = Array.Empty<string>();

    /// <summary>Value for frame-ancestors. "self" protects the admin portal from third-party framing.</summary>
    public string FrameAncestors { get; set; } = "self";
}

/// <summary>
/// Adds browser security headers to every response.
/// Implemented at the app layer so headers travel with each deployment.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;
    private readonly string _contentSecurityPolicy;

    public SecurityHeadersMiddleware(RequestDelegate next, IOptions<SecurityHeadersOptions> options)
    {
        _next = next;
        _options = options.Value;
        _contentSecurityPolicy = BuildContentSecurityPolicy(_options);
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["Content-Security-Policy"] = _contentSecurityPolicy;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "SAMEORIGIN";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] =
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";

        return _next(context);
    }

    private static string BuildContentSecurityPolicy(SecurityHeadersOptions options)
    {
        var directives = new List<string>
        {
            "default-src 'self'",
            "base-uri 'self'",
            "object-src 'none'",
            "frame-ancestors " + options.FrameAncestors,
            "font-src 'self' data:",
            "img-src 'self' data: " + Join(options.ImageSources),
            "script-src 'self' " + Join(options.ScriptSources),
            "style-src 'self' " + Join(options.StyleSources),
            "connect-src 'self' " + Join(options.ConnectSources),
            "frame-src 'self' " + Join(options.FrameSources),
            "form-action 'self' " + Join(options.FormActionSources),
            "manifest-src 'self'",
            "upgrade-insecure-requests"
        };

        return string.Join("; ", directives);
    }

    private static string Join(IEnumerable<string> sources)
    {
        var list = sources?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
        return list is { Count: > 0 } ? string.Join(" ", list) : string.Empty;
    }
}
