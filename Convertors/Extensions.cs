using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Convertors
{
    public static class Extensions
    {
        public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (process.HasExited)
            {
                return Task.FromResult(0);
            }

            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(
                    () =>
                    {
                        tcs.TrySetCanceled();
                        process.Kill();
                    });
            }

            return tcs.Task;
        }
    }
}
