using System.Text;

namespace TextEdit.Tests;

[TestFixture]
public class DocumentTests
{
    [Test]
    public void Constructor_Default()
    {
        var doc = new Document();
        Assert.That(doc.GetText(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Constructor_Stream()
    {
        using var stream = new MemoryStream("line1\nline2"u8.ToArray());
        var doc = new Document(stream);
        Assert.That(doc.GetText(), Is.EqualTo("line1\nline2"));
    }

    [Test]
    public void Constructor_StringContent()
    {
        var doc = new Document("line1\nline2");
        Assert.That(doc.GetText(), Is.EqualTo("line1\nline2"));
    }

    [Test]
    public void Insert_ValidPosition()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        Assert.That(doc.GetText(), Is.EqualTo("a"));
    }

    [Test]
    public void Insert_NewLine()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.Insert('\n', 0, 1);
        Assert.That(doc.GetText(), Is.EqualTo("a\n"));
    }

    [Test]
    public void Insert_InvalidPosition()
    {
        var doc = new Document();
        doc.Insert('a', -1, 0);
        doc.Insert('b', 2, 0);
        Assert.That(doc.GetText(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void DeleteForward_MiddleOfLine()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.Insert('b', 0, 1);
        doc.Insert('c', 0, 2);
        doc.DeleteForward(0, 1); // Deletes 'b'
        Assert.That(doc.GetText(), Is.EqualTo("ac"));
    }

    [Test]
    public void DeleteForward_EndOfLine()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.Insert('\n', 0, 1);
        doc.Insert('b', 1, 0);
        doc.DeleteForward(0, 1); // Deletes newline, merges lines
        Assert.That(doc.GetText(), Is.EqualTo("ab"));
    }

    [Test]
    public void DeleteForward_LastLine()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.DeleteForward(0, 1); // No effect
        Assert.That(doc.GetText(), Is.EqualTo("a"));
    }

    [Test]
    public void DeleteForward_InvalidPosition()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.DeleteForward(-1, 0); // No effect
        doc.DeleteForward(1, 0); // No effect
        Assert.That(doc.GetText(), Is.EqualTo("a"));
    }

    [Test]
    public void DeleteBackward_MiddleOfLine()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.Insert('b', 0, 1);
        doc.Insert('c', 0, 2);
        doc.DeleteBackward(0, 2); // Deletes 'b'
        Assert.That(doc.GetText(), Is.EqualTo("ac"));
    }

    [Test]
    public void DeleteBackward_StartOfLine()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.Insert('\n', 0, 1);
        doc.Insert('b', 1, 0);
        doc.DeleteBackward(1, 0); // Merges lines
        Assert.That(doc.GetText(), Is.EqualTo("ab"));
    }

    [Test]
    public void DeleteBackward_FirstLine()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.DeleteBackward(0, 0); // No effect
        Assert.That(doc.GetText(), Is.EqualTo("a"));
    }

    [Test]
    public void DeleteBackward_InvalidPosition()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.DeleteBackward(-1, 0); // No effect
        doc.DeleteBackward(1, 0); // No effect
        Assert.That(doc.GetText(), Is.EqualTo("a"));
    }

    [Test]
    public void GetText_EmptyDocument()
    {
        var doc = new Document();
        Assert.That(doc.GetText(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetText_NonEmptyDocument()
    {
        var doc = new Document();
        doc.Insert('a', 0, 0);
        doc.Insert('b', 0, 1);
        doc.Insert('c', 0, 2);
        Assert.That(doc.GetText(), Is.EqualTo("abc"));
    }
}
