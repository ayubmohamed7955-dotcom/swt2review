namespace MobileChargingStation.Lib.Boundary.Interfaces;

public interface IDoor
{
    event EventHandler? DoorOpened;

    event EventHandler? DoorClosed;

    void LockDoor();
    void UnlockDoor();
}
