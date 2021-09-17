using System;

namespace EasyDbMigrator
{
    public class Script
    {
        public string NameScriptsComplete { get; private set; }
        public string Content { get; private set; }
        public DateTime DatePartOfName { get; private set; }
        public int SequenceNumberPart { get; private set; }

        public string NamePart { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Script(string scriptname, string content)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            if (string.IsNullOrWhiteSpace(scriptname))
            {
                throw new ArgumentException($"'{nameof(scriptname)}' cannot be null or whitespace.", nameof(scriptname));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException($"'{nameof(content)}' cannot be null or whitespace.", nameof(content));
            }

            NameScriptsComplete = scriptname;
            Content = content;
            SplitUpNameofScript(scriptname);
        }

        private void SplitUpNameofScript(string completeNameOfScript)
        {
            string[] namePartsWithAssemblyPrefix = completeNameOfScript.Split('.', StringSplitOptions.None);
            string[] nameParts = namePartsWithAssemblyPrefix[^2].Split('_', StringSplitOptions.None); //not the last one because this is the .sql file extension

            string datePart = nameParts[0];
            int yearPart = int.Parse(datePart.Substring(0, 4));
            int monthPart = int.Parse(datePart.Substring(3, 2)); 
            int dayPart = int.Parse(datePart.Substring(6, 2));
            DatePartOfName = new DateTime(year: yearPart, month: monthPart, day:dayPart);

            SequenceNumberPart = int.Parse(nameParts[1]);
            NamePart = nameParts[2];
        }
        public override string ToString()
        {
            return $"{NamePart} - {DatePartOfName} - {SequenceNumberPart} - content: {Content}";
        }
    }
}