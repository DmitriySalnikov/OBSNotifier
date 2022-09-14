# Custom Libraries

Modified versions of [obs-websocket](https://github.com/obsproject/obs-websocket) and [obs-websocket-dotnet](https://github.com/BarRaider/obs-websocket-dotnet) libraries are located here.

Their main difference from the original versions 4.9.1 is that they added support for the "ReplaySaved" event.

`obs-websocket-dotnet.dll` is used by the OBS Notifier application itself.

`obs-websocket.dll` or `obs-websocket-compat.dll` is optional. But if you want to see notifications about the successful saving of replays, I recommend replacing the original plugin file with a modified one.

* Copy the file `obs-websocket.dll` (for OBS 27) or `obs-websocket-compat.dll` (for OBS 28+) to the folder `.../obs-studio/obs-plugins/64bit/` where you have already installed the `obs-websocket` plugin or its `compat` version.
