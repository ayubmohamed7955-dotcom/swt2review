using MobileChargingStation.Lib.Boundary.Interfaces;
using MobileChargingStation.Lib.Control.Interfaces;



namespace MobileChargingStation.Lib.Control;

public class ChargeControl : IChargeControl
{

    private const double FullyChargedLimit = 5.0;  // mA
    private const double OverloadLimit = 500.0;    // mA

    private enum ChargeState
    {
        Idle, Charging, FullyCharged, Error
    }

    private ChargeState _state = ChargeState.Idle;
    private readonly IUsbCharger _usbCharger;
    private readonly IDisplay _display;
    private string? _lastChargeMessage;

    public ChargeControl(IUsbCharger usbCharger, IDisplay display)
    {
        _usbCharger = usbCharger;
        _display = display;
        _usbCharger.CurrentValueEvent += OnCurrentValueChanged;
    }

    public bool Connected => _usbCharger.Connected;

    public void StartCharge()
    {
        if (_state != ChargeState.Idle)
            return;

        _state = ChargeState.Charging;
        _usbCharger.StartCharge();
    }


    public void StopCharge()
    {
        if (_state == ChargeState.Idle)
            return;

        _state = ChargeState.Idle;
        _usbCharger.StopCharge();
        ShowChargeMessage("");
    }


    private void OnCurrentValueChanged(object? sender, CurrentEventArgs e)
    {
        if (_state == ChargeState.Idle || _state == ChargeState.Error)
            return;

        if (e.Current > OverloadLimit)
        {
            _state = ChargeState.Error;
            _usbCharger.StopCharge();
            ShowChargeMessage("Fejl i opladning");
            return;
        }

        if (_state == ChargeState.FullyCharged)
            return;

        if (e.Current > FullyChargedLimit)
        {
            ShowChargeMessage("Telefon lader");
        }
        else if (e.Current > 0)
        {
            _state = ChargeState.FullyCharged;
            ShowChargeMessage("Telefon er fuldt opladet");
        }
        else
        {
            ShowChargeMessage("");
        }
    }

    // Simulatoren sender en måling hvert 250 ms, så samme besked vises kun én gang
    private void ShowChargeMessage(string message)
    {
        if (message == _lastChargeMessage)
            return;

        _lastChargeMessage = message;
        _display.ShowChargeMessage(message);
    }
}