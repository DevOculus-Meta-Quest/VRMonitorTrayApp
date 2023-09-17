using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

public class VRMonitorTrayApp
{
    private static NotifyIcon trayIcon;
    private static System.Threading.Timer monitoringTimer; // Specify the Timer from System.Threading
    private static bool isMonitoring = true;

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        InitializeTrayIcon();

        monitoringTimer = new System.Threading.Timer(MonitorProcesses, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)); // Specify the Timer from System.Threading

        Application.Run();
    }

    private static void InitializeTrayIcon()
    {
        trayIcon = new NotifyIcon()
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = new ContextMenuStrip(), // Use ContextMenuStrip
            Visible = true
        };

        // Add ToolStripMenuItems to the ContextMenuStrip
        trayIcon.ContextMenuStrip.Items.Add("Toggle Monitoring", null, ToggleMonitoring);
        trayIcon.ContextMenuStrip.Items.Add("Restart Oculus Runtime", null, RestartOculusRuntime);
        trayIcon.ContextMenuStrip.Items.Add("About", null, OnAbout);
        trayIcon.ContextMenuStrip.Items.Add("Exit", null, OnExit);
    }

    private static void ToggleMonitoring(object sender, EventArgs e)
    {
        isMonitoring = !isMonitoring;
        string status = isMonitoring ? "Monitoring started" : "Monitoring paused";
        trayIcon.ShowBalloonTip(1000, "Status", status, ToolTipIcon.Info);
    }

    private static void RestartOculusRuntime(object sender, EventArgs e)
    {
        try
        {
            ServiceController sc = new ServiceController("OVRService");
            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                sc.Start();
                trayIcon.ShowBalloonTip(1000, "Status", "Oculus Runtime restarted", ToolTipIcon.Info);
            }
        }
        catch (Exception ex)
        {
            trayIcon.ShowBalloonTip(1000, "Error", "Failed to restart Oculus Runtime: " + ex.Message, ToolTipIcon.Error);
        }
    }

    private static void OnAbout(object sender, EventArgs e)
    {
        trayIcon.ShowBalloonTip(1000, "About", "VR Monitor Tray App", ToolTipIcon.Info);
    }

    private static void OnExit(object sender, EventArgs e)
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    private static void MonitorProcesses(object state)
    {
        if (!isMonitoring) return;

        try
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName.Contains("OVR"))
                {
                    Log($"Found process: {process.ProcessName}");
                }
            }
        }
        catch (Exception ex)
        {
            Log($"Error monitoring processes: {ex.Message}");
        }
    }

    private static void Log(string message)
    {
        string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
        File.AppendAllText(logPath, $"{DateTime.Now}: {message}\n");
    }
}
