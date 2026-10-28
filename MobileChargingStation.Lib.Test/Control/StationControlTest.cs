using MobileChargingStation.Lib.Boundary.Interfaces;
using MobileChargingStation.Lib.Control;
using MobileChargingStation.Lib.Control.Interfaces;
using NSubstitute;

namespace MobileChargingStation.Lib.Test.Control;

public class StationControlTest
{
    private IDoor _door = null!;
    private IRfidReader _rfidReader = null!;
    private IChargeControl _charger = null!;
    private IDisplay _display = null!;
    private ILog _log = null!;
    private StationControl _uut = null!;

    [SetUp]
    public void Setup()
    {
        _door = Substitute.For<IDoor>();
        _rfidReader = Substitute.For<IRfidReader>();
        _charger = Substitute.For<IChargeControl>();
        _display = Substitute.For<IDisplay>();
        _log = Substitute.For<ILog>();
        _uut = new StationControl(_door, _rfidReader, _charger, _display, _log);
    }

    private void ScanRfid(int id)
    {
        _rfidReader.RfidDetected += Raise.EventWith(new RfidEventArgs { Id = id });
    }

    [Test]
    public void Ctor_UnlocksDoorAndShowsScanRfid()
    {
        _door.Received(1).UnlockDoor();
        _display.Received(1).ShowUserMessage("Indlæs RFID");
    }

    [Test]
    public void DoorOpened_Available_ShowsConnectPhone()
    {
        _door.DoorOpened += Raise.Event();

        _display.Received(1).ShowUserMessage("Tilslut telefon");
    }

    [Test]
    public void DoorClosed_DoorOpen_ShowsScanRfid()
    {
        _door.DoorOpened += Raise.Event();
        _display.ClearReceivedCalls();

        _door.DoorClosed += Raise.Event();

        _display.Received(1).ShowUserMessage("Indlæs RFID");
    }

    [Test]
    public void Rfid_AvailableAndConnected_LocksAndStartsCharging()
    {
        _charger.Connected.Returns(true);

        ScanRfid(42);

        _door.Received(1).LockDoor();
        _charger.Received(1).StartCharge();
        _log.Received(1).LogDoorLocked(42);
        _display.Received(1).ShowUserMessage("Ladeskab optaget");
    }

    [Test]
    public void Rfid_AvailableAndNotConnected_ShowsConnectionError()
    {
        _charger.Connected.Returns(false);

        ScanRfid(42);

        _display.Received(1).ShowUserMessage("Tilslutningsfejl");
        _door.DidNotReceive().LockDoor();
        _charger.DidNotReceive().StartCharge();
        _log.DidNotReceive().LogDoorLocked(Arg.Any<int>());
    }

    [Test]
    public void Rfid_LockedAndCorrectId_UnlocksAndStopsCharging()
    {
        _charger.Connected.Returns(true);
        ScanRfid(42);
        _display.ClearReceivedCalls();
        _door.ClearReceivedCalls();

        ScanRfid(42);

        _charger.Received(1).StopCharge();
        _door.Received(1).UnlockDoor();
        _log.Received(1).LogDoorUnlocked(42);
        _display.Received(1).ShowUserMessage("Fjern telefon");
    }

    [TestCase(41)]
    [TestCase(43)]
    [TestCase(0)]
    public void Rfid_LockedAndWrongId_ShowsRfidError(int wrongId)
    {
        _charger.Connected.Returns(true);
        ScanRfid(42);
        _display.ClearReceivedCalls();
        _door.ClearReceivedCalls();
        _charger.ClearReceivedCalls();
        _log.ClearReceivedCalls();

        ScanRfid(wrongId);

        _display.Received(1).ShowUserMessage("RFID fejl");
        _charger.DidNotReceive().StopCharge();
        _door.DidNotReceive().UnlockDoor();
        _log.DidNotReceive().LogDoorUnlocked(Arg.Any<int>());
    }

    [Test]
    public void DoorOpened_Locked_IsIgnored()
    {
        _charger.Connected.Returns(true);
        ScanRfid(42);
        _display.ClearReceivedCalls();

        _door.DoorOpened += Raise.Event();

        _display.DidNotReceive().ShowUserMessage(Arg.Any<string>());
    }

    [Test]
    public void DoorClosed_Available_IsIgnored()
    {
        _display.ClearReceivedCalls();

        _door.DoorClosed += Raise.Event();

        _display.DidNotReceive().ShowUserMessage(Arg.Any<string>());
    }

    [Test]
    public void Rfid_DoorOpen_IsIgnored()
    {
        _charger.Connected.Returns(true);
        _door.DoorOpened += Raise.Event();
        _display.ClearReceivedCalls();
        _door.ClearReceivedCalls();

        ScanRfid(42);

        _door.DidNotReceive().LockDoor();
        _charger.DidNotReceive().StartCharge();
        _log.DidNotReceive().LogDoorLocked(Arg.Any<int>());
        _display.DidNotReceive().ShowUserMessage(Arg.Any<string>());
    }

    [Test]
    public void Rfid_AfterUnlock_LocksAgainWithNewId()
    {
        _charger.Connected.Returns(true);
        ScanRfid(42);
        ScanRfid(42);
        _door.ClearReceivedCalls();
        _log.ClearReceivedCalls();

        ScanRfid(7);

        _door.Received(1).LockDoor();
        _log.Received(1).LogDoorLocked(7);
    }
}