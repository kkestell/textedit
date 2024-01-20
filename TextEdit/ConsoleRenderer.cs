using System.Text;

namespace TextEdit;

class ConsoleRenderer : IDisposable
{
    private readonly View _view;
    private readonly Stream _stdout;

    public ConsoleRenderer(View view)
    {
        _view = view;

        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        _stdout = Console.OpenStandardOutput();
    }

    public void Dispose()
    {
        Console.CursorVisible = true;

        _stdout.Dispose();
    }

    public void Render()
    {
        Console.Clear();
        _view.Render(_stdout);
    }
}
