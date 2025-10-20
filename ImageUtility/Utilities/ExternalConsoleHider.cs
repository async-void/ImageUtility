using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class ExternalConsoleHider : IDisposable
{
    const int SW_HIDE = 0;
    const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
    const int MAX_PATH = 260;

    delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);

    readonly string matchExeNameLower;
    readonly CancellationTokenSource cts = new CancellationTokenSource();
    readonly Task monitorTask;
    readonly int pollMs;

    public ExternalConsoleHider(string exeNameOrPathFragment = "avifenc.exe", int pollIntervalMs = 50)
    {
        if (string.IsNullOrWhiteSpace(exeNameOrPathFragment)) throw new ArgumentNullException(nameof(exeNameOrPathFragment));
        matchExeNameLower = exeNameOrPathFragment.ToLowerInvariant();
        pollMs = Math.Max(20, pollIntervalMs);
        monitorTask = Task.Run(() => MonitorLoop(cts.Token), cts.Token);
    }

    async Task MonitorLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                EnumWindows((hWnd, lParam) =>
                {
                    if (!IsWindowVisible(hWnd)) return true;

                    GetWindowThreadProcessId(hWnd, out uint pid);
                    if (pid == 0) return true;

                    IntPtr ph = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
                    if (ph == IntPtr.Zero) return true;

                    try
                    {
                        var sb = new StringBuilder(MAX_PATH);
                        int len = sb.Capacity;
                        if (QueryFullProcessImageName(ph, 0, sb, ref len))
                        {
                            string exePath = sb.ToString().ToLowerInvariant();
                            string exeName = System.IO.Path.GetFileName(exePath);

                            if (exePath.Contains(matchExeNameLower) || exeName == matchExeNameLower)
                            {
                                ShowWindow(hWnd, SW_HIDE);
                            }
                        }
                    }
                    finally
                    {
                        CloseHandle(ph);
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch { }

            try { await Task.Delay(pollMs, ct); } catch { }
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        try { monitorTask.Wait(500); } catch { }
        cts.Dispose();
    }

    public static ExternalConsoleHider WatchAvifenc(int pollIntervalMs = 50) => new ExternalConsoleHider("avifenc.exe", pollIntervalMs);
}


