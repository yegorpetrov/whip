using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Whip.Runtime
{
    /*
        extern Timer.onTimer();
        extern Timer.setDelay(int millisec);
        extern Int Timer.getDelay();
        extern Timer.start();
        extern Timer.stop();
        extern Timer.isRunning();
    */
    sealed class MakiTimer : IDisposable
    {
        Timer timer = new Timer();

        public MakiTimer()
        {
            timer.Elapsed += (a, b) => OnTimer?.Invoke();
        }

        public event Action OnTimer;

        public void SetDelay(int ms)
        {
            timer.Interval = ms;
        }

        public int GetDelay()
        {
            return (int)Math.Round(timer.Interval);
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public bool IsRunning()
        {
            return timer.Enabled;
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
