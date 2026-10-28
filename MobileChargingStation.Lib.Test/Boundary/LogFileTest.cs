using MobileChargingStation.Lib.Boundary;

namespace MobileChargingStation.Lib.Test.Boundary;

[TestFixture]
public class LogFileTest
{
    private string _path = null!;
    private DateTime _now;
    private LogFile _uut = null!;

    [SetUp]
    public void Setup()
    {
        _path = Path.Combine(Path.GetTempPath(), $"logtest_{Guid.NewGuid()}.txt");
        _now = new DateTime(2026, 10, 22, 14, 3, 22);
        _uut = new LogFile(_path, () => _now);
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }

    private string[] Lines() => File.ReadAllLines(_path);

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Ctor_InvalidPath_Throws(string? path)
    {
        Assert.That(() => new LogFile(path!), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public void Ctor_DoesNotCreateFile()
    {
        Assert.That(File.Exists(_path), Is.False);
    }

    [Test]
    public void LogDoorLocked_WritesTimestampIdAndText()
    {
        _uut.LogDoorLocked(42);

        Assert.That(Lines(), Is.EqualTo(new[] { "14:03:22: Skab låst med RFID: 42" }));
    }

    [Test]
    public void LogDoorUnlocked_WritesTimestampIdAndText()
    {
        _uut.LogDoorUnlocked(42);

        Assert.That(Lines(), Is.EqualTo(new[] { "14:03:22: Skab låst op med RFID: 42" }));
    }

    [TestCase(0, 0, 0, "00:00:00")]
    [TestCase(9, 5, 7, "09:05:07")]
    [TestCase(23, 59, 59, "23:59:59")]
    public void LogDoorLocked_Time_FormattedAsHoursMinutesSeconds(int h, int m, int s, string expected)
    {
        _now = new DateTime(2026, 10, 22, h, m, s);

        _uut.LogDoorLocked(1);

        Assert.That(Lines()[0], Does.StartWith($"{expected}: "));
    }

    [TestCase(int.MinValue)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(int.MaxValue)]
    public void LogDoorUnlocked_Id_WrittenUnchanged(int id)
    {
        _uut.LogDoorUnlocked(id);

        Assert.That(Lines()[0], Does.EndWith($"RFID: {id}"));
    }

    [Test]
    public void DefaultClock_WritesTimestamp()
    {
        var uut = new LogFile(_path);

        uut.LogDoorLocked(1);

        Assert.That(Lines()[0], Does.Match(@"^\d{2}:\d{2}:\d{2}: Skab låst med RFID: 1$"));
    }

    [Test]
    public void LockThenUnlock_AppendsInOrder()
    {
        _uut.LogDoorLocked(7);
        _now = _now.AddMinutes(30);
        _uut.LogDoorUnlocked(7);

        Assert.That(Lines(), Is.EqualTo(new[]
        {
            "14:03:22: Skab låst med RFID: 7",
            "14:33:22: Skab låst op med RFID: 7"
        }));
    }

    [Test]
    public void ExistingFile_IsAppendedNotOverwritten()
    {
        File.WriteAllText(_path, "eksisterende linje" + Environment.NewLine);

        _uut.LogDoorLocked(1);

        Assert.That(Lines(), Has.Length.EqualTo(2));
        Assert.That(Lines()[0], Is.EqualTo("eksisterende linje"));
    }

    [TestCase(10)]
    [TestCase(100)]
    public void LogDoorLocked_Many_OneLineEach(int count)
    {
        for (var i = 0; i < count; i++)
            _uut.LogDoorLocked(i);

        Assert.That(Lines(), Has.Length.EqualTo(count));
    }
}
