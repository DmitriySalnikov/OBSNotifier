# Custom Libraries

Modified versions of [obs-websocket](https://github.com/obsproject/obs-websocket) and [obs-websocket-dotnet](https://github.com/BarRaider/obs-websocket-dotnet) libraries are located here.

Their main difference from the original versions 4.9.1 is that they added support for the "ReplaySaved" event.

`obs-websocket-dotnet.dll` is used by the obs-notifier application itself.

`obs-websocket.dll` is optional. But if you want to see notifications about the successful saving of replays, I recommend replacing the original plugin file with a modified one.

* Just copy the file `obs-websocket.dll` to the folder `.../obs-studio/obs-plugins/64bit/` where have you already installed `obs-websocket` plugin.
