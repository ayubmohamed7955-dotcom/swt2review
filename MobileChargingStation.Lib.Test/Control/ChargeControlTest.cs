using MobileChargingStation.Lib.Boundary.Interfaces;
using MobileChargingStation.Lib.Control;
using NSubstitute;

namespace MobileChargingStation.Lib.Test.Control;

[TestFixture]
public class ChargeControlTest
{
    private ChargeControl _uut = null!;
    private IUsbCharger _usbCharger = null!;
    private IDisplay _display = null!;

    [SetUp]
    public void Setup()
    {
        _usbCharger = Substitute.For<IUsbCharger>();
        _display = Substitute.For<IDisplay>();
        _uut = new ChargeControl(_usbCharger, _display);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Connected_UsbChargerConnectedIs_ReturnsSame(bool connected)
    {
        _usbCharger.Connected.Returns(connected);

        Assert.That(_uut.Connected, Is.EqualTo(connected));
    }

    [Test]
    public void StartCharge_Idle_CallsUsbStartCharge()
    {
        _uut.StartCharge();

        _usbCharger.Received(1).StartCharge();
    }

    [Test]
    public void StartCharge_AlreadyCharging_UsbStartChargeCalledOnlyOnce()
    {
        _uut.StartCharge();
        _uut.StartCharge();

        _usbCharger.Received(1).StartCharge();
    }

    [Test]
    public void StopCharge_Idle_UsbStopChargeNotCalled()
    {
        _uut.StopCharge();

        _usbCharger.DidNotReceive().StopCharge();
    }

    [Test]
    public void StopCharge_Charging_CallsUsbStopCharge()
    {
        _uut.StartCharge();

        _uut.StopCharge();

        _usbCharger.Received(1).StopCharge();
    }

    [Test]
    public void StopCharge_Charging_ClearsChargeMessage()
    {
        _uut.StartCharge();

        _uut.StopCharge();

        _display.Received(1).ShowChargeMessage("");
    }

    [Test]
    public void StopCharge_ThenStartCharge_UsbStartChargeCalledAgain()
    {
        _uut.StartCharge();
        _uut.StopCharge();

        _uut.StartCharge();

        _usbCharger.Received(2).StartCharge();
    }



    // --- Charging: grænseværdier (BVA) ---

    [TestCase(5.01)]
    [TestCase(250)]
    [TestCase(499.99)]
    [TestCase(500)]
    public void CurrentEvent_ChargingNormalCurrent_ShowsCharging(double current)
    {
        _uut.StartCharge();

        RaiseCurrent(current);

        _display.Received(1).ShowChargeMessage("Telefon lader");
    }

    [TestCase(0.01)]
    [TestCase(2.5)]
    [TestCase(4.99)]
    [TestCase(5)]
    public void CurrentEvent_ChargingLowCurrent_ShowsFullyCharged(double current)
    {
        _uut.StartCharge();

        RaiseCurrent(current);

        _display.Received(1).ShowChargeMessage("Telefon er fuldt opladet");
    }

    [TestCase(500.01)]
    [TestCase(750)]
    public void CurrentEvent_ChargingOverload_ShowsError(double current)
    {
        _uut.StartCharge();

        RaiseCurrent(current);

        _display.Received(1).ShowChargeMessage("Fejl i opladning");
    }

    [TestCase(500.01)]
    [TestCase(750)]
    public void CurrentEvent_ChargingOverload_StopsUsbCharger(double current)
    {
        _uut.StartCharge();

        RaiseCurrent(current);

        _usbCharger.Received(1).StopCharge();
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void CurrentEvent_ChargingZeroOrBelow_ClearsChargeMessage(double current)
    {
        _uut.StartCharge();

        RaiseCurrent(current);

        _display.Received(1).ShowChargeMessage("");
    }

    // --- Andre tilstande ---

    [Test]
    public void CurrentEvent_Idle_NothingShown()
    {
        RaiseCurrent(250);

        _display.DidNotReceiveWithAnyArgs().ShowChargeMessage(default!);
    }

    [Test]
    public void CurrentEvent_FullyChargedLowCurrentAgain_MessageShownOnlyOnce()
    {
        _uut.StartCharge();
        RaiseCurrent(2.5);
        _display.ClearReceivedCalls();

        RaiseCurrent(2.5);

        _display.DidNotReceiveWithAnyArgs().ShowChargeMessage(default!);
    }

    [Test]
    public void CurrentEvent_FullyChargedOverload_StopsUsbCharger()
    {
        _uut.StartCharge();
        RaiseCurrent(2.5);

        RaiseCurrent(750);

        _usbCharger.Received(1).StopCharge();
    }

    [Test]
    public void CurrentEvent_ErrorNormalCurrent_NothingMoreShown()
    {
        _uut.StartCharge();
        RaiseCurrent(750);
        _display.ClearReceivedCalls();

        RaiseCurrent(250);

        _display.DidNotReceiveWithAnyArgs().ShowChargeMessage(default!);
    }

    [Test]
    public void StartCharge_FullyCharged_Ignored()
    {
        _uut.StartCharge();
        RaiseCurrent(2.5);

        _uut.StartCharge();

        _usbCharger.Received(1).StartCharge();
    }

    [Test]
    public void StopCharge_FullyCharged_CallsUsbStopCharge()
    {
        _uut.StartCharge();
        RaiseCurrent(2.5);

        _uut.StopCharge();

        _usbCharger.Received(1).StopCharge();
    }

    [Test]
    public void StopCharge_Error_CanStartChargeAgain()
    {
        _uut.StartCharge();
        RaiseCurrent(750);
        _uut.StopCharge();

        _uut.StartCharge();

        _usbCharger.Received(2).StartCharge();
    }


    // --- Samme besked vises kun én gang ---

    [Test]
    public void CurrentEvent_ChargingNormalCurrentTwice_ShowsChargingOnlyOnce()
    {
        _uut.StartCharge();

        RaiseCurrent(250);
        RaiseCurrent(200);

        _display.Received(1).ShowChargeMessage("Telefon lader");
    }

    [Test]
    public void CurrentEvent_ChargingZeroTwice_ClearsOnlyOnce()
    {
        _uut.StartCharge();

        RaiseCurrent(0);
        RaiseCurrent(0);

        _display.Received(1).ShowChargeMessage("");
    }

    [Test]
    public void CurrentEvent_ChargeAgainAfterStop_ShowsChargingAgain()
    {
        _uut.StartCharge();
        RaiseCurrent(250);
        _uut.StopCharge();
        _uut.StartCharge();

        RaiseCurrent(250);

        _display.Received(2).ShowChargeMessage("Telefon lader");
    }

    private void RaiseCurrent(double milliAmpere)
    {
        _usbCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs { Current = milliAmpere });
    }

}
