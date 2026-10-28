namespace MobileChargingStation.Lib.Boundary.Interfaces;

public class RfidEventArgs : EventArgs
{
    public int Id { get; set; }
}

public interface IRfidReader
{
    event EventHandler<RfidEventArgs>? RfidDetected;
}