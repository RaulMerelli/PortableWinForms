namespace System.Windows.Forms
{
    using System.ComponentModel;
    using System.ServiceModel.Channels;

    public interface IBindableComponent : IComponent
    {
        ControlBindingsCollection DataBindings { get; }

        BindingContext BindingContext { get; set; }
    }
}