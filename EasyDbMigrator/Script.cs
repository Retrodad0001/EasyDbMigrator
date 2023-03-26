using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator;

public sealed record Script
{
    public string FileName { get; }
    public string Content { get; }
    public DateTime DatePartOfName { get; private set; }
    public int SequenceNumberPart { get; private set; }

    public Script(string filename, string content)
    {
        if (string.IsNullOrWhiteSpace(value: filename))
        {
            throw new ArgumentException(message: $"'{nameof(filename)}' cannot be null or whitespace.", paramName: nameof(filename));
        }

        if (string.IsNullOrWhiteSpace(value: content))
        {
            throw new ArgumentException(message: $"'{nameof(content)}' cannot be null or whitespace.", paramName: nameof(content));
        }

        FileName = filename;
        Content = content;
        SplitUpNameofScript(filename: filename);
    }
    private void SplitUpNameofScript(string filename)
    {
        int yearPart = int.Parse(s: filename[..4]);
        int monthPart = int.Parse(s: filename.Substring(startIndex: 4, length: 2));
        int dayPart = int.Parse(s: filename.Substring(startIndex: 6, length: 2));
        DatePartOfName = new DateTime(year: yearPart, month: monthPart, day: dayPart);

        SequenceNumberPart = int.Parse(s: filename.Substring(startIndex: 9, length: 3));
    }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"{FileName} - {DatePartOfName} - {SequenceNumberPart} - content: {Content}";
    }
}