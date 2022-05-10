using Mapps.Gamepads.Components;

namespace Mapps.Gamepads.Styles.Xbox.GenericXInput
{
    public class BasicXInputGamepad : PollingGamepad, IHasButtons<XboxButton>, IHasDualJoysticks, IHasDualTriggers
    {
        private readonly static Dictionary<XboxButton, ushort> ButtonMap = new()
        {
            { XboxButton.DpadUp, 1 },
            { XboxButton.DpadDown, 2 },
            { XboxButton.DpadLeft, 4 },
            { XboxButton.DpadRight, 8 },
            { XboxButton.Start, 16 },
            { XboxButton.Back, 32 },
            { XboxButton.LeftStick, 64 },
            { XboxButton.RightStick, 128 },
            { XboxButton.LeftShoulder, 256 },
            { XboxButton.RightShoulder, 512 },
            { XboxButton.A, 4096 },
            { XboxButton.B, 8192 },
            { XboxButton.X, 16384 },
            { XboxButton.Y, 32768 },
        };

        private readonly uint _slot;

        private uint _lastPacketNumber;

        public sealed override event EventHandler? OnConnect;

        public sealed override event EventHandler? OnDisconnect;

        public sealed override event EventHandler? OnStateChanged;

        public BasicXInputGamepad(int slot)
        {
            if (slot < 0 || slot > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(slot), "Slot must be between 0 and 3 inclusive.");
            }

            _slot = (uint)slot;
        }

        public sealed override bool IsConnected { get; protected set; }

        public Buttons<XboxButton> Buttons { get; } = new Buttons<XboxButton>();

        public Joystick LeftJoystick { get; } = new Joystick();

        public Joystick RightJoystick { get; } = new Joystick();

        public Trigger LeftTrigger { get; } = new Trigger();

        public Trigger RightTrigger { get; } = new Trigger();

        protected sealed override TimeSpan PollingInterval => TimeSpan.Zero;

        protected override TimeSpan SendInterval => TimeSpan.MaxValue;

        public sealed override void StartTracking()
        {
            _lastPacketNumber = uint.MaxValue;

            base.StartTracking();
        }

        protected sealed override void UpdateState()
        {
            var state = new XInput.XInputState();
            if (XInput.XInputGetState(_slot, ref state) == 0)
            {
                if (!IsConnected)
                {
                    IsConnected = true;
                    OnConnect?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                if (IsConnected)
                {
                    IsConnected = false;
                    OnDisconnect?.Invoke(this, EventArgs.Empty);
                }
            }

            if (state.dwPacketNumber != _lastPacketNumber)
            {
                UpdateFromXInputState(state.Gamepad);

                OnStateChanged?.Invoke(this, EventArgs.Empty);
                _lastPacketNumber++;
            }
        }

        protected override void SendState()
        {
            throw new NotImplementedException();
        }

        protected virtual void UpdateFromXInputState(XInput.XInputGamepad gamepad)
        {
            Buttons.HeldButtons = ExtractButtons(gamepad.wButtons);

            LeftJoystick.X = ConvertJoystickValue(gamepad.sThumbLX);
            LeftJoystick.Y = ConvertJoystickValue(gamepad.sThumbLY);
            RightJoystick.X = ConvertJoystickValue(gamepad.sThumbRX);
            RightJoystick.Y = ConvertJoystickValue(gamepad.sThumbRY);

            LeftTrigger.Pressure = gamepad.bLeftTrigger / 255f;
            RightTrigger.Pressure = gamepad.bRightTrigger / 255f;
        }

        private static float ConvertJoystickValue(short value)
        {
            return (float)value / short.MaxValue;
        }

        private static IEnumerable<XboxButton> ExtractButtons(ushort buttons)
        {
            foreach (var (button, flag) in ButtonMap)
            {
                if ((buttons & flag) != 0)
                {
                    yield return button;
                }
            }
        }
    }
}
