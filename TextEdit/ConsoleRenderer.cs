using System.Text;

namespace TextEdit;

class ConsoleRenderer : IDisposable
{
    private readonly Stream _stdout;

    public ConsoleRenderer()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        _stdout = Console.OpenStandardOutput();
    }

    public void Dispose()
    {
        Console.CursorVisible = true;

        _stdout.Dispose();
    }

    public void Render(View view)
    {
        Console.Clear();
        view.Render(_stdout);
    }
}
