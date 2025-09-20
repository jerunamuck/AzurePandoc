using Microsoft.AspNetCore.Mvc;
using AzurePandoc.Services;

namespace AzurePandoc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PandocController : ControllerBase
{
    private readonly IPandocService _pandoc;

    public PandocController(IPandocService pandoc)
    {
        _pandoc = pandoc;
    }

    [HttpGet("version")]
    public async Task<IActionResult> GetVersion(CancellationToken ct)
    {
        var res = await _pandoc.RunAsync("--version", null, ct);
        return Content(res.StdOut + res.StdErr, "text/plain");
    }

    [HttpGet("formats")]
    public async Task<IActionResult> GetFormats(CancellationToken ct)
    {
        var res = await _pandoc.RunAsync("--list-output-formats", null, ct);
        return Content(res.StdOut + res.StdErr, "text/plain");
    }

    public class ConvertRequest
    {
        public string Input { get; set; } = string.Empty;
        public string Args { get; set; } = "-f markdown -t html"; // default
    }

    [HttpPost("convert")]
    public async Task<IActionResult> Convert([FromBody] ConvertRequest req, CancellationToken ct)
    {
        var res = await _pandoc.RunAsync(req.Args, req.Input, ct);
        if (res.ExitCode != 0)
            return Problem(detail: res.StdErr, statusCode: 500);
        return Content(res.StdOut, "application/octet-stream");
    }

    public class RunRequest
    {
        public string Args { get; set; } = string.Empty;
        public string? Input { get; set; }
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunRequest req, CancellationToken ct)
    {
        // Warning: This executes arbitrary args against pandoc binary. Ensure App Service has strict identity and validation in production.
        var res = await _pandoc.RunAsync(req.Args, req.Input, ct);
        return Content((res.StdOut + res.StdErr), "text/plain");
    }
}
