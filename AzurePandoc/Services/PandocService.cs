using System.Diagnostics;
namespace AzurePandoc.Services;

public class PandocResult
{
    public int ExitCode { get; set; }
    public string StdOut { get; set; } = string.Empty;
    public string StdErr { get; set; } = string.Empty;
}

public interface IPandocService
{
    Task<PandocResult> RunAsync(string args, string? input = null, CancellationToken ct = default);
}

public class PandocService : IPandocService
{
    private readonly ILogger<PandocService> _logger;
    private readonly string _pandocPath;

    public PandocService(ILogger<PandocService> logger, IConfiguration config)
    {
        _logger = logger;
        _pandocPath = config["Pandoc:Path"] ?? "pandoc"; // assume on PATH
    }

    public async Task<PandocResult> RunAsync(string args, string? input = null, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _pandocPath,
            Arguments = args,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var proc = new Process { StartInfo = psi };

        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var outputTcs = new TaskCompletionSource<bool>();
        var errorTcs = new TaskCompletionSource<bool>();

        proc.OutputDataReceived += (s, e) =>
        {
            if (e.Data == null) outputTcs.TrySetResult(true);
            else stdout.WriteLine(e.Data);
        };
        proc.ErrorDataReceived += (s, e) =>
        {
            if (e.Data == null) errorTcs.TrySetResult(true);
            else stderr.WriteLine(e.Data);
        };

        try
        {
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            if (!string.IsNullOrEmpty(input))
            {
                await proc.StandardInput.WriteAsync(input.AsMemory(), ct);
                await proc.StandardInput.FlushAsync(ct);
                proc.StandardInput.Close();
            }

            await Task.WhenAll(outputTcs.Task, errorTcs.Task).WaitAsync(ct);

            await proc.WaitForExitAsync(ct);

            return new PandocResult
            {
                ExitCode = proc.ExitCode,
                StdOut = stdout.ToString(),
                StdErr = stderr.ToString(),
            };
        }
        catch (OperationCanceledException)
        {
            try { if (!proc.HasExited) proc.Kill(true); } catch { }
            throw;
        }
    }
}
