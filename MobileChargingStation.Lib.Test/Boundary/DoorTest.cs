using MobileChargingStation.Lib.Boundary;

namespace MobileChargingStation.Lib.Test.Boundary;

[TestFixture]
public class DoorTest
{
    private Door _uut = null!;
    private int _openedCount;
    private int _closedCount;

    [SetUp]
    public void Setup()
    {
        _uut = new Door();
        _openedCount = 0;
        _closedCount = 0;
        _uut.DoorOpened += (_, _) => _openedCount++;
        _uut.DoorClosed += (_, _) => _closedCount++;
    }

    [Test]
    public void Ctor_DoorIsClosedAndUnlocked()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_uut.IsOpen, Is.False);
            Assert.That(_uut.IsLocked, Is.False);
        });
    }

    [Test]
    public void Ctor_NoEventsRaised()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_openedCount, Is.Zero);
            Assert.That(_closedCount, Is.Zero);
        });
    }

    [Test]
    public void OnDoorOpen_ClosedAndUnlocked_RaisesDoorOpenedOnce()
    {
        _uut.OnDoorOpen();

        Assert.That(_openedCount, Is.EqualTo(1));
    }

    [Test]
    public void OnDoorOpen_ClosedAndUnlocked_DoorIsOpen()
    {
        _uut.OnDoorOpen();

        Assert.That(_uut.IsOpen, Is.True);
    }

    [Test]
    public void OnDoorOpen_EventSenderIsDoor()
    {
        object? sender = null;
        _uut.DoorOpened += (s, _) => sender = s;

        _uut.OnDoorOpen();

        Assert.That(sender, Is.SameAs(_uut));
    }

    [Test]
    public void OnDoorClose_DoorOpen_RaisesDoorClosedOnce()
    {
        _uut.OnDoorOpen();

        _uut.OnDoorClose();

        Assert.Multiple(() =>
        {
            Assert.That(_closedCount, Is.EqualTo(1));
            Assert.That(_uut.IsOpen, Is.False);
        });
    }

    [Test]
    public void OnDoorClose_EventSenderIsDoor()
    {
        object? sender = null;
        _uut.DoorClosed += (s, _) => sender = s;
        _uut.OnDoorOpen();

        _uut.OnDoorClose();

        Assert.That(sender, Is.SameAs(_uut));
    }

    [Test]
    public void LockDoor_DoorClosed_DoorIsLocked()
    {
        _uut.LockDoor();

        Assert.That(_uut.IsLocked, Is.True);
    }

    [Test]
    public void UnlockDoor_DoorLocked_DoorIsUnlocked()
    {
        _uut.LockDoor();

        _uut.UnlockDoor();

        Assert.That(_uut.IsLocked, Is.False);
    }

    [Test]
    public void OnDoorOpen_DoorLocked_NoEventAndStaysClosed()
    {
        _uut.LockDoor();

        _uut.OnDoorOpen();

        Assert.Multiple(() =>
        {
            Assert.That(_openedCount, Is.Zero);
            Assert.That(_uut.IsOpen, Is.False);
        });
    }

    [Test]
    public void OnDoorOpen_AlreadyOpen_NoSecondEvent()
    {
        _uut.OnDoorOpen();

        _uut.OnDoorOpen();

        Assert.That(_openedCount, Is.EqualTo(1));
    }

    [Test]
    public void OnDoorClose_AlreadyClosed_NoEvent()
    {
        _uut.OnDoorClose();

        Assert.That(_closedCount, Is.Zero);
    }

    [Test]
    public void LockDoor_DoorOpen_DoorIsNotLocked()
    {
        _uut.OnDoorOpen();

        _uut.LockDoor();

        Assert.That(_uut.IsLocked, Is.False);
    }

    [Test]
    public void UnlockDoor_DoorNotLocked_StaysUnlocked()
    {
        _uut.UnlockDoor();

        Assert.That(_uut.IsLocked, Is.False);
    }

    [Test]
    public void OnDoorOpen_AfterLockThenUnlock_RaisesDoorOpened()
    {
        _uut.LockDoor();
        _uut.UnlockDoor();

        _uut.OnDoorOpen();

        Assert.That(_openedCount, Is.EqualTo(1));
    }

    [Test]
    public void OnDoorOpen_NoSubscribers_DoesNotThrow()
    {
        var door = new Door();

        Assert.DoesNotThrow(() =>
        {
            door.OnDoorOpen();
            door.OnDoorClose();
        });
    }

    [TestCase(2)]
    [TestCase(5)]
    [TestCase(100)]
    public void OpenCloseCycle_Many_RaisesEventPerCycle(int cycles)
    {
        for (var i = 0; i < cycles; i++)
        {
            _uut.OnDoorOpen();
            _uut.OnDoorClose();
        }

        Assert.Multiple(() =>
        {
            Assert.That(_openedCount, Is.EqualTo(cycles));
            Assert.That(_closedCount, Is.EqualTo(cycles));
        });
    }
}
