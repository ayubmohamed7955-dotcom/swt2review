namespace MobileChargingStation.Lib.Boundary.Interfaces;

public interface ILog
{
    void LogDoorLocked(int id);
    void LogDoorUnlocked(int id);
}