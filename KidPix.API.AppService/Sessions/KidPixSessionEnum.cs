using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.AppService.Sessions
{
    /// <summary>
    /// The current overall gameplay state for this session
    /// </summary>
    public enum KidPixGameplayStates
    {
        /// <summary>
        /// Showing a cool splash screen before the game begins
        /// </summary>
        SplashScreen,
        /// <summary>
        /// Selecting a new user to log in with
        /// </summary>
        UserSelect,
        /// <summary>
        /// Normal Gameplay
        /// </summary>
        Easel
    }
    /// <summary>
    /// <see cref="Enum"/> States for UI components
    /// </summary>
    public class KidPixUIEnum {
        
        /// <summary>
        /// Values determining how to display the ToolSubpage (beneath the Easel Canvas)
        /// </summary>
        public enum EaselToolSubpageUIStates
        {
            Hidden,
            //**UI TRAY TYPE 1
            Pencils,
            Paints,
            Fills,
            Erasers,
            Mixers,
            ClippingSelections,
            //**UI TRAY TYPE 2
        }
    }
}
