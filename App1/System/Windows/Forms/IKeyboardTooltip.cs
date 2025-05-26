using System.Collections.Generic;
using System.Drawing;

namespace System.Windows.Forms
{
    internal interface IKeyboardToolTip
    {
        bool CanShowToolTipsNow();

        Rectangle GetNativeScreenRectangle();

        IList<Rectangle> GetNeighboringToolsRectangles();

        bool IsHoveredWithMouse();

        bool HasRtlModeEnabled();

        bool AllowsToolTip();

        IWin32Window GetOwnerWindow();

        //void OnHooked(ToolTip toolTip);

        //void OnUnhooked(ToolTip toolTip);

        //string GetCaptionForTool(ToolTip toolTip);

        bool ShowsOwnToolTip();

        bool IsBeingTabbedTo();

        bool AllowsChildrenToShowToolTips();
    }
}