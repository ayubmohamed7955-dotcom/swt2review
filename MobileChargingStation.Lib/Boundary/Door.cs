using MobileChargingStation.Lib.Boundary.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

public class Door : IDoor
{
    public event EventHandler? DoorOpened;
    public event EventHandler? DoorClosed;

    public bool IsOpen { get; private set; }
    public bool IsLocked { get; private set; }

    public void LockDoor()
    {
        if (IsOpen)
            return;

        IsLocked = true;
    }

    public void UnlockDoor()
    {
        IsLocked = false;
    }

    public void OnDoorOpen()
    {
        if (IsOpen || IsLocked)
            return;

        IsOpen = true;
        DoorOpened?.Invoke(this, EventArgs.Empty);
    }

    public void OnDoorClose()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        DoorClosed?.Invoke(this, EventArgs.Empty);
    }
}
