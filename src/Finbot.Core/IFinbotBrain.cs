namespace Finbot.Core;
using System.Threading;
using System.Threading.Tasks;

public interface IFinbotBrain
{
    Task RunAsync(CancellationToken cancellationToken);
}
