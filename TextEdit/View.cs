using System.Text;

namespace TextEdit;

class View
{
    private const string BoldOn = "\x1b[1m";
    private const string BoldOff = "\x1b[22m";
    private const string InvertedOn = "\x1b[7m";
    private const string InvertedOff = "\x1b[27m";

    private readonly Document _document;
    private readonly StringBuilder _bufferString = new();

    private Cell[] _buffer;

    private int _scrollRow;
    private int _scrollCol;

    private int _cursorRow;
    private int _cursorCol;

    public View(Document document)
    {
        _document = document;
        InitializeBuffer();
    }

    private string CurrentLine => _document.Lines.ElementAt(_cursorRow);

    private char CurrentCharacter
    {
        get
        {
            if (_cursorCol < CurrentLine.Length)
            {
                return CurrentLine[_cursorCol];
            }
            else
            {
                return '\n';
            }
        }
    }

    private char? PreviousCharacter
    {
        get
        {
            if (_cursorCol > 0)
            {
                return CurrentLine[_cursorCol - 1];
            }
            else if (_cursorRow > 0)
            {
                return '\n';
            }
            else
            {
                return null;
            }
        }
    }

    private char? NextCharacter
    {
        get
        {
            if (_cursorCol < CurrentLine.Length - 1)
            {
                return CurrentLine[_cursorCol + 1];
            }
            else if (_cursorRow < _document.Lines.Count() - 1)
            {
                return '\n';
            }
            else
            {
                return null;
            }
        }
    }

    private bool IsCurrentCharacterWhitespace => char.IsWhiteSpace(CurrentCharacter);

    public void Render(Stream stream)
    {
        var rows = Console.WindowHeight;
        var cols = Console.WindowWidth;

        if (rows * cols != _buffer.Length)
        {
            InitializeBuffer();
        }

        var lines = _document.Lines.Skip(_scrollRow);

        // Clear buffer and reset formatting
        foreach (var cell in _buffer)
        {
            cell.Value = ' ';
            cell.Bold = false;
            cell.Inverted = false;
        }

        var bufferIndex = 0;
        foreach (var line in lines)
        {
            if (bufferIndex / cols >= rows)
            {
                break;
            }

            for (var col = 0; col < cols; col++)
            {
                var lineIndex = col + _scrollCol;

                // Invert cell at cursor position
                if (bufferIndex / cols == _cursorRow - _scrollRow && col == _cursorCol - _scrollCol)
                {
                    _buffer[bufferIndex].Inverted = true;
                }

                if (lineIndex < line.Length)
                {
                    _buffer[bufferIndex].Value = line[lineIndex];
                }
                else
                {
                    _buffer[bufferIndex].Value = ' ';
                }

                bufferIndex++;

                if (col == cols - 1 && bufferIndex % cols != 0)
                {
                    bufferIndex += cols - (bufferIndex % cols);
                }
            }
        }

        // Convert buffer to string with ANSI codes
        _bufferString.Clear();
        foreach (var cell in _buffer)
        {
            if (cell.Bold)
            {
                _bufferString.Append(BoldOn);
            }

            if (cell.Inverted)
            {
                _bufferString.Append(InvertedOn);
            }

            _bufferString.Append(cell.Value);

            if (cell.Inverted)
            {
                _bufferString.Append(InvertedOff);
            }

            if (cell.Bold)
            {
                _bufferString.Append(BoldOff);
            }
        }

        stream.Write(Encoding.UTF8.GetBytes(_bufferString.ToString()), 0, _bufferString.Length);
    }

    public void Command(EditorCommand command, char? ch = null)
    {
        switch (command)
        {
            case EditorCommand.InsertCharacter:
                if (ch.HasValue)
                {
                    Insert(ch.Value);
                }
                break;
            case EditorCommand.MoveUp:
                MoveCursorUp();
                break;
            case EditorCommand.MoveDown:
                MoveCursorDown();
                break;
            case EditorCommand.MoveLeft:
                MoveCursorLeft();
                break;
            case EditorCommand.MoveRight:
                MoveCursorRight();
                break;
            case EditorCommand.DeleteNextCharacter:
                DeleteNextCharacter();
                break;
            case EditorCommand.DeletePreviousCharacter:
                DeletePreviousCharacter();
                break;
            case EditorCommand.DeleteLine:
                DeleteLine();
                break;
            case EditorCommand.Indent:
                Indent();
                break;
            case EditorCommand.Unindent:
                Unindent();
                break;
            case EditorCommand.MoveToStartOfLine:
                MoveToStartOfLine();
                break;
            case EditorCommand.MoveToEndOfLine:
                MoveToEndOfLine();
                break;
            case EditorCommand.Copy:
            case EditorCommand.Cut:
            case EditorCommand.Paste:
            case EditorCommand.Undo:
            case EditorCommand.Redo:
            case EditorCommand.SelectAll:
            case EditorCommand.Find:
            case EditorCommand.FindPrevious:
            case EditorCommand.Replace:
            case EditorCommand.Save:
            case EditorCommand.Open:
            case EditorCommand.NewDocument:
            case EditorCommand.Print:
            case EditorCommand.CloseDocument:
                break;
            case EditorCommand.MoveToNextWord:
                MoveToNextWord();
                break;
            case EditorCommand.MoveToPreviousWord:
                MoveToPreviousWord();
                break;
            case EditorCommand.MovePageUp:
                PageUp();
                break;
            case EditorCommand.MovePageDown:
                PageDown();
                break;
            case EditorCommand.SelectLeftCharacter:
            case EditorCommand.SelectRightCharacter:
            case EditorCommand.SelectUpLine:
            case EditorCommand.SelectDownLine:
            case EditorCommand.SelectLeftWord:
            case EditorCommand.SelectRightWord:
            case EditorCommand.SelectToStartOfDocument:
            case EditorCommand.SelectToEndOfDocument:
                break;
            case EditorCommand.MoveToStartOfDocument:
                MoveToStartOfDocument();
                break;
            case EditorCommand.MoveToEndOfDocument:
                MoveToEndOfDocument();
                break;
            case EditorCommand.ToggleInsertMode:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }

        ClampCursorToCurrentLine();
        ScrollToCursor();
    }

    private void InitializeBuffer()
    {
        var rows = Console.WindowHeight;
        var cols = Console.WindowWidth;

        _buffer = new Cell[rows * cols];

        for (int i = 0; i < _buffer.Length; i++)
        {
            _buffer[i] = new Cell { X = i % cols, Y = i / cols, Value = ' ', Bold = false };
        }
    }

    private void Insert(char ch)
    {
        _document.Insert(ch, _cursorRow, _cursorCol);
        MoveCursorRight();
    }

    private void MoveCursorUp()
    {
        if (_cursorRow > 0)
        {
            _cursorRow--;
            _cursorCol = Math.Min(_cursorCol, CurrentLine.Length);
        }
    }

    private void MoveCursorDown()
    {
        if (_cursorRow < _document.Lines.Count() - 1)
        {
            _cursorRow++;
            _cursorCol = Math.Min(_cursorCol, CurrentLine.Length);
        }
    }

    private void MoveCursorLeft()
    {
        if (_cursorCol > 0)
        {
            _cursorCol--;
        }
        else if (_cursorRow > 0)
        {
            _cursorRow--;
            _cursorCol = CurrentLine.Length;
        }
    }

    private void MoveCursorRight()
    {
        if (_cursorCol < CurrentLine.Length)
        {
            _cursorCol++;
        }
        else if (_cursorRow < _document.Lines.Count - 1)
        {
            _cursorRow++;
            _cursorCol = 0;
        }
    }

    private void DeleteNextCharacter()
    {
        _document.DeleteForward(_cursorRow, _cursorCol);
    }

    private void DeletePreviousCharacter()
    {
        var destCol = _document.Lines.ElementAtOrDefault(_cursorRow - 1)?.Length ?? 0;

        _document.DeleteBackward(_cursorRow, _cursorCol);

        if (_cursorCol > 0)
        {
            _cursorCol--;
        }
        else if (_cursorRow > 0)
        {
            _cursorRow--;
            _cursorCol = destCol;
        }
    }

    private void DeleteLine()
    {
        _document.DeleteLine(_cursorRow);
    }

    private void Indent()
    {
        _document.Indent(_cursorRow);

        _cursorCol += 4;
    }

    private void Unindent()
    {
        if (!_document.Unindent(_cursorRow))
            return;

        if (_cursorCol >= 4)
        {
            _cursorCol -= 4;
        }
        else
        {
            _cursorCol = 0;
        }
    }

    private void MoveToStartOfLine()
    {
        _cursorCol = 0;
    }

    private void MoveToEndOfLine()
    {
        _cursorCol = _document.Lines.ElementAtOrDefault(_cursorRow)?.Length ?? 0;
    }

    private void MoveToNextWord()
    {
        if (char.IsWhiteSpace(CurrentCharacter))
        {
            MoveCursorRightUntil(() => !IsCurrentCharacterWhitespace);
        }
        else
        {
            if (char.IsWhiteSpace(NextCharacter ?? 'X'))
            {
                MoveCursorRight();
                MoveCursorRightUntil(() => !IsCurrentCharacterWhitespace);
            }
            else
            {
                if (!NextCharacter.HasValue)
                {
                    return;
                }

                MoveCursorRightUntil(() => IsCurrentCharacterWhitespace);
                MoveCursorRightUntil(() => !IsCurrentCharacterWhitespace);
            }
        }
    }

    private void MoveToPreviousWord()
    {
        if (char.IsWhiteSpace(CurrentCharacter))
        {
            MoveCursorLeftUntil(() => !IsCurrentCharacterWhitespace);
            MoveCursorLeftUntil(() => IsCurrentCharacterWhitespace);
        }
        else
        {
            if (char.IsWhiteSpace(PreviousCharacter ?? 'X'))
            {
                MoveCursorLeft();
                MoveCursorLeftUntil(() => !IsCurrentCharacterWhitespace);
            }

            MoveCursorLeftUntil(() => IsCurrentCharacterWhitespace);
        }

        if (_cursorRow != 0 || _cursorCol != 0)
        {
            MoveCursorRight();
        }
    }

    private void PageUp()
    {
        var rows = Console.WindowHeight;

        if (_cursorRow >= rows)
        {
            _cursorRow -= rows;
            _scrollRow -= rows;
        }
        else
        {
            _cursorRow = 0;
            _scrollRow = 0;
        }
    }

    private void PageDown()
    {
        var rows = Console.WindowHeight;

        if (_cursorRow < _document.Lines.Count() - rows)
        {
            _cursorRow += rows;
            _scrollRow += rows;
        }
        else
        {
            _cursorRow = _document.Lines.Count() - 1;
            _scrollRow = _cursorRow - rows + 1;
        }

        if (_scrollRow > _document.Lines.Count() - rows)
        {
            _scrollRow = _document.Lines.Count() - rows;
        }
    }

    private void MoveToStartOfDocument()
    {
        _cursorRow = 0;
        _cursorCol = 0;
    }

    private void MoveToEndOfDocument()
    {
        _cursorRow = _document.Lines.Count() - 1;
        _cursorCol = CurrentLine.Length;
    }

    private void ScrollToCursor()
    {
        var rows = Console.WindowHeight;
        var cols = Console.WindowWidth;

        if (_cursorRow < _scrollRow)
        {
            _scrollRow = _cursorRow;
        }
        else if (_cursorRow >= _scrollRow + rows)
        {
            _scrollRow = _cursorRow - rows + 1;
        }

        if (_cursorCol < _scrollCol)
        {
            _scrollCol = _cursorCol;
        }
        else if (_cursorCol >= _scrollCol + cols)
        {
            _scrollCol = _cursorCol - cols + 1;
        }
    }

    private void ClampCursorToCurrentLine()
    {
        if (_cursorRow < 0)
        {
            _cursorRow = 0;
        }
        else if (_cursorRow >= _document.Lines.Count())
        {
            _cursorRow = _document.Lines.Count() - 1;
        }

        _cursorCol = Math.Min(_cursorCol, CurrentLine.Length);
    }

    private void MoveCursorLeftUntil(Func<bool> condition)
    {
        while (!condition())
        {
            if (_cursorRow == 0 && _cursorCol == 0)
            {
                break;
            }

            MoveCursorLeft();
        }
    }

    private void MoveCursorRightUntil(Func<bool> condition)
    {
        while (!condition())
        {
            if (_cursorRow == _document.Lines.Count - 1 && _cursorCol == CurrentLine.Length)
            {
                break;
            }

            MoveCursorRight();
        }
    }
}
