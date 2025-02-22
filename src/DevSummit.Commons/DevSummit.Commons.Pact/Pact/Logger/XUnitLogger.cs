namespace DevSummit.Commons.Pact.Logger;

using PactNet.Infrastructure.Outputters;
using Xunit.Abstractions;
public class XUnitLogger : IOutput
{
    private readonly ITestOutputHelper _output;
    public XUnitLogger(ITestOutputHelper output)
    {
        _output = output;
    }
    public void WriteLine(string line)
    {
        _output.WriteLine(line);
    }
}
