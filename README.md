PortableWinForms
================

**⚠️ This is a work-in-progress project. Not ready for production use.**

PortableWinForms aims to reimplement .NET Windows Forms on **UWP (via WebView2)** and **Android (via WebView)**.

* * * * *

### 🌐 Project Description

The core idea is to use WebView(2) as a rendering sandbox to visually recreate Windows Forms and their controls. The backend remains standard C# WinForms, using the same `System.Windows.Forms` namespace, with familiar `.Designer.cs` and `.cs` files, and event handlers.

* * * * *

### ✅ Current Status

A limited demo works: you can import a form with supported elements, apply a slightly modified entry point (take a look at `Program.cs`), and make minimal manual adjustments.

* * * * *

### 🖼 Platform Support

-   **UWP (Windows)**
  
![image](https://github.com/user-attachments/assets/eba8b2bb-e791-4192-902f-ade29ce3e4b1)

-   **Android**
  
![image](https://github.com/user-attachments/assets/0b33adf6-ea0f-4473-9d28-6982cc97c18a)

* * * * *

### 🧱 Partially Implemented Controls

-   Form

-   Button

-   GroupBox

-   Panel

-   Label

-   TextBox

-   CheckBox

-   RadioButton

-   MessageBox

* * * * *

### ⚙️ Working Properties (after layout, more works only before)

-   `Text` (Form, Button, Label, TextBox, GroupBox)

-   `Size`, `Location`, `Anchor` (all controls)

-   `BorderStyle` (Panel)

* * * * *

### 🧩 Implemented Events

-   `TextChanged` (TextBox)

-   `Resize`, `ResizeBegin`, `ResizeEnd`, `Move` (Form)

-   `Click`, `MouseEnter`, `MouseLeave`, `MouseDown`, `MouseClick`, `MouseDoubleClick`, `MouseUp`, `MouseMove`\
    *(Supported on: Button, TextBox, Label, CheckBox, RadioButton, GroupBox, Panel)*

More events might be already working.

* * * * *

### 📦 Examples

Includes basic examples (`Form1`, `Form2`) demonstrating the current capabilities.

* * * * *

### 📚 Dependencies

-   **Window management** via [jsPanel](https://github.com/Flyer53/jsPanel4) (lightly customized)

-   **Icons** from KDE Oxygen

-   **Base code** adapted from [Microsoft Reference Source](https://referencesource.microsoft.com/), stripped of GDI+ and native Windows DLL calls in favor of WebView rendering.

-   **Nuget packages** are required for each platform, and notably [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)

Other dependencies are there, but those are the most important by design.

* * * * *

### 🏗️ Building

-   **UWP**: Tested with VS2019 and VS2022 (recommended).  
    Use `UwpWinForms.sln` or `UwpWinForms.csproj`.  

-   **Android**: Tested with VS2022 and from `dotnet` command to build the APK package.  
    Use `AndroidWinForms.sln` or `AndroidWinForms.csproj`.  

**Android**\
The Android project is not MAUI, but having installed the full MAUI workload might make easier to compile the Android version.  
Requires .NET9 and platform tools version 35. You might try to change it using older NET versions or platform tools, maybe it works anyway. I was able to use platform tools 34 for example.  
Version 35 is easier as it comes as default while installing what is needed with Visual Studio Installer.  

**UWP**\
Requires a Windows SDK installed, the Windows 11 22000 is ok.  
NET version required depends on the Windows SDK choosen.  

More specific details of the necessary workloads needed for Visual Studio will be added here soon, but you can try guessing what is needed, especially if you are already a UWP or .NET Android or Xamarin developer.

* * * * *

### 🚧 Known Issues

-   **Xbox/Android**: Cursor inconsistencies

-   **Xbox/Android**: Missing fonts

-   **Android**: Cannot click behind forms. Close the app using debug from Visual Studio, adb or 

-   **Android**: Scaling at 100% is not the best option for smartphones

* * * * *

### ⚠️ Code Quality Warning

Current code is still rough (excluding original Microsoft code). 

Review it at your own risk!

Some code comments are in Italian, as it is more comfortable for me. It will be English only in the next revisions, don't worry.

* * * * *

### 🛠 Troubleshooting

-   **UWP -- Debugger won't attach**\
    Delete `obj` and `bin`, enable *Compile with .NET Native toolchain*, rebuild. Then disable the option again.

-   **UWP -- HTML not loading in WebView2**\
    Temporarily move the `html/` folder to `Assets/`, rebuild (without running), then move it back.

-   **Xbox debugging**\
    Use **Visual Studio 2019**. VS2022 doesn't work due to remote debugger incompatibility.

-   **Android -- App can't be closed normally**\
    Use `adb` to kill the process manually.

* * * * *

### 💻 Future platform support

-   **UWP -- Windows 10 Mobile, and other UWP platforms**\
    Currently not supported, as WebView2 is not available on Windows 10 Mobile and others.\
    I own W10M devices where I could test and develop the project, but I don't know if it is possible to use the older EdgeHTML control, and even if possible, it might hold back the HTML/JS/CSS chain requiring major testings to be sure the project is compatible with older and non-Chromium engines.\
     If you want to try creating an extended port, feel free to do it. If it works fine, I am more than glad with this kind of contributions.

-   **iOS, and other Apple platforms**\
    Apple products might be more strict, but I'm quite sure a port would be possibile on a `WebKit` based control.\
    Anyway I do not own any Apple product, needed to build and test a port.

-   **Linux**\
    It doesn't make sense to port this to Linux desktop, when Windows Forms are already available with more compatibility already with the Mono chain.\
    So it is not a priority or planned right now, but it would be for sure possible.

-   **Others platforms**\
    I don't know? Feel free to propose.\
    It requires anyway for sure .NET support and a quite modern HTML/JS engine, so forget about ancient operating systems, like OS/2.\
    Anyway I will not be able to support the project on a platform I do not own or that I can't run, when running on specific hardware.

* * * * *

### ❓ Why This Project?

-   I wanted to better understand Windows Forms and experiment.

-   Early code of this project comes from a private developement abandoned because there was no need anymore. It was a Server-Client porting of a legacy app. This one is not running with a web server, but shaped this project idea and it was really helpful to not start from scratch. 

-   I wanted to create basic GUI windows on Xbox.

-   Port Windows Forms projects to other platforms (currently only UWP and Android are supported). Porting the logic from UWP to Android proved feasible within a few days.

-   It may help migrate legacy Windows CE WinForms apps to Android. 

* * * * *

### 🧱 Major Challenges

-   `System.Drawing` dependency on GDI+ or `libgdiplus`. Maybe it is possible to use `canvas` and SkiaSharp to create a portable reimplementation.

-   Rewriting controls manually (with all the properties and methods) is slow and labor-intensive.

-   Something cannot be reimplemented at all as everything is running inside a sandbox (especially file interaction, overrides over windows messages, and external DLLs). Access to platform specific infos might not be always possible.

-   Licensing: Cannot include fonts like "Microsoft Sans Serif" for non Windows platforms; cautious reuse of icons/styles to avoid legal issues. (Classic styles may be safe as used in Wine for example)

* * * * *

### 👨‍💻 Want to Contribute?

I'm developing this in my spare time. The project may pause and resume unpredictably.\
**This is the perfect time to jump in if you're interested!**

Merge to main branch is ok only if not breaking any previous implemented functionality and we are able to compile without issues.

New classes and controls are needed to be created and tested first on the UWP version. If done right, Android implementation should receive effortless the changes.

* * * * *

### 🔚 Final Notes

If you're interested, feel free to clone, try, and contribute!

If you want to support the project, new Issues are welcome, and even if you don't want to code, you might help by tracking compatibility creating a new Issue (or even better, Issues templates and specific tracking for every class and control), or by just leaving a ⭐ Star, in order to let me know you are interested!

I am not currently accepting monetary donations. More money will not help me have more free time for the project.\
But if you want me to support a specific hardware/software, I don't want to ask you to donate or lend me hardware, but having remote access to systems or having access to online services that somehow enable the development in question would be very helpful.

Have a question? Just ask!
