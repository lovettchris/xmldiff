using Microsoft.XmlDiffPatch;
using System.IO;
using System.Numerics;
using System.Text;

class Program
{
    string file1;
    string file2;
    string outputFile;
    bool compact;
    enum OutputFormat
    {
        Xml,
        Html
    }
    OutputFormat format;

    bool ParseCommandLine(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg[0] == '-')
            {
                switch (arg.TrimStart('-').ToLowerInvariant())
                {
                    case "?":
                    case "h":
                    case "help":
                        return false;
                    case "format":
                        if (i + 1 >= args.Length)
                        {
                            WriteError($"Missing --format parameter 'xml|html'");
                            return false;
                        }
                        i++;
                        arg = args[i];
                        if (arg == "xml")
                        {
                            format = OutputFormat.Xml;
                        }
                        else if (arg == "html")
                        {
                            format = OutputFormat.Html;
                        }
                        else
                        {
                            WriteError($"### Error: Unknown --format parameter {arg}, expecting 'xml' or 'html'");
                            return false;
                        }
                        break;
                    case "compact":
                        compact = true;
                        break;
                    default:
                        WriteError($"### Error: Unknown argument: {args[i]}");
                        return false;
                }
            }
            else if (file1 == null)
            {
                file1 = arg;
            }
            else if (file2 == null)
            {
                file2 = arg;
            }
            else if (outputFile == null)
            {
                outputFile = arg;
            }
            else
            {
                WriteError("### Error: Too many arguments");
                return false;
            }
        }
        if (file1 == null || file2 == null || outputFile == null)
        {
            WriteError("### Error: Not enough arguments");
            return false;
        }

        return true;
    }

    void WriteError(string msg) {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ForegroundColor = color;
    }

    void PrintUsage()
    {
        Console.WriteLine("Usage: xmldiff file1 file2 outputFile --format [xml|html] --compact");
    }

    static void Main(string[] args)
    {
        Program p = new Program();
        if (p.ParseCommandLine(args))
        {
            p.Run();
        }
        else
        {
            p.PrintUsage();
        }
    }

    void Run()
    {
        var view = new XmlDiffView();
        var options = XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreWhitespace;
        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }
        switch (format)
        {
            case OutputFormat.Xml:
                view.DifferencesAsFormattedText(file1, file2, outputFile, false, options);
                break;
            case OutputFormat.Html:
                view.DifferencesSideBySideAsHtml(file1, file2, outputFile, false, options, compact);
                break;
            default:
                break;
        }
    }
}