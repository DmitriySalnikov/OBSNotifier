using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSNotifier.Plugins
{
    public enum NotificationType
    {
        ReplayStarting,
        ReplayStarted,
        ReplayStopping,
        ReplayStopped,
        ReplaySaved,

        RecordStarting,
        RecordStarted,
        RecordStopping,
        RecordStopped,
        RecordSaved,

        StreamStarting,
        StreamStarted,
        StreamStopping,
        StreamStopped,
    }
}
