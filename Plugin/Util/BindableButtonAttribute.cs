using Nautilus.Handlers;
using System;

namespace Subnautica_Echelon.Util
{
    /// <summary>
    /// Specifies that a field represents a bindable button input in a config structure, providing metadata for input binding systems. <br />
    /// The field must be of type <see cref="GameInput.Button"/> to store and link the button binding information. <br />
    /// <b>To prevent serialization issues, ensure that field is also attributed with [JsonProperty].</b>
    /// </summary>
    /// <remarks>Add this attribute to automatically create a config entry bindable via the options menu</remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class BindableButtonAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the BindableButtonAttribute class with the specified button name.
        /// </summary>
        /// <param name="name">The name of the button to associate with this attribute. Cannot be null or empty.</param>
        public BindableButtonAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Gets the name associated with the current instance.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets or sets the translation key for the button label. If null, the Name property will be used as the (english) label.
        /// </summary>
        public string? LabelLocalizationKey { get; set; }
        /// <summary>
        /// Gets or sets the localization key used to retrieve the tooltip text for this element. If null, no tooltip will be shown.
        /// </summary>
        public string? TooltipLocalizationKey { get; set; }
        /// <summary>
        /// Gets or sets the default keyboard binding path.
        /// <see cref="GameInputHandler.Paths.Keyboard"/> and <see cref="GameInputHandler.Paths.Mouse"/> can be used to retrieve default paths.
        /// </summary>
        public string? KeyboardDefault { get; set; }
        /// <summary>
        /// Gets or sets the default controller binding path
        /// <see cref="GameInputHandler.Paths.Gamepad"/> can be used to retrieve default paths.
        /// </summary>
        public string? GamepadDefault { get; set; }

    }
}
