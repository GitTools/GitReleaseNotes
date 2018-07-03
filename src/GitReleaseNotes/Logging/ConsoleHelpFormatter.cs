using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Args.Help;
using Args.Help.Formatters;

public class ConsoleHelpFormatter : IHelpFormatter
{
    protected int BufferWidth { get; set; }
    protected TextWriter Output { get; set; }
    protected int CommandSamplePadding { get; set; }
    protected int ArgumentDescriptionPadding { get; set; }

    public ConsoleHelpFormatter()
        : this(Console.BufferWidth, 1, 5)
    {

    }

    public ConsoleHelpFormatter(int bufferWidth, int commandSamplePadding, int argumentDescriptionPadding)
    {
        BufferWidth = bufferWidth;
        CommandSamplePadding = commandSamplePadding;
        ArgumentDescriptionPadding = argumentDescriptionPadding;

    }

    public virtual void WriteHelp(ModelHelp modelHelp, TextWriter writer)
    {
        Output = writer;

        if (string.IsNullOrEmpty(modelHelp.HelpText) == false)
        {
            WriteJustifiedItem(string.Empty, modelHelp.HelpText, 0);
        }

        WriteUsage(modelHelp, writer);

        Output.WriteLine();

        WriteArgumentDescriptions(modelHelp);
    }

    private void WriteArgumentDescriptions(ModelHelp modelHelp)
    {
        var items = modelHelp.Members
            .Where(m => string.IsNullOrEmpty(m.HelpText) == false)
            .OrderByDescending(m => m.OrdinalIndex.HasValue)
            .ThenBy(m => m.OrdinalIndex)
            .ToDictionary(ks => ks.OrdinalIndex.HasValue ? ks.Name : GetFullSwitchString(modelHelp.SwitchDelimiter, ks.Switches), es => es.HelpText);

        WriteJustifiedOutput(items, ArgumentDescriptionPadding);
    }

    protected virtual string GetFullSwitchString(string switchDelimiter, IEnumerable<string> switches)
    {
        var values = string.Join("|", switches.Select(s => switchDelimiter + s).ToArray());

        return "[" + values + "]";
    }

    protected virtual void WriteJustifiedOutput(IDictionary<string, string> lines, int padding)
    {
        if (lines.Any())
        {
            var totalPadding = lines.Max(l => l.Key.Length) + padding;

            foreach (var line in lines)
            {
                WriteJustifiedItem(line.Key, line.Value, totalPadding);
            }

            Output.WriteLine();
        }
    }

    protected virtual void WriteJustifiedItem(string leftColumnText, string rightColumnText, int totalPadding)
    {
        rightColumnText = rightColumnText ?? string.Empty;

        Output.Write(leftColumnText);
        Output.Write(new string(' ', totalPadding - leftColumnText.Length));

        var maxRightColumnTextLength = BufferWidth - totalPadding - 1;

        var newLinePadding = new string(' ', totalPadding);

        var firstPass = true;

        while (string.IsNullOrEmpty(rightColumnText.Trim()) == false)
        {
            string part;
            if (rightColumnText.Length < maxRightColumnTextLength)
                part = rightColumnText;
            else
            {
                part = rightColumnText.Substring(0, maxRightColumnTextLength);

                int lastSpaceIndex = -1;

                for (var i = part.Length - 1; i >= 0; i--)
                {
                    if (char.IsWhiteSpace(part[i]))
                    {
                        lastSpaceIndex = i;
                        break;
                    }
                }

                var partLength = lastSpaceIndex > 0 ? lastSpaceIndex + 1 : maxRightColumnTextLength;

                partLength = Math.Min(partLength, part.Length);

                part = part.Substring(0, partLength);
            }

            //padding is already done if this is the first pass
            if (firstPass == false) Output.Write(newLinePadding);

            Output.WriteLine(part);

            rightColumnText = rightColumnText.Substring(part.Length);

            firstPass = false;
        }

        //while loop was skipped, need new line
        if (firstPass == true)
            Output.WriteLine();
    }

    protected virtual void WriteUsage(ModelHelp modelHelp, TextWriter writer)
    {
        var values = modelHelp.Members.Where(m => m.OrdinalIndex.HasValue)
            .OrderBy(m => m.OrdinalIndex.Value)
            .Select(m => m.Name)
            .Concat(modelHelp.Members
                .Where(m => m.OrdinalIndex.HasValue == false)
                .OrderBy(m => m.Name)
                .Select(m => string.Format("[{0}]", string.Join("|", m.Switches.Select(s => modelHelp.SwitchDelimiter + s).ToArray()))));


        //TODO: Figure out actual command name?
        var dictionary = new Dictionary<string, string>
        {
            {"<command>", string.Join(" ", values.ToArray())}
        };

        WriteJustifiedOutput(dictionary, CommandSamplePadding);
    }
}