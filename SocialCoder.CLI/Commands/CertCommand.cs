using Spectre.Console.Cli;

namespace SocialCoder.CLI.Commands;

public class CertCommand : Command
{
    public override int Execute(CommandContext context)
    {
        Util.CreateSSL();
    
        return 0;
    }
}