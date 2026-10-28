namespace MobileChargingStation.Lib.Boundary.Interfaces;

public interface IDisplay

{
    void ShowUserMessage(string message);
    void ShowChargeMessage(string message);
    
}