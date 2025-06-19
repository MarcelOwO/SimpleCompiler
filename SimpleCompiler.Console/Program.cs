using Antlr4.Runtime;
using SimpleCompiler.Shared;

string inFile;
string outPath;
if (args.Length > 2)
{
    inFile = args[0];
    outPath = args[1];
}
else
{
    inFile = "TestProgramm";
    outPath = Directory.GetCurrentDirectory();
}


if (args.Length > 2)
{
    Console.WriteLine("Too many arguments");
    return;
}

if (!File.Exists(inFile))
{
    Console.WriteLine("Input file not found");
    return;
}

if (!Directory.Exists(outPath))
{
    Console.WriteLine("Output directory does not exist");
    outPath = Directory.GetCurrentDirectory();
}

Console.WriteLine("Starting compilation...");
Compiler compiler = new();
await compiler.Compile(inFile, outPath);


Console.WriteLine("Compiler finished");