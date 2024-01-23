namespace TextEdit;

class KeyboardHandler
{
    private readonly Editor _editor;

    public KeyboardHandler(Editor editor)
    {
        _editor = editor;

        Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, e) => {
            e.Cancel = true;
            _editor.Command(EditorCommand.Copy);
        });
    }

    public bool HandleKeyboardInput()
    {
        if (Console.KeyAvailable)
        {
            var keyInfo = Console.ReadKey(true);
            HandleKeyPress(keyInfo);

            return true;
        }

        return false;
    }

    private void HandleKeyPress(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Q:
                    _editor.Command(EditorCommand.Quit);
                    break;
                case ConsoleKey.X:
                    _editor.Command(EditorCommand.Cut);
                    break;
                case ConsoleKey.V:
                    _editor.Command(EditorCommand.Paste);
                    break;
                case ConsoleKey.Z:
                    _editor.Command(EditorCommand.Undo);
                    break;
                case ConsoleKey.Y:
                    _editor.Command(EditorCommand.Redo);
                    break;
                case ConsoleKey.A:
                    _editor.Command(EditorCommand.SelectAll);
                    break;
                case ConsoleKey.F:
                    _editor.Command(EditorCommand.Find);
                    break;
                case ConsoleKey.H:
                    _editor.Command(EditorCommand.Replace);
                    break;
                case ConsoleKey.S:
                    _editor.Command(EditorCommand.Save);
                    break;
                case ConsoleKey.O:
                    _editor.Command(EditorCommand.Open);
                    break;
                case ConsoleKey.N:
                    _editor.Command(EditorCommand.NewDocument);
                    break;
                case ConsoleKey.P:
                    _editor.Command(EditorCommand.Print);
                    break;
                case ConsoleKey.W:
                    _editor.Command(EditorCommand.CloseDocument);
                    break;
                case ConsoleKey.RightArrow:
                    _editor.Command(EditorCommand.MoveToNextWord);
                    break;
                case ConsoleKey.LeftArrow:
                    _editor.Command(EditorCommand.MoveToPreviousWord);
                    break;
                case ConsoleKey.Home:
                    _editor.Command(EditorCommand.MoveToStartOfDocument);
                    break;
                case ConsoleKey.End:
                    _editor.Command(EditorCommand.MoveToEndOfDocument);
                    break;
            }
        }
        else if (keyInfo.Modifiers == ConsoleModifiers.Shift)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.RightArrow:
                    _editor.Command(EditorCommand.SelectRightCharacter);
                    break;
                case ConsoleKey.LeftArrow:
                    _editor.Command(EditorCommand.SelectLeftCharacter);
                    break;
                case ConsoleKey.UpArrow:
                    _editor.Command(EditorCommand.SelectUpLine);
                    break;
                case ConsoleKey.DownArrow:
                    _editor.Command(EditorCommand.SelectDownLine);
                    break;
                case ConsoleKey.Tab:
                    _editor.Command(EditorCommand.Unindent);
                    break;
                case ConsoleKey.Delete:
                    _editor.Command(EditorCommand.DeleteLine);
                    break;
                default:
                    var ch = keyInfo.KeyChar;
                    _editor.Command(EditorCommand.InsertCharacter, ch);
                    break;
            }
        }
        else if (keyInfo.Modifiers == (ConsoleModifiers.Control | ConsoleModifiers.Shift))
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.RightArrow:
                    _editor.Command(EditorCommand.SelectRightWord);
                    break;
                case ConsoleKey.LeftArrow:
                    _editor.Command(EditorCommand.SelectLeftWord);
                    break;
                case ConsoleKey.F:
                    _editor.Command(EditorCommand.FindPrevious);
                    break;
            }
        }
        else
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.RightArrow:
                    _editor.Command(EditorCommand.MoveRight);
                    break;
                case ConsoleKey.LeftArrow:
                    _editor.Command(EditorCommand.MoveLeft);
                    break;
                case ConsoleKey.UpArrow:
                    _editor.Command(EditorCommand.MoveUp);
                    break;
                case ConsoleKey.DownArrow:
                    _editor.Command(EditorCommand.MoveDown);
                    break;
                case ConsoleKey.Home:
                    _editor.Command(EditorCommand.MoveToStartOfLine);
                    break;
                case ConsoleKey.End:
                    _editor.Command(EditorCommand.MoveToEndOfLine);
                    break;
                case ConsoleKey.Backspace:
                    _editor.Command(EditorCommand.DeletePreviousCharacter);
                    break;
                case ConsoleKey.Delete:
                    _editor.Command(EditorCommand.DeleteNextCharacter);
                    break;
                case ConsoleKey.Enter:
                    _editor.Command(EditorCommand.InsertCharacter, '\n');
                    break;
                case ConsoleKey.Tab:
                    _editor.Command(EditorCommand.Indent);
                    break;
                case ConsoleKey.PageUp:
                    _editor.Command(EditorCommand.MovePageUp);
                    break;
                case ConsoleKey.PageDown:
                    _editor.Command(EditorCommand.MovePageDown);
                    break;
                default:
                    var ch = keyInfo.KeyChar;
                    _editor.Command(EditorCommand.InsertCharacter, ch);
                    break;
            }
        }
    }
}
