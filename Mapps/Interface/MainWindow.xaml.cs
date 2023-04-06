using Mapps.Gamepads.Events;
using Mapps.Gamepads.OutputWrappers;
using Mapps.Gamepads.Styles.PlayStation;
using Mapps.Gamepads.Styles.PlayStation.DualShock4;
using Mapps.Gamepads.Styles.Xbox;
using Mapps.Mappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Interface;

public enum ControlState
{
    Normal,
    Recording,
    Playback,
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ControlState _currentState = ControlState.Normal;
    private SemaphoreSlim _gamepadSemaphore = new SemaphoreSlim(1);
    private IGamepadEventProducer<PSButton> _gamepadEventProducer;
    private GamepadEventRecorder<PSButton> _gamepadEventRecorder;
    private ButtonMapper<PSButton, XboxButton> _buttonMapper;
    private IGamepadOutputWrapper<PSButton> _outputWrapper;

    private DualShock4? _gamepad = null;
    private CancellationTokenSource? _playbackCancellationTokenSource = null;
    private (byte Red, byte Green, byte Blue) _desiredColor = (0, 128, 64);
    private (byte Red, byte Green, byte Blue) _recordingColor = (255, 0, 0);
    private (byte Red, byte Green, byte Blue) _playbackColor = (255, 255, 0);

    public MainWindow()
    {
        _gamepadEventProducer = new GamepadEventProducer<PSButton>();
        _gamepadEventRecorder = new GamepadEventRecorder<PSButton>(_gamepadEventProducer);

        _buttonMapper = DefaultMappers.DualShock4ToXboxButtonMapper;
        _buttonMapper.RemoveMapping(PSButton.PS);

        _outputWrapper = new ToXbox360<PSButton>(_buttonMapper);
        _outputWrapper.SetEventSource(_gamepadEventProducer);
        _outputWrapper.Connect();

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
        _gamepadSemaphore.Wait();

        try
        {
            _gamepadEventProducer.Deregister();
            _gamepad?.Dispose();
            _gamepad = null;

            if (comboSerialNumbers.SelectedIndex <= 0)
            {
                return;
            }

            _gamepad = new DualShock4((string)comboSerialNumbers.SelectedItem);

            _gamepadEventProducer.Register(_gamepad);
            _outputWrapper.SetFeedbackGamepad(_gamepad);

            labelSmallInfo.Content = $"Serial number: {_gamepad.SerialNumber}";

            _gamepad.OnStateChanged += (a, b) =>
            {
                Dispatcher.Invoke(UpdateControllerInformation);
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
                if (button == PSButton.PS)
                {
                    if (_currentState == ControlState.Normal)
                    {
                        _gamepadEventRecorder.StartRecording();
                        _currentState = ControlState.Recording;
                    }
                    else if (_currentState == ControlState.Recording)
                    {
                        _gamepadEventRecorder.StopRecording();
                        _outputWrapper.SetEventSource(_gamepadEventRecorder);
                        _playbackCancellationTokenSource = new();

                        Task.Run(async () =>
                        {
                            while (!_playbackCancellationTokenSource.IsCancellationRequested)
                            {
                                await _gamepadEventRecorder.PlayRecording(_playbackCancellationTokenSource.Token);
                            }
                        });

                        _currentState = ControlState.Playback;
                    }
                    else
                    {
                        _playbackCancellationTokenSource!.Cancel();
                        _outputWrapper.SetEventSource(_gamepadEventProducer);
                        _currentState = ControlState.Normal;
                    }
                }

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

            Task.Run(async () =>
            {
                var lowBatteryFlashTimer = Stopwatch.StartNew();

                while (true)
                {
                    try
                    {
                        if (_gamepad is null || !_gamepad.IsConnected)
                        {
                            await Task.Delay(100);
                            continue;
                        }

                        await _gamepadSemaphore.WaitAsync();
                        try
                        {
                            if (!_gamepad.Battery.IsCharging && _gamepad.Battery.Percentage <= 20 || _gamepad.Battery.IsCharging && _gamepad.Battery.Percentage >= 95)
                            {
                                if (lowBatteryFlashTimer.ElapsedMilliseconds >= 200)
                                {
                                    if (_gamepad.LightBar.Red == 0 && _gamepad.LightBar.Green == 0 && _gamepad.LightBar.Blue == 0)
                                    {
                                        _gamepad.LightBar.Red = _desiredColor.Red;
                                        _gamepad.LightBar.Green = _desiredColor.Green;
                                        _gamepad.LightBar.Blue = _desiredColor.Blue;
                                    }
                                    else
                                    {
                                        _gamepad.LightBar.Red = 0;
                                        _gamepad.LightBar.Green = 0;
                                        _gamepad.LightBar.Blue = 0;
                                    }
                                    lowBatteryFlashTimer.Restart();
                                }
                            }
                            else
                            {
                                _gamepad.LightBar.Red = _desiredColor.Red;
                                _gamepad.LightBar.Green = _desiredColor.Green;
                                _gamepad.LightBar.Blue = _desiredColor.Blue;
                            }

                            if (_currentState == ControlState.Recording)
                            {
                                _gamepad.LightBar.Red = _recordingColor.Red;
                                _gamepad.LightBar.Green = _recordingColor.Green;
                                _gamepad.LightBar.Blue = _recordingColor.Blue;
                            }
                            else if (_currentState == ControlState.Playback)
                            {
                                _gamepad.LightBar.Red = _playbackColor.Red;
                                _gamepad.LightBar.Green = _playbackColor.Green;
                                _gamepad.LightBar.Blue = _playbackColor.Blue;
                            }

                            Dispatcher.Invoke(() =>
                            {
                                lightBar.Fill = new SolidColorBrush(Color.FromRgb(_gamepad.LightBar.Red, _gamepad.LightBar.Green, _gamepad.LightBar.Blue));
                            });
                        }
                        finally
                        {
                            _gamepadSemaphore.Release();
                        }

                        await Task.Delay(10);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Exception in lightbar task: {e}");
                        await Task.Delay(1000);
                    }
                }
            });

            _gamepad.StartTracking();
        }
        finally
        {
            _gamepadSemaphore.Release();
        }
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

    private Control? GetControlForButton(PSButton button)
    {
        var map = new Dictionary<PSButton, Control>
        {
            { PSButton.L1, labelL1 },
            { PSButton.R1, labelR1 },
            { PSButton.L3, labelL3 },
            { PSButton.R3, labelR3 },
            { PSButton.Cross, labelCross },
            { PSButton.Circle, labelCircle },
            { PSButton.Square, labelSquare },
            { PSButton.Triangle, labelTriangle },
            { PSButton.DpadUp, labelUp },
            { PSButton.DpadDown, labelDown },
            { PSButton.DpadLeft, labelLeft },
            { PSButton.DpadRight, labelRight },
            { PSButton.Share, labelShare },
            { PSButton.Options, labelOptions },
            { PSButton.PS, labelPS },
            { PSButton.TouchPad, labelTouchPad }
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

        var dialog = new ColorPicker(_desiredColor.Red, _desiredColor.Green, _desiredColor.Blue);

        dialog.OnColorChanged += (a, b) =>
        {
            _desiredColor.Red = dialog.Red;
            _desiredColor.Green = dialog.Green;
            _desiredColor.Blue = dialog.Blue;
        };

        dialog.ShowDialog();
    }
}
