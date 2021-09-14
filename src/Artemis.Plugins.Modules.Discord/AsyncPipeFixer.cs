using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Plugins.Modules.Discord
{
    public static class AsyncPipeFixer
    {
        public static Task<int> ReadAsyncCancellable(this PipeStream pipe, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<int>(cancellationToken);
            var registration = cancellationToken.Register(() => CancelPipeIo(pipe));
            var async = pipe.BeginRead(buffer, offset, count, null, null);
            return Task.Run(() => {
                try { return pipe.EndRead(async); }
                catch { return 0; }
                finally { registration.Dispose(); }
            }, cancellationToken);
        }

        private static void CancelPipeIo(PipeStream pipe)
        {
            // Note: no PipeStream.IsDisposed, we'll have to swallow
            try
            {
                CancelIoEx(pipe.SafePipeHandle);
            }
            catch (ObjectDisposedException) { }
        }
        [DllImport("kernel32.dll")]
        private static extern bool CancelIoEx(Microsoft.Win32.SafeHandles.SafePipeHandle handle, IntPtr _ = default);
    }
}