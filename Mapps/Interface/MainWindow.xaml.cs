using Mapps.Gamepads.DualShock4;
using Mapps.Mappers;
using Mapps.OutputWrappers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DualShock4? _gamepad = null;

        private ToXbox360<DS4Button>? _outputWrapper = null;

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer serialNumbersFetchTimer = new DispatcherTimer();
            serialNumbersFetchTimer.Tick += new EventHandler(SerialNumbersFetchTimer_Tick);
            serialNumbersFetchTimer.Interval = TimeSpan.FromSeconds(0.5);
            serialNumbersFetchTimer.Start();

            UpdateControllerInformation();
        }

        private void SerialNumbersFetchTimer_Tick(object? sender, EventArgs e)
        {
            var serialNumbers = DualShock4.GetSerialNumbers();
            foreach (var serialNumber in serialNumbers)
            {
                if (!comboSerialNumbers.Items.Contains(serialNumber))
                {
                    comboSerialNumbers.Items.Add(serialNumber);
                    if (comboSerialNumbers.Items.Count == 2)
                    {
                        comboSerialNumbers.SelectedIndex = 1;
                    }
                }
            }
            UpdateControllerInformation();
        }

        private void ComboSerialNumbers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _gamepad?.Dispose();
            _gamepad = null;

            _outputWrapper?.Dispose();
            _outputWrapper = null;

            if (comboSerialNumbers.SelectedIndex <= 0)
            {
                return;
            }

            _gamepad = new DualShock4((string)comboSerialNumbers.SelectedItem);
            _outputWrapper = new ToXbox360<DS4Button>(_gamepad, DefaultMappers.DualShock4ToXboxButtonMapper);

            labelSmallInfo.Content = $"Serial number: {_gamepad.SerialNumber}";

            _gamepad.OnStateChanged += (a, b) =>
            {
                Dispatcher.Invoke(UpdateControllerInformation);
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        lightBar.Fill = new SolidColorBrush(Color.FromRgb(_gamepad.LightBar.Red, _gamepad.LightBar.Green, _gamepad.LightBar.Blue));
                    }
                    catch (NullReferenceException)
                    {
                        // ignore
                    }
                    catch (ObjectDisposedException)
                    {
                        // ignore
                    }
                });
            };

            _gamepad.Buttons.OnButtonDown += (_, button) =>
            {
                var control = GetControlForButton(button);
                if (control != null)
                {
                    Dispatcher.Invoke(() => { control.Background = Brushes.Red; });
                }
            };

            _gamepad.Buttons.OnButtonUp += (_, button) =>
            {
                var control = GetControlForButton(button);
                if (control != null)
                {
                    Dispatcher.Invoke(() => { control.Background = Brushes.DarkRed; });
                }
            };

            _gamepad.LeftTrigger.OnChange += (_, pressure) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ShowTriggerPressure(labelL2, pressure);
                });
            };

            _gamepad.RightTrigger.OnChange += (_, pressure) =>
            {
                Dispatcher.Invoke(() =>
                {
                    ShowTriggerPressure(labelR2, pressure);
                });
            };

            _gamepad.StartTracking();
            _outputWrapper.Connect();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _outputWrapper?.Dispose();
            _gamepad?.Dispose();
        }

        private void ShowTriggerPressure(Label label, float pressure)
        {
            var rMin = Brushes.DarkRed.Color.R;
            var rMax = Brushes.Red.Color.R;
            label.Background = new SolidColorBrush(Color.FromRgb((byte)(rMin + (rMax - rMin) * pressure), 0, 0));
        }

        private Control? GetControlForButton(DS4Button button)
        {
            var map = new Dictionary<DS4Button, Control>
            {
                { DS4Button.L1, labelL1 },
                { DS4Button.R1, labelR1 },
                { DS4Button.L3, labelL3 },
                { DS4Button.R3, labelR3 },
                { DS4Button.Cross, labelCross },
                { DS4Button.Circle, labelCircle },
                { DS4Button.Square, labelSquare },
                { DS4Button.Triangle, labelTriangle },
                { DS4Button.DpadUp, labelUp },
                { DS4Button.DpadDown, labelDown },
                { DS4Button.DpadLeft, labelLeft },
                { DS4Button.DpadRight, labelRight },
                { DS4Button.Share, labelShare },
                { DS4Button.Options, labelOptions },
                { DS4Button.PS, labelPS },
                { DS4Button.TouchPad, labelTouchPad }
            };

            if (map.ContainsKey(button))
            {
                return map[button];
            }

            return null;
        }

        private void UpdateControllerInformation()
        {
            if (_gamepad == null)
            {
                labelSmallInfo.Content = "Please select a serial number.";
                labelConnection.Content = "Connection: N/A";
                labelBattery.Content = "Battery: N/A";
                HandleStick(labelL3, leftStickPosition, 0, 0);
                HandleStick(labelR3, rightStickPosition, 0, 0);
                return;
            }
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                labelSmallInfo.Content = $"Serial number: {_gamepad.SerialNumber}, Avg. polling rate: {Math.Round(_gamepad?.MeasuredPollingRate.Average ?? 0, 3)}ms";
                if (_gamepad.IsConnected)
                {
                    labelConnection.Content = $"Connection: {(_gamepad.IsBluetooth ? "Bluetooth" : "USB")}";
                }
                else
                {
                    labelConnection.Content = "Connection: Disconnected";
                }
                labelBattery.Content = $"Battery: {_gamepad.Battery.Percentage}% {(_gamepad.Battery.IsCharging ? "(charging)" : string.Empty)}";
                HandleStick(labelL3, leftStickPosition, _gamepad.LeftJoystick.X, _gamepad.LeftJoystick.Y);
                HandleStick(labelR3, rightStickPosition, _gamepad.RightJoystick.X, _gamepad.RightJoystick.Y);
#pragma warning restore CS8602
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }

        private void HandleStick(Label socket, Ellipse stick, float x, float y)
        {
            const float scale = 0.9f;

            stick.Width = 4;
            stick.Height = 4;

            var formX = socket.Margin.Left + socket.Width / 2 - stick.Width / 2 + socket.Width / 2 * x * scale;
            var formY = socket.Margin.Top + socket.Height / 2 - stick.Width / 2 - socket.Height / 2 * y * scale;

            stick.Margin = new Thickness(formX, formY, stick.Margin.Right, stick.Margin.Bottom);
        }

        private void ButtonTestRumble_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                _gamepad?.TestRumble();
            });
        }

        private void ButtonSetColor_Click(object sender, RoutedEventArgs e)
        {
            if (_gamepad == null)
            {
                return;
            }

            var dialog = new ColorPicker(_gamepad.LightBar.Red, _gamepad.LightBar.Green, _gamepad.LightBar.Blue);

            dialog.OnColorChanged += (a, b) =>
            {
                _gamepad.LightBar.Red = dialog.Red;
                _gamepad.LightBar.Green = dialog.Green;
                _gamepad.LightBar.Blue = dialog.Blue;
            };

            dialog.ShowDialog();
        }
    }
}
