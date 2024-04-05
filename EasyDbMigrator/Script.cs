using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EasyDbMigrator;

/// <summary>
/// Represents a script file with a name and content.
/// </summary>
public sealed record Script
{
    /// <summary>
    /// The name of the script file.
    /// </summary>
    public string FileName { get; }
    /// <summary>
    /// The content of the script file.
    /// </summary>
    public string Content { get; }
    /// <summary>
    /// The date part of the name of the script file.
    /// </summary>
    public DateTime DatePartOfName { get; private set; }
    /// <summary>
    /// The sequence number part of the name of the script file.
    /// </summary>
    public int SequenceNumberPart { get; private set; }

    /// <summary>
    /// Constructor for a script file.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="content"></param>
    /// <exception cref="ArgumentException"></exception>
    public Script(string filename, string content)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentException(
                new StringBuilder().Append('\'')
                    .Append(nameof(filename))
                    .Append("' cannot be null or whitespace.")
                    .ToString(), nameof(filename));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                new StringBuilder().Append('\'')
                    .Append(nameof(content))
                    .Append("' cannot be null or whitespace.")
                    .ToString(), nameof(content));
        }

        FileName = filename;
        Content = content;
        SplitUpNameofScript(filename);
    }
    private void SplitUpNameofScript(string filename)
    {
        // ReSharper disable once HeapView.ObjectAllocation
        int yearPart = int.Parse(filename[..4]);
        int monthPart = int.Parse(filename.Substring(4, 2));
        int dayPart = int.Parse(filename.Substring(6, 2));
        DatePartOfName = new DateTime(yearPart, monthPart, dayPart);

        SequenceNumberPart = int.Parse(filename.Substring(9, 3));
    }

    [ExcludeFromCodeCoverage]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        return "{FileName} - {DatePartOfName} - {SequenceNumberPart} - content: {Content}";
    }
}