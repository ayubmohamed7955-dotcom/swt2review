namespace MobileChargingStation.Lib.Control.Interfaces;

public interface IChargeControl
{
    bool Connected { get; }
    void StartCharge();
    void StopCharge();
}
