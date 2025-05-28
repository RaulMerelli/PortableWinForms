namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class BindingCompleteEventArgs : CancelEventArgs
    {
        private Binding binding;
        private BindingCompleteState state;
        private BindingCompleteContext context;
        private string errorText;
        private Exception exception;

        public BindingCompleteEventArgs(Binding binding,
                                        BindingCompleteState state,
                                        BindingCompleteContext context,
                                        string errorText,
                                        Exception exception,
                                        bool cancel) : base(cancel)
        {
            this.binding = binding;
            this.state = state;
            this.context = context;
            this.errorText = (errorText == null) ? string.Empty : errorText;
            this.exception = exception;
        }

        public BindingCompleteEventArgs(Binding binding,
                                        BindingCompleteState state,
                                        BindingCompleteContext context,
                                        string errorText,
                                        Exception exception) : this(binding, state, context, errorText, exception, true)
        {
        }

        public BindingCompleteEventArgs(Binding binding,
                                        BindingCompleteState state,
                                        BindingCompleteContext context,
                                        string errorText) : this(binding, state, context, errorText, null, true)
        {
        }

        public BindingCompleteEventArgs(Binding binding,
                                        BindingCompleteState state,
                                        BindingCompleteContext context) : this(binding, state, context, string.Empty, null, false)
        {
        }

        public Binding Binding
        {
            get
            {
                return this.binding;
            }
        }

        public BindingCompleteState BindingCompleteState
        {
            get
            {
                return this.state;
            }
        }

        public BindingCompleteContext BindingCompleteContext
        {
            get
            {
                return this.context;
            }
        }

        public string ErrorText
        {
            get
            {
                return this.errorText;
            }
        }

        public Exception Exception
        {
            get
            {
                return this.exception;
            }
        }
    }
}