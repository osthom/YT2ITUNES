using System;
using System.IO;
using System.Text;

namespace YT2ITUNES.Services;
public class ConsoleRedirect : TextWriter
{
    private readonly Action<string> _stringAction;

    public ConsoleRedirect(Action<string> StringAction)
    {
        _stringAction = StringAction;
    }


    public override void Write(char value)
    {
        _stringAction(value.ToString());
    }
    public override void Write(string value)
    {
        _stringAction(value);
    }
    public override Encoding Encoding => Encoding.UTF8;
}