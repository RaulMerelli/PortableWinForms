namespace System.Windows.Forms
{
    using System.ComponentModel;

    public interface IBindableComponent : IComponent
    {
        ControlBindingsCollection DataBindings { get; }

        BindingContext BindingContext { get; set; }
    }
}