namespace TextEdit;

class Editor : IDisposable
{
    private readonly KeyboardHandler _keyboardHandler;
    private readonly ConsoleRenderer _consoleRenderer;

    private bool _running = true;
    private Document _document;
    private View _view;


    public Editor(string filePath)
    {
        _document = new Document(File.OpenRead(filePath));
        _view = new View(_document);

        _keyboardHandler = new KeyboardHandler(this);
        _consoleRenderer = new ConsoleRenderer();
    }

    public void Run()
    {
        _consoleRenderer.Render(_view);

        while (_running)
        {
            if (_keyboardHandler.HandleKeyboardInput())
            {
                _consoleRenderer.Render(_view);
            }
        }
    }

    public void Command(EditorCommand command, char? ch = null)
    {
        switch (command)
        {
            case EditorCommand.NewDocument:
                NewDocument();
                break;
            case EditorCommand.Quit:
                Quit();
                break;
            // Certain commands are delegated to the view because their implementation
            // depends on the view's state (e.g. the cursor position).
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
            case EditorCommand.MoveToPreviousWord:
            case EditorCommand.MoveToNextWord:
            case EditorCommand.MoveToStartOfDocument:
            case EditorCommand.MoveToEndOfDocument:
            case EditorCommand.MovePageUp:
            case EditorCommand.MovePageDown:
            case EditorCommand.DeleteLine:
                _view.Command(command, ch);
                break;
        }
    }

    private void NewDocument()
    {
        _document = new Document();
        _view = new View(_document);
    }

    private void OpenDocument(string filePath)
    {
        _document = new Document(File.OpenRead(filePath));
        _view = new View(_document);
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
