using System.Text;

namespace TextEdit;

enum Direction
{
    Up,
    Down,
    Left,
    Right
}

class Cell
{
    public int X { get; set; }
    public int Y { get; set; }
    public char Value { get; set; }
    public bool Bold { get; set; }
    public bool Inverted { get; set; }
}

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

    public void Command(EditorCommand command, char? ch = null)
    {
        switch (command)
        {
            case EditorCommand.InsertCharacter:
                if (ch.HasValue)
                    Insert(ch.Value);
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
        }
    }

    private void MoveToStartOfLine()
    {
        _cursorCol = 0;
        ScrollToCursor();
    }

    private void MoveToEndOfLine()
    {
        _cursorCol = _document.Lines.ElementAtOrDefault(_cursorRow)?.Length ?? 0;
        ScrollToCursor();
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

    public void Render(Stream stream)
    {
        var rows = Console.WindowHeight;
        var cols = Console.WindowWidth;

        if (rows * cols != _buffer.Length)
        {
            InitializeBuffer();
        }

        IEnumerable<string> lines = _document.Lines.Skip(_scrollRow);

        // Clear buffer and reset formatting
        foreach (var cell in _buffer)
        {
            cell.Value = ' ';
            cell.Bold = false;
            cell.Inverted = false;
        }

        int bufferIndex = 0;
        foreach (var line in lines)
        {
            if (bufferIndex / cols >= rows)
                break;

            for (int col = 0; col < cols; col++)
            {
                int lineIndex = col + _scrollCol; // Adjusted for horizontal scroll

                if (bufferIndex / cols == _cursorRow - _scrollRow &&
                    col == _cursorCol - _scrollCol)
                {
                    _buffer[bufferIndex].Inverted = true; // Invert cell at cursor position
                }

                if (lineIndex < line.Length)
                    _buffer[bufferIndex].Value = line[lineIndex];
                else
                    _buffer[bufferIndex].Value = ' ';

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
                _bufferString.Append(BoldOn);

            if (cell.Inverted)
                _bufferString.Append(InvertedOn);

            _bufferString.Append(cell.Value);

            if (cell.Inverted)
                _bufferString.Append(InvertedOff);

            if (cell.Bold)
                _bufferString.Append(BoldOff);
        }

        stream.Write(Encoding.UTF8.GetBytes(_bufferString.ToString()), 0, _bufferString.Length);
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

        ScrollToCursor();
    }

    private void Insert(char ch)
    {
        _document.Insert(ch, _cursorRow, _cursorCol);
        MoveCursorRight();
        ScrollToCursor();
    }

    private void MoveCursorUp()
    {
        if (_cursorRow > 0)
        {
            _cursorRow--;
            _cursorCol = Math.Min(_cursorCol, CurrentLine.Length);
        }

        ScrollToCursor();
    }

    private void MoveCursorDown()
    {
        if (_cursorRow < _document.Lines.Count() - 1)
        {
            _cursorRow++;
            _cursorCol = Math.Min(_cursorCol, CurrentLine.Length);
        }

        ScrollToCursor();
    }

    private void MoveCursorLeft()
    {
        if (_cursorCol > 0)
            _cursorCol--;
        else if (_cursorRow > 0)
        {
            _cursorRow--;
            _cursorCol = CurrentLine.Length;
        }

        ScrollToCursor();
    }

    private void MoveCursorRight()
    {
        if (_cursorCol < CurrentLine.Length)
            _cursorCol++;
        else if (_cursorRow < _document.Lines.Count - 1)
        {
            _cursorRow++;
            _cursorCol = 0;
        }

        ScrollToCursor();
    }

    private void ScrollToCursor()
    {
        var rows = Console.WindowHeight;
        var cols = Console.WindowWidth;

        if (_cursorRow < _scrollRow)
            _scrollRow = _cursorRow;
        else if (_cursorRow >= _scrollRow + rows)
            _scrollRow = _cursorRow - rows + 1;

        if (_cursorCol < _scrollCol)
            _scrollCol = _cursorCol;
        else if (_cursorCol >= _scrollCol + cols)
            _scrollCol = _cursorCol - cols + 1;
    }
}
