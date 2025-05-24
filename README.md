
# UwpWinForms

This is a really work in progress project that aims to create a reimplementation of .NET Windows Forms in UWP, using WebView2.

Basically the plan is to use *WebView2* as a rendering sandbox to draw the forms and the relative controls. In the backend remains plain WinForms C#, using the same `System.Windows.Forms` namespace, the `form.designer.cs` file, the `form.cs` file and its relatives events.

At the current state "something" works, only with *EnabledVisualStyles* disabled (not yet implemented) and other manual touches:

### Partially drawn controls
- Form
- Button
- GroupBox
- Label
- TextBox
- CheckBox
- RadioButton
- MessageBox

### Working properties after performing layout (more work only at drawing time)
- Text (Form, Button, Label, TextBox)
- Size (All controls)
- Location (All controls)
- Anchor (All controls)

### Implemented events
- TextChanged (TextBox)
- Resize (Form)
- ResizeBegin (Form)
- ResizeEnd (Form)
- Move (Form)
- Click (Button, TextBox, Label, CheckBox, RadioButton, GroupBox)
- MouseEnter (Button, TextBox, Label, CheckBox, RadioButton, GroupBox)
- MouseLeave (Button, TextBox, Label, CheckBox, RadioButton, GroupBox)
- MouseDown (Button, TextBox, Label, CheckBox, RadioButton, GroupBox)
- MouseClick (Button, TextBox, Label, CheckBox, RadioButton, GroupBox)
- MouseDoubleClick (Button, TextBox, Label, CheckBox, RadioButton, GroupBox)
- MouseUp (Button, TextBox, Label, CheckBox, RadioButton, GroupBox)
- MouseMove (Button, TextBox, Label, CheckBox, RadioButton, GroupBox)

An example is provided (Form1), showcasing what this project is currently able to do.

Windows management is provided by the great JavaScript library jsPanel, with some minor custom edits to better fit the project.
Dialog icons are from KDE/oxygen-icons

It works also on Xbox (tested Xbox Series S), but there are some more issues than running it on Windows:
- Cursors
- Missing fonts
- Debug must take place with Visual Studio 2019, VS2022 won't work because its remote debugger it's not installed in the Xbox system. You can still compile with VS2022.

At the current stage the code quality is really low (aside from Microsoft unchanged code), please don't look at it until further indication if you don't want to have a stroke.

## Compilation and execution problems
- Debugger can't attach: Try by deleting obj and bin folders, ticking "Compile with NET Native toolchain", recompile the  project and removing the tick. 
- Can't find the page in WebView2: Try moving the html folder in assets, recompile but don't run, and then moving the html folder out again. 

## Why this project exists?
- I wanted to have some forms on Xbox, also I'm quite sure this code could be easily ported to Android. Imagine the hell of old RAD devs so used to drag and drop controls in a designer, being able to not learn Android development and stick with Windows dirty interfaces. As happened with WinCE before Android came. A nightmare. 

## Major obstacles
- System.Drawing dependencies on libgdiplus.
- Everything must be reimplemented manually, slowing down the develpement.
- Do not get sued by Microsoft. I would love to include for example "Microsoft Sans Serif" for Xbox compatibility, but it is not allowed. The same also applies for icons and styles. Classic style should be allowed, as it is used also by Wine and others, right?
