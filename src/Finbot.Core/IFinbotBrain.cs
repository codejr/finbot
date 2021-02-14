using System.Threading;
using System.Threading.Tasks;

namespace Finbot.Core
{
    public interface IFinbotBrain
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}