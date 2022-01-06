using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBSNotifier.Plugins
{
    [Flags]
    public enum DefaultPluginSettings
    {
        None = 0,
        /// <summary>
        /// ComboBox with options
        /// </summary>
        Options = 1 << 1,
        /// <summary>
        /// X,Y relative offset Sliders
        /// </summary>
        Offset = 1 << 2,
        /// <summary>
        /// Fade delay Slider
        /// </summary>
        FadeDelay = 1 << 3,
        /// <summary>
        /// Additional data TextBox
        /// </summary>
        AdditionalData = 1 << 4,
        /// <summary>
        /// Custom settings Button
        /// </summary>
        CustomSettings = 1 << 5,

        All = Options | Offset | FadeDelay | AdditionalData | CustomSettings,
        AllNoCustomSettings = Options | Offset | FadeDelay | AdditionalData,
    }

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
