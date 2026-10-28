using MobileChargingStation.Lib.Boundary.Interfaces;

namespace MobileChargingStation.Lib.Boundary;

public class Display : IDisplay
{
    public void ShowUserMessage(string message)
    {
        Console.WriteLine($"[Bruger] {message}");
    }

    public void ShowChargeMessage(string message)
    {
        Console.WriteLine($"[Ladning] {message}");
    }
}
