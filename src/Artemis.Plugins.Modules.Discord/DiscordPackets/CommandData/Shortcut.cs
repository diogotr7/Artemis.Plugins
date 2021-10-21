namespace Artemis.Plugins.Modules.Discord
{
    public record Shortcut
    (
         //0 - KEYBOARD_KEY
         //1 - MOUSE_BUTTON
         //2 - KEYBOARD_MODIFIER_KEY
         //3 - GAMEPAD_BUTTON
         int Type,
         int Code,
         string Name
    );
}