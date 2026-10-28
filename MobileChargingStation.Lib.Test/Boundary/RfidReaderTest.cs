using MobileChargingStation.Lib.Boundary;
using MobileChargingStation.Lib.Boundary.Interfaces;

namespace MobileChargingStation.Lib.Test.Boundary;

[TestFixture]
public class RfidReaderTest
{
    private RfidReader _uut = null!;
    private List<RfidEventArgs> _received = null!;

    [SetUp]
    public void Setup()
    {
        _uut = new RfidReader();
        _received = new List<RfidEventArgs>();
        _uut.RfidDetected += (_, e) => _received.Add(e);
    }

    [Test]
    public void Ctor_NoEventRaised()
    {
        Assert.That(_received, Is.Empty);
    }

    [Test]
    public void OnRfidRead_NoSubscribers_DoesNotThrow()
    {
        var reader = new RfidReader();

        Assert.DoesNotThrow(() => reader.OnRfidRead(1));
    }

    [Test]
    public void OnRfidRead_RaisesEventOnce()
    {
        _uut.OnRfidRead(42);

        Assert.That(_received, Has.Count.EqualTo(1));
    }

    [Test]
    public void OnRfidRead_EventSenderIsReader()
    {
        object? sender = null;
        _uut.RfidDetected += (s, _) => sender = s;

        _uut.OnRfidRead(42);

        Assert.That(sender, Is.SameAs(_uut));
    }

    [TestCase(int.MinValue)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(int.MaxValue)]
    public void OnRfidRead_Id_EventContainsSameId(int id)
    {
        _uut.OnRfidRead(id);

        Assert.That(_received[0].Id, Is.EqualTo(id));
    }

    [Test]
    public void OnRfidRead_ManyReads_EventPerReadInOrder()
    {
        int[] ids = [10, 20, 10, 30];

        foreach (var id in ids)
            _uut.OnRfidRead(id);

        Assert.That(_received.Select(e => e.Id), Is.EqualTo(ids));
    }

    [Test]
    public void OnRfidRead_MultipleSubscribers_AllNotified()
    {
        var secondCount = 0;
        _uut.RfidDetected += (_, _) => secondCount++;

        _uut.OnRfidRead(5);

        Assert.Multiple(() =>
        {
            Assert.That(_received, Has.Count.EqualTo(1));
            Assert.That(secondCount, Is.EqualTo(1));
        });
    }
}
