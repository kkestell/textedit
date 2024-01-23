namespace TextEdit;

public class Document
{
    private readonly List<string> lines;

    public Document()
    {
        lines = [""];
    }

    public IReadOnlyList<string> Lines => lines;

    public Document(Stream stream)
    {
        using var reader = new StreamReader(stream);
        
        lines = [];
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line is not null)
                lines.Add(line);
        }
    }

    public Document(string content)
    {
        lines = [.. content.Split('\n')];
    }

    public void Insert(char ch, int line, int col)
    {
        if (line < 0 || line >= lines.Count)
            return;

        if (ch == '\n')
        {
            var tail = lines[line][col..];
            lines[line] = lines[line][..col];
            lines.Insert(line + 1, tail);
        }
        else
        {
            var pre = lines[line][..col];
            var post = lines[line][col..];
            lines[line] = pre + ch + post;
        }
    }

    public void DeleteForward(int line, int col)
    {
        if (line < 0 || line >= lines.Count)
            return;

        if (col >= lines[line].Length)
        {
            if (line + 1 >= lines.Count)
                return;

            lines[line] += lines[line + 1];
            lines.RemoveAt(line + 1);
        }
        else
        {
            var pre = lines[line][..col];
            var post = lines[line][(col + 1)..];
            lines[line] = pre + post;
        }
    }

    public void DeleteBackward(int line, int col)
    {
        if (line < 0 || line >= lines.Count)
            return;

        if (col == 0)
        {
            if (line == 0)
                return;

            lines[line - 1] += lines[line];
            lines.RemoveAt(line);
        }
        else
        {
            var pre = lines[line][..(col - 1)];
            var post = lines[line][col..];
            lines[line] = pre + post;
        }
    }

    public void DeleteLine(int line)
    {
        if (line < 0 || line >= lines.Count)
            return;

        lines.RemoveAt(line);
    }

    public void Indent(int line)
    {
        lines[line] = "    " + lines[line];
    }

    public bool Unindent(int line)
    {
        if (lines[line].StartsWith("    "))
        {
            lines[line] = lines[line][4..];
            return true;
        }

        return false;
    }

    public string GetText()
    {
        return string.Join("\n", lines);
    }
}
