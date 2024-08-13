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

        //
        // Riepilogo:
        //     Ottiene o imposta una stringa arbitraria che rappresenta un tipo di stato dell'utente.
        //
        //
        // Valori restituiti:
        //     Stringa arbitraria che rappresenta un tipo di stato dell'utente.
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

        //
        // Riepilogo:
        //     Ottiene o imposta un valore che indica se il timer è in esecuzione.
        //
        // Valori restituiti:
        //     true se il timer è attualmente abilitato; in caso contrario, false.Il valore
        //     predefinito è false.
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

        //
        // Riepilogo:
        //     Ottiene o imposta l'intervallo di tempo, in millisecondi, prima che venga generato
        //     l'evento System.Windows.Forms.Timer.Tick relativo all'ultima occorrenza dell'evento
        //     System.Windows.Forms.Timer.Tick.
        //
        // Valori restituiti:
        //     Un valore System.Int32 che specifica il numero di millisecondi prima che venga
        //     generato l'evento System.Windows.Forms.Timer.Tick relativo all'ultima occorrenza
        //     dell'evento System.Windows.Forms.Timer.Tick.Il valore non può essere minore di
        //     1.
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

        //
        // Riepilogo:
        //     Si verifica quando l'intervallo specificato del timer è trascorso e il timer
        //     viene attivato.
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

        //
        // Riepilogo:
        //     Inzializza una nuova istanza della classe System.Windows.Forms.Timer.
        public Timer()
        {
            interval = 100;
        }

        //
        // Riepilogo:
        //     Inizializza una nuova istanza della classe System.Windows.Forms.Timer insieme
        //     al contenitore specificato.
        //
        // Parametri:
        //   container:
        //     Oggetto System.ComponentModel.IContainer che rappresenta il contenitore per il
        //     timer.
        public Timer(IContainer container)
            : this()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

        }

        //
        // Riepilogo:
        //     Genera l'evento System.Windows.Forms.Timer.Tick.
        //
        // Parametri:
        //   e:
        //     Oggetto System.EventArgs che contiene i dati dell'evento.Questo valore è sempre
        //     System.EventArgs.Empty.
        protected virtual void OnTick(EventArgs e)
        {
            if (onTimer != null)
            {
                onTimer(this, e);
            }
        }

        //
        // Riepilogo:
        //     Avvia il timer.
        public void Start()
        {
            Enabled = true;
        }

        //
        // Riepilogo:
        //     Arresta il timer.
        public void Stop()
        {
            Enabled = false;
        }

        //
        // Riepilogo:
        //     Restituisce una stringa che rappresenta l'oggetto System.Windows.Forms.Timer.
        //
        //
        // Valori restituiti:
        //     Stringa che rappresenta l'oggetto System.Windows.Forms.Timer corrente.
        public override string ToString()
        {
            string text = base.ToString();
            return text + ", Interval: " + Interval.ToString(CultureInfo.CurrentCulture);
        }
    }
}
