namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class ApplicationContext : IDisposable
    {
        Form mainForm;
        object userData;

        public ApplicationContext() : this(null)
        {
        }

        public ApplicationContext(Form mainForm)
        {
            this.MainForm = mainForm;
        }

        ~ApplicationContext()
        {
            Dispose(false);
        }

        public Form MainForm
        {
            get
            {
                return mainForm;
            }
            set
            {
                EventHandler onClose = new EventHandler(OnMainFormDestroy);
                if (mainForm != null)
                {
                    mainForm.HandleDestroyed -= onClose;
                }

                mainForm = value;

                if (mainForm != null)
                {
                    mainForm.HandleDestroyed += onClose;
                }
            }
        }

        [
        Localizable(false),
        Bindable(true),
        DefaultValue(null),
        TypeConverter(typeof(StringConverter)),
        ]
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

        public event EventHandler ThreadExit;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (mainForm != null)
                {
                    if (!mainForm.IsDisposed)
                    {
                        mainForm.Dispose();
                    }
                    mainForm = null;
                }
            }
        }

        public void ExitThread()
        {
            ExitThreadCore();
        }

        protected virtual void ExitThreadCore()
        {
            if (ThreadExit != null)
            {
                ThreadExit(this, EventArgs.Empty);
            }
        }

        protected virtual void OnMainFormClosed(object sender, EventArgs e)
        {
            ExitThreadCore();
        }

        private void OnMainFormDestroy(object sender, EventArgs e)
        {
            Form form = (Form)sender;
            if (!form.RecreatingHandle)
            {
                form.HandleDestroyed -= new EventHandler(OnMainFormDestroy);
                OnMainFormClosed(sender, e);
            }
        }
    }
}