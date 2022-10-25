# OBS Notifier

<img src="Images/obs_notifier.png"/>

This is a simple program for displaying notifications from OBS on your desktop.

* Currently only Windows is supported
* Does not display notifications in the form of overlays (such applications are usually banned in game anti-cheats).
* This program will be useful if you have multiple monitors, or if your game/application is not running in full-screen mode. (You can also create a plugin with sound alerts.)

## Features

* Supports `obs-websocket` 5.x (embedded in OBS 28+)
* Plugin system
* Separate settings for each plugin
* Adjusting the position and offsets of notifications
* Configurable notification display time
* Ability to choose which types of notifications to display
* Ability to set custom settings
* Highly customizable default plugin
* Nvidia-like plugin out of the box

## [Download](https://github.com/DmitriySalnikov/OBSNotifier/releases/latest)

## Important

To install OBS Notifier for OBS 27.x and get working notifications about saving replays, [read this article](https://dmitriysalnikov.itch.io/obs-notifier/devlog/335353/how-to-install-obs-notifier).

## Screenshots

![Nvidia-like notifications](Images/readme/nvidia-like_notif1.png)

![Nvidia-like notifications](Images/readme/nvidia-like_notif2.gif)

![Default notifications](Images/readme/default_notif1.gif)

![Default notifications](Images/readme/default_notif2.png)

![Settings Window](Images/readme/OBSNotifier_setting.png)

## Support

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/I2I53VZ2D)

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://paypal.me/dmitriysalnikov)

[<img src="https://upload.wikimedia.org/wikipedia/commons/8/8f/QIWI_logo.svg" alt="qiwi" width=90px/>](https://qiwi.com/n/DMITRIYSALNIKOV)

## Plugin Development

To make your own plugin, you need to create a library where `IOBSNotifierPlugin` interface will be implemented and exported.
So you need to add a reference to OBSNotifier.exe or to the OBSNotifier project.

```csharp
using OBSNotifier;
using OBSNotifier.Plugins;
using System.ComponentModel.Composition;
using ...;

namespace AwesomeNotification
{
    [Export(typeof(IOBSNotifierPlugin))]
    public partial class MyAwesomeNotification : IOBSNotifierPlugin
    {
        // Interface implementation here
    }
}
```

**Tip:** For faster implementation of all the methods and properties of the plugin, you can simply place the text cursor on the interface name and press ALT + Enter, then select `Implement interface`.

![Nvidia-like notifications](Images/readme/interface.png)

To position the notification window, you can use the `OBSNotifier.Utils.GetWindowPosition()`. Or just use other functions from the `Utils`.

Also, for more information, you can view the code of the default plugin or `Nvidia-like` plugin in the `Plugins/NvidiaLikeNotification/` folder

To test the plugin, place your dll in the `OBSNotifier/Plugins` folder with installed program, or to a subdirectory, for example `OBSNotifier/Plugins/MyAwesomePlugin/Plugin.dll`.

**Tip:** For ease of development, you can add the command `copy /Y "[your dll]" "[target path]"` in post-build actions ([example](https://github.com/DmitriySalnikov/OBSNotifier/blob/463fcb63f6b07c6a80df4b9cc70f41ccd6f405c8/Plugins/NvidiaLikeNotification/NvidiaLikeNotification.csproj#L106)).

![Post-build Action](Images/readme/post-build.png)

## License

MIT license
