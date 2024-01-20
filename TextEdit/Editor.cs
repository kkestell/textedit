namespace TextEdit;

class Editor : IDisposable
{
    private readonly Document _document;
    private readonly View _view;
    private readonly KeyboardHandler _keyboardHandler;
    private readonly ConsoleRenderer _consoleRenderer;

    private bool _running = true;

    public Editor()
    {
        _document = new Document(File.OpenRead(@"D:\src\TextEdit\TextEdit\Document.cs"));
        _view = new View(_document);
        _keyboardHandler = new KeyboardHandler(this);
        _consoleRenderer = new ConsoleRenderer(_view);
    }

    public void Run()
    {
        _consoleRenderer.Render();

        while (_running)
        {
            if (_keyboardHandler.HandleKeyboardInput())
            {
                _consoleRenderer.Render();
            }
        }
    }

    public void Command(EditorCommand command, char? ch = null)
    {
        switch (command)
        {
            case EditorCommand.Quit:
                Quit();
                break;
            case EditorCommand.InsertCharacter:
            case EditorCommand.MoveUp:
            case EditorCommand.MoveDown:
            case EditorCommand.MoveLeft:
            case EditorCommand.MoveRight:
            case EditorCommand.DeletePreviousCharacter:
            case EditorCommand.DeleteNextCharacter:
            case EditorCommand.Indent:
            case EditorCommand.Unindent:
            case EditorCommand.MoveToEndOfLine:
            case EditorCommand.MoveToStartOfLine:
                _view.Command(command, ch);
                break;
        }
    }

    private void Quit()
    {
        _running = false;
    }

    public void Dispose()
    {
        _consoleRenderer.Dispose();
    }
}
