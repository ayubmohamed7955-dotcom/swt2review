using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileChargingStation.Lib.Boundary.Interfaces;
using MobileChargingStation.Lib.Control.Interfaces;



namespace MobileChargingStation.Lib.Control;

public class StationControl
{
    private enum MobileChargerState
    {
        Available,
        Locked,
        DoorOpen,
    }

    private readonly IDoor _door;
    private readonly IChargeControl _charger;
    private readonly IDisplay _display;
    private readonly ILog _log;

    private MobileChargerState _state;
    private int _oldId;

    public StationControl(
        IDoor door,
        IRfidReader rfidReader,
        IChargeControl charger,
        IDisplay display,
        ILog log
    )
    {
        _door = door;
        _charger = charger;
        _display = display;
        _log = log;
        door.DoorOpened += OnDoorOpened;
        door.DoorClosed += OnDoorClosed;
        rfidReader.RfidDetected += OnRfidDetected;

        _door.UnlockDoor();
        _display.ShowUserMessage("Indlæs RFID");
        _state = MobileChargerState.Available;
    }

    private void OnDoorOpened(object? sender, EventArgs e)
    {
        if (_state == MobileChargerState.Available)
        {
            _display.ShowUserMessage("Tilslut telefon");
            _state = MobileChargerState.DoorOpen;
        }
    }

    private void OnDoorClosed(object? sender, EventArgs e)
    {
        if (_state == MobileChargerState.DoorOpen)
        {
            _display.ShowUserMessage("Indlæs RFID");
            _state = MobileChargerState.Available;
        }
    }

    private void OnRfidDetected(object? sender, RfidEventArgs e)
    {
        switch (_state)
        {
            case MobileChargerState.Available:
                if (_charger.Connected)
                {
                    _oldId = e.Id;
                    _door.LockDoor();
                    _charger.StartCharge();
                    _log.LogDoorLocked(e.Id);
                    _display.ShowUserMessage("Ladeskab optaget");
                    _state = MobileChargerState.Locked;
                }
                else
                {
                    _display.ShowUserMessage("Tilslutningsfejl");
                }
                break;

            case MobileChargerState.Locked:
                if (e.Id == _oldId)
                {
                    _charger.StopCharge();
                    _door.UnlockDoor();
                    _log.LogDoorUnlocked(e.Id);
                    _display.ShowUserMessage("Fjern telefon");
                    _state = MobileChargerState.Available;
                }
                else
                {
                    _display.ShowUserMessage("RFID fejl");
                }
                break;
        }
    }
}