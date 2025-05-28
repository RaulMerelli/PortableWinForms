
# PortableWinForms

This is a really work in progress project that aims to create a reimplementation of .NET Windows Forms in UWP (using WebView2) and Android (using WebView)

*NOT READY FOR PRODUCTION*

Basically the plan is to use *WebView(2)* as a rendering sandbox to draw the forms and the relative controls. In the backend remains plain WinForms C#, using the same `System.Windows.Forms` namespace, the `form.designer.cs` file, the `form.cs` file and its relatives events.

At the current state "something" works, by importing a form designed with the only supported elements, an adjusted entrypoint and minimal manual touches.


## UWP (Windows)
![image](https://github.com/user-attachments/assets/eba8b2bb-e791-4192-902f-ade29ce3e4b1)


## Android
![image](https://github.com/user-attachments/assets/0b33adf6-ea0f-4473-9d28-6982cc97c18a)



### Partially drawn controls
- Form
- Button
- GroupBox
- Panel
- Label
- TextBox
- CheckBox
- RadioButton
- MessageBox

### Working properties after performing layout (more work only at drawing time)
- Text (Form, Button, Label, TextBox, GroupBox)
- Size (All controls)
- Location (All controls)
- Anchor (All controls)
- BorderStyle (Panel)

### Implemented events
- TextChanged (TextBox)
- Resize (Form)
- ResizeBegin (Form)
- ResizeEnd (Form)
- Move (Form)
- Click (Button, TextBox, Label, CheckBox, RadioButton, GroupBox, Panel)
- MouseEnter (Button, TextBox, Label, CheckBox, RadioButton, GroupBox, Panel)
- MouseLeave (Button, TextBox, Label, CheckBox, RadioButton, GroupBox, Panel)
- MouseDown (Button, TextBox, Label, CheckBox, RadioButton, GroupBox, Panel)
- MouseClick (Button, TextBox, Label, CheckBox, RadioButton, GroupBox, Panel)
- MouseDoubleClick (Button, TextBox, Label, CheckBox, RadioButton, GroupBox, Panel)
- MouseUp (Button, TextBox, Label, CheckBox, RadioButton, GroupBox, Panel)
- MouseMove (Button, TextBox, Label, CheckBox, RadioButton, GroupBox, Panel)

An example is provided (Form1 + Form2), showcasing what this project is currently able to do.

Windows management is provided by the great JavaScript library jsPanel, with some minor custom edits to better fit the project.
Dialog icons are from KDE/oxygen-icons.
Most of the source code comes from referencesource.microsoft.com, edited to avoid calls to GDI+ and all other native Windows DLLs and redirecting instead to the browser instance.

It works best on Windows, on Xbox and Android there are some minor issues:
- Xbox/Android: Cursors
- Xbox/Android: Missing fonts
- Android: Cannot click what is behind the forms

At the current stage the code quality is really low (aside from Microsoft unchanged code), please don't look at it until further indication if you don't want to have a stroke.

## Compilation and execution problems
- UWP: Debugger can't attach: Try by deleting obj and bin folders, ticking "Compile with NET Native toolchain", recompile the  project and removing the tick. 
- UWP: Can't find the page in WebView2: Try moving the html folder in assets, recompile but don't run, and then moving the html folder out again. 
- UWP: on Xbox (tested Xbox Series S) Debug must take place with Visual Studio 2019, VS2022 won't work because its remote debugger it's not installed in the Xbox system. You can still compile with VS2022.
- Android: close the app might be difficult. Be ready to kill it from adb.

## Why this project exists?
- I wanted to have some windows on Xbox, also I was quite sure this code could be easily ported to Android. In fact I was able to adapt the code in a couple of days.
- Porting old Win-Forms made for WinCE to Android might be easier now

## Major obstacles
- System.Drawing dependencies on libgdiplus.
- Everything must be reimplemented manually, slowing down the develpement.
- Do not get sued by Microsoft. I would love to include for example "Microsoft Sans Serif" for Xbox compatibility, but it is not allowed. The same also applies for icons and styles. Classic style I think it is allowed, as it is used also by Wine and others, right?
- My spare time (early stage is the perfect time to join the project if you want!). This project may pause and then resume after months of inactivity without other maintainers than me.
