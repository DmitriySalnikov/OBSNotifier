# OBS Notifier

<img src="Images/obs_notifier.png"/>

[![Crowdin](https://badges.crowdin.net/obs-notifier/localized.svg)](https://crowdin.com)

This is a simple program for displaying notifications from OBS on your desktop.

* Currently only Windows is supported.
* Does not display notifications in the form of overlays (such applications are usually banned in game anti-cheats).
* This program will be useful if you have multiple monitors, or if your game/application is not running in full-screen mode. (You can also create a plugin with sound alerts.)

## Features

* Supports `obs-websocket` 5.x (embedded in OBS 28+)
* Plugin system
* Separate settings for each plugin
* Adjusting the position and offsets of notifications
* Configurable notification display time
* Highly customizable default plugin
* Nvidia-like plugin out of the box
* Ability to choose which types of notifications to display
* Ability to quickly open saved files
* Multi-language support

## [Download](https://github.com/DmitriySalnikov/OBSNotifier/releases/latest)

## Important

To install OBS Notifier for OBS 27.x and get working notifications about saving replays, [read this article](https://dmitriysalnikov.itch.io/obs-notifier/devlog/335353/how-to-install-obs-notifier).

## Troubleshooting

If you have found a bug or want to suggest a new feature, then please create a new Issue. If necessary, attach logs, you can find them in `%APPDATA%/OBSNotifier/logs` (`log.txt` and `plugin_manager_log.txt`) or use the `Open logs folder` button in the tray.

## Screenshots

![Nvidia-like notifications](Images/readme/nvidia-like_notif1.png)

![Nvidia-like notifications](Images/readme/nvidia-like_notif2.gif)

![Nvidia-like qucik actions](Images/readme/quick_actions.gif)

![Default notifications](Images/readme/default_notif1.gif)

![Default notifications](Images/readme/default_notif2.png)

![Settings Window](Images/readme/OBSNotifier_setting.png)

## Support

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/I2I53VZ2D)

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://paypal.me/dmitriysalnikov)

[<img src="https://upload.wikimedia.org/wikipedia/commons/8/8f/QIWI_logo.svg" alt="qiwi" width=90px/>](https://qiwi.com/n/DMITRIYSALNIKOV)

## Localization

This application supports several languages into which it is translated by the community (`currently localization is completely based on machine translation`). If you want to help with the translation, you can visit the application page on [Crowdin](https://crowdin.com/project/obs-notifier).

## License

MIT license
