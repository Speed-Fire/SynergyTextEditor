using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace SynergyTextEditor.Classes.Workers
{
    public interface IWorkerArgs { }

    public abstract class BaseWorker : IDisposable
    {
        private readonly string _workerName;
        private readonly bool _isBackground;

        private volatile bool _isSuspended;
        protected bool IsSuspended => _isSuspended;

        private volatile bool _isActuallySuspended;
        protected bool IsActuallySuspended => _isActuallySuspended;

        protected CancellationTokenSource CancellationTokenSource { get; set; }
        private Thread _workerThread;
        private bool disposedValue;
        private readonly Mutex _workerMutex;

        protected ThreadState WorkerState => _workerThread is null ?
            ThreadState.Unstarted : _workerThread.ThreadState;

        public BaseWorker(string workerName, bool isBackground)
        {
            CancellationTokenSource = new();

            _isSuspended = false;
            _isActuallySuspended = false;

            _workerMutex = new();

            _workerName = workerName;
            _isBackground = isBackground;
        }

        protected abstract void DoWork(IWorkerArgs args);

        private void Running(CancellationToken token, IWorkerArgs args)
        {
            while (!token.IsCancellationRequested)
            {
                if (_isSuspended)
                {
                    _isActuallySuspended = true;
                    continue;
                }

                _isActuallySuspended = false;

                DoWork(args);
            }
        }

        public bool Run(IWorkerArgs initializationArgs, IWorkerArgs iterationArgs)
        {
            _workerMutex.WaitOne();

            if (_workerThread != null && _workerThread.ThreadState != ThreadState.Stopped)
                return false;

            if (_workerThread is null)
            {
                _workerThread = new Thread(() => Running(CancellationTokenSource.Token, iterationArgs))
                {
                    Name = _workerName,
                    IsBackground = _isBackground
                };
            }

            ApplyInitializationArgs(initializationArgs);

            bool res = false;

            try 
            {
                _workerThread.Start();
                _isSuspended = false;
                _isActuallySuspended = false;

                res = true;
            }
            catch
            {
                res = false;
            }

            _workerMutex.ReleaseMutex();

            return res;
        }

        public bool Run(IWorkerArgs initializationArgs)
        {
            return Run(initializationArgs, null);
        }

        public void Abort()
        {
            if (_workerThread is null)
                return;

            _workerMutex.WaitOne();

            if (_workerThread.ThreadState == ThreadState.Running)
            {
                CancellationTokenSource.Cancel();
            }

            _workerMutex.ReleaseMutex();
        }

        public void Resume()
        {
            if (!IsSuspended) return;

            _workerMutex.WaitOne();

            _isSuspended = false;

            _workerMutex.ReleaseMutex();
        }

        public void Suspend()
        {
            if (IsSuspended) return;

            _workerMutex.WaitOne();

            _isSuspended = true;

            _workerMutex.ReleaseMutex();
        }

        public void SuspendWait()
        {
            if (IsSuspended) return;

            _workerMutex.WaitOne();

            _isSuspended = true;

            while (!_isActuallySuspended) { }

            _workerMutex.ReleaseMutex();
        }

        protected virtual void ApplyInitializationArgs(IWorkerArgs initializationArgs) { }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                    CancellationTokenSource.Dispose();
                    _workerMutex.Dispose();
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~BaseWorker()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
