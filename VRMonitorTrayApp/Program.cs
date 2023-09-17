using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;

public class VRMonitorTrayApp
{
    private NotifyIcon trayIcon;
    private System.Windows.Forms.Timer monitoringTimer;
    private HashSet<string> monitoredProcessesAndServices = new HashSet<string>();
    private string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

    public VRMonitorTrayApp()
    {
        trayIcon = new NotifyIcon()
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = new ContextMenuStrip
            {
                Items =
                {
                    new ToolStripMenuItem("Start Monitoring", null, StartMonitoring),
                    new ToolStripMenuItem("Stop Monitoring", null, StopMonitoring),
                    new ToolStripMenuItem("Show Log", null, ShowLog),
                    new ToolStripMenuItem("Exit", null, Exit)
                }
            },
            Visible = true
        };

        monitoringTimer = new System.Windows.Forms.Timer
        {
            Interval = 5000 // 5 seconds
        };
        monitoringTimer.Tick += MonitorProcessesAndServices;

        // Log already running Oculus and SteamVR processes and services
        LogInitialProcessesAndServices();
    }

    private void StartMonitoring(object sender, EventArgs e)
    {
        monitoringTimer.Start();
    }

    private void StopMonitoring(object sender, EventArgs e)
    {
        monitoringTimer.Stop();
    }

    private void ShowLog(object sender, EventArgs e)
    {
        Process.Start("notepad.exe", logFilePath);
    }

    private void Exit(object sender, EventArgs e)
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    private void MonitorProcessesAndServices(object sender, EventArgs e)
    {
        MonitorProcesses();
        MonitorServices();
    }

    private void MonitorProcesses()
    {
        var currentProcesses = Process.GetProcesses()
            .Where(p => p.ProcessName.Contains("Oculus") || p.ProcessName.Contains("SteamVR"))
            .ToList();

        foreach (var process in currentProcesses)
        {
            string processDetails = $"{process.ProcessName} ({process.MainModule.FileName})";
            if (!monitoredProcessesAndServices.Contains(processDetails))
            {
                monitoredProcessesAndServices.Add(processDetails);
                Log($"{DateTime.Now}: {processDetails} started.");
            }
        }

        var stoppedProcesses = monitoredProcessesAndServices.Except(currentProcesses.Select(p => $"{p.ProcessName} ({p.MainModule.FileName})")).ToList();
        foreach (var process in stoppedProcesses)
        {
            monitoredProcessesAndServices.Remove(process);
            Log($"{DateTime.Now}: {process} stopped.");
        }
    }

    private void MonitorServices()
    {
        var currentServices = ServiceController.GetServices()
            .Where(s => s.DisplayName.Contains("Oculus") || s.DisplayName.Contains("SteamVR"))
            .ToList();

        foreach (var service in currentServices)
        {
            string serviceDetails = $"{service.ServiceName} ({service.DisplayName})";
            if (!monitoredProcessesAndServices.Contains(serviceDetails))
            {
                monitoredProcessesAndServices.Add(serviceDetails);
                Log($"{DateTime.Now}: Service {serviceDetails} started.");
            }
        }

        var stoppedServices = monitoredProcessesAndServices.Except(currentServices.Select(s => $"{s.ServiceName} ({s.DisplayName})")).ToList();
        foreach (var service in stoppedServices)
        {
            monitoredProcessesAndServices.Remove(service);
            Log($"{DateTime.Now}: Service {service} stopped.");
        }
    }

    private void LogInitialProcessesAndServices()
    {
        var initialProcesses = Process.GetProcesses()
            .Where(p => p.ProcessName.Contains("Oculus") || p.ProcessName.Contains("SteamVR"))
            .ToList();

        foreach (var process in initialProcesses)
        {
            string processDetails = $"{process.ProcessName} ({process.MainModule.FileName})";
            if (!monitoredProcessesAndServices.Contains(processDetails))
            {
                monitoredProcessesAndServices.Add(processDetails);
                Log($"{DateTime.Now}: {processDetails} was already running when the application started.");
            }
        }

        var initialServices = ServiceController.GetServices()
            .Where(s => s.DisplayName.Contains("Oculus") || s.DisplayName.Contains("SteamVR"))
            .ToList();

        foreach (var service in initialServices)
        {
            string serviceDetails = $"{service.ServiceName} ({service.DisplayName})";
            if (!monitoredProcessesAndServices.Contains(serviceDetails))
            {
                monitoredProcessesAndServices.Add(serviceDetails);
                Log($"{DateTime.Now}: Service {serviceDetails} was already running when the application started.");
            }
        }
    }

    private void Log(string message)
    {
        File.AppendAllText(logFilePath, message + Environment.NewLine);
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var app = new VRMonitorTrayApp();
        Application.Run();
    }
}
