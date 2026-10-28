using System.Globalization;
using MobileChargingStation.Lib.Boundary.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

public class LogFile : ILog
{
    private readonly string _filePath;
    private readonly Func<DateTime> _clock;

    public LogFile(string filePath = "logfile.txt", Func<DateTime>? clock = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        _filePath = filePath;
        _clock = clock ?? (() => DateTime.Now);
    }

    public void LogDoorLocked(int id)
    {
        Write($"Skab låst med RFID: {id}");
    }

    public void LogDoorUnlocked(int id)
    {
        Write($"Skab låst op med RFID: {id}");
    }

    private void Write(string message)
    {
        var time = _clock().ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        File.AppendAllText(_filePath, $"{time}: {message}{Environment.NewLine}");
    }
}
