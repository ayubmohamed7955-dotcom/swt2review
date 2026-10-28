using MobileChargingStation.Lib.Boundary;

namespace MobileChargingStation.Lib.Test.Boundary;

[TestFixture]
public class DisplayTest
{
    private Display _uut = null!;
    private StringWriter _output = null!;
    private TextWriter _originalOut = null!;

    [SetUp]
    public void Setup()
    {
        _originalOut = Console.Out;
        _output = new StringWriter();
        Console.SetOut(_output);
        _uut = new Display();
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(_originalOut);
        _output.Dispose();
    }

    private string[] Lines() =>
        _output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

    [Test]
    public void Ctor_WritesNothing()
    {
        Assert.That(_output.ToString(), Is.Empty);
    }

    [Test]
    public void ShowUserMessage_EmptyMessage_WritesOnlyPrefix()
    {
        _uut.ShowUserMessage("");

        Assert.That(_output.ToString(), Is.EqualTo("[Bruger] " + Environment.NewLine));
    }

    [Test]
    public void ShowChargeMessage_EmptyMessage_WritesOnlyPrefix()
    {
        _uut.ShowChargeMessage("");

        Assert.That(_output.ToString(), Is.EqualTo("[Ladning] " + Environment.NewLine));
    }

    [TestCase("Tilslut telefon")]
    [TestCase("Indlæs RFID")]
    [TestCase("Forkert RFID tag")]
    public void ShowUserMessage_WritesUserPrefixAndMessage(string message)
    {
        _uut.ShowUserMessage(message);

        Assert.That(_output.ToString(), Is.EqualTo($"[Bruger] {message}{Environment.NewLine}"));
    }

    [TestCase("Telefonen lades")]
    [TestCase("Telefonen er fuldt opladet")]
    [TestCase("Fejl: kortslutning")]
    public void ShowChargeMessage_WritesChargePrefixAndMessage(string message)
    {
        _uut.ShowChargeMessage(message);

        Assert.That(_output.ToString(), Is.EqualTo($"[Ladning] {message}{Environment.NewLine}"));
    }

    [Test]
    public void ShowUserMessage_DoesNotWriteChargePrefix()
    {
        _uut.ShowUserMessage("Hej");

        Assert.That(_output.ToString(), Does.Not.Contain("[Ladning]"));
    }

    [Test]
    public void ShowChargeMessage_DoesNotWriteUserPrefix()
    {
        _uut.ShowChargeMessage("Hej");

        Assert.That(_output.ToString(), Does.Not.Contain("[Bruger]"));
    }

    [Test]
    public void MixedMessages_WrittenInOrderWithCorrectPrefix()
    {
        _uut.ShowUserMessage("a");
        _uut.ShowChargeMessage("b");
        _uut.ShowUserMessage("c");

        Assert.That(Lines(), Is.EqualTo(new[] { "[Bruger] a", "[Ladning] b", "[Bruger] c" }));
    }

    [TestCase(2)]
    [TestCase(10)]
    public void ShowChargeMessage_Many_OneLinePerMessage(int count)
    {
        for (var i = 0; i < count; i++)
            _uut.ShowChargeMessage($"msg {i}");

        Assert.That(Lines(), Has.Length.EqualTo(count));
    }
}
