using MobileChargingStation.Lib.Boundary.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

public class RfidReader : IRfidReader
{
    public event EventHandler<RfidEventArgs>? RfidDetected;

    public void OnRfidRead(int id)
    {
        RfidDetected?.Invoke(this, new RfidEventArgs { Id = id });
    }
}
