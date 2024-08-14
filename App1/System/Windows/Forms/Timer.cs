using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
    public class Timer
    {
        private int interval;

        private bool enabled;

        internal EventHandler onTimer;

        private GCHandle timerRoot;

        private object userData;

        private object syncObj = new object();

        [Localizable(false)]
        [Bindable(true)]
        [DefaultValue(null)]
        [TypeConverter(typeof(StringConverter))]
        public object Tag
        {
            get
            {
                return userData;
            }
            set
            {
                userData = value;
            }
        }

        [DefaultValue(false)]
        public virtual bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                lock (syncObj)
                {
                    if (enabled == value)
                    {
                        return;
                    }

                    enabled = value;
                    if (timerRoot.IsAllocated)
                    {
                        timerRoot.Free();
                    }
                }
            }
        }

        [DefaultValue(100)]
        public int Interval
        {
            get
            {
                return interval;
            }
            set
            {
                lock (syncObj)
                {
                    if (value < 1)
                    {
                        throw new ArgumentOutOfRangeException("Interval", "TimerInvalidInterval");
                    }

                    if (interval != value)
                    {
                        interval = value;
                    }
                }
            }
        }

        public event EventHandler Tick
        {
            add
            {
                onTimer = (EventHandler)Delegate.Combine(onTimer, value);
            }
            remove
            {
                onTimer = (EventHandler)Delegate.Remove(onTimer, value);
            }
        }

        public Timer()
        {
            interval = 100;
        }

        public Timer(IContainer container)
            : this()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

        }

        protected virtual void OnTick(EventArgs e)
        {
            if (onTimer != null)
            {
                onTimer(this, e);
            }
        }

        public void Start()
        {
            Enabled = true;
        }

        public void Stop()
        {
            Enabled = false;
        }

        public override string ToString()
        {
            string text = base.ToString();
            return text + ", Interval: " + Interval.ToString(CultureInfo.CurrentCulture);
        }
    }
}