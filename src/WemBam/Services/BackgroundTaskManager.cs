using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WemBam.Contracts;
using WemBam.Models;

namespace WemBam.Services
{
    public class BackgroundTaskManager
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private Stopwatch? _stopwatch;

        public static BackgroundTaskManager Instance { get; } = new();

        public BackgroundTaskState State { get; private set; } =
            BackgroundTaskState.Idle;

        public bool IsRunning =>
            State == BackgroundTaskState.Running ||
            State == BackgroundTaskState.Cancelling;

        public TimeSpan Elapsed =>
            _stopwatch?.Elapsed ?? TimeSpan.Zero;

        public event EventHandler? TaskStarted;

        public event EventHandler<BackgroundTaskProgress>? ProgressChanged;

        public event EventHandler<BackgroundOperationResult>? TaskCompleted;

        public event EventHandler? TaskCancelled;

        private BackgroundTaskManager()
        {
        }

        public async Task<bool> StartAsync(IBackgroundOperation operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            if (IsRunning)
            {
                return false;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _stopwatch = Stopwatch.StartNew();

            State = BackgroundTaskState.Running;

            TaskStarted?.Invoke(this, EventArgs.Empty);

            Progress<BackgroundTaskProgress> progress = new(
                value => ProgressChanged?.Invoke(this, value));

            try
            {
                BackgroundOperationResult result =
                    await operation.ExecuteAsync(
                        progress,
                        _cancellationTokenSource.Token);

                _stopwatch.Stop();

                State = result.State;

                if (result.State == BackgroundTaskState.Cancelled)
                {
                    TaskCancelled?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    TaskCompleted?.Invoke(this, result);
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                _stopwatch.Stop();

                State = BackgroundTaskState.Cancelled;

                TaskCancelled?.Invoke(this, EventArgs.Empty);

                return false;
            }
            catch (Exception ex)
            {
                _stopwatch.Stop();

                BackgroundOperationResult result =
                    BackgroundOperationResult.Failed(ex.Message);

                State = BackgroundTaskState.Failed;

                TaskCompleted?.Invoke(this, result);

                return false;
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                _stopwatch = null;

                if (State != BackgroundTaskState.Running &&
                    State != BackgroundTaskState.Cancelling)
                {
                    State = BackgroundTaskState.Idle;
                }
            }
        }

        public bool Cancel()
        {
            if (!IsRunning || _cancellationTokenSource is null)
            {
                return false;
            }

            State = BackgroundTaskState.Cancelling;

            _cancellationTokenSource.Cancel();

            return true;
        }
    }
}