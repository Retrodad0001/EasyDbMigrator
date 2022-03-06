using System;
using System.Diagnostics.CodeAnalysis;

namespace EasyDbMigrator
{ 
    public sealed record Script
    {
        public string FileName { get; }
        public string Content { get; }
        public DateTime DatePartOfName { get; private set; }
        public int SequenceNumberPart { get; private set; }

        public Script(string filename, string content)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException($"'{nameof(filename)}' cannot be null or whitespace.", nameof(filename));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException($"'{nameof(content)}' cannot be null or whitespace.", nameof(content));
            }

            FileName = filename;
            Content = content;
            SplitUpNameofScript(filename);
        }
        private void SplitUpNameofScript(string filename)
        {
            int yearPart = int.Parse(filename.Substring(0, 4));
            int monthPart = int.Parse(filename.Substring(4, 2));
            int dayPart = int.Parse(filename.Substring(6, 2));
            DatePartOfName = new DateTime(year: yearPart, month: monthPart, day: dayPart);

            SequenceNumberPart = int.Parse(filename.Substring(9, 3));
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return $"{FileName} - {DatePartOfName} - {SequenceNumberPart} - content: {Content}";
        }
    }
}