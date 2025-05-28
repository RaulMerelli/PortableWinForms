using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

public class PopupEventArgs : CancelEventArgs
{

    private IWin32Window associatedWindow;
    private Size size;
    private Control associatedControl;
    private bool isBalloon;


    public PopupEventArgs(IWin32Window associatedWindow, Control associatedControl, bool isBalloon, Size size)
    {
        this.associatedWindow = associatedWindow;
        this.size = size;
        this.associatedControl = associatedControl;
        this.isBalloon = isBalloon;

    }

    public IWin32Window AssociatedWindow
    {
        get
        {
            return associatedWindow;
        }
    }

    public Control AssociatedControl
    {
        get
        {
            return associatedControl;
        }

    }

    public bool IsBalloon
    {
        get
        {
            return isBalloon;
        }

    }

    public Size ToolTipSize
    {
        get
        {
            return size;
        }
        set
        {
            size = value;
        }
    }
}
