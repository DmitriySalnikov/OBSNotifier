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
        Options = 0b0000001,
        /// <summary>
        /// X,Y relative offset Sliders
        /// </summary>
        Offset = 0b0000010,
        /// <summary>
        /// Fade delay Slider
        /// </summary>
        FadeDelay = 0b0000100,
        /// <summary>
        /// Additional data TextBox
        /// </summary>
        AdditionalData = 0b0001000,
        /// <summary>
        /// Custom settings Button
        /// </summary>
        CustomSettings = 0b0010000,

        All = 0b0011111,
        AllNoCustomSettings = 0b0001111,
    }
}
