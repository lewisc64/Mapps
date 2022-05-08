using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Interface
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        private byte _originalRed;

        private byte _originalGreen;

        private byte _originalBlue;

        public ColorPicker(byte defaultRed, byte defaultGreen, byte defaultBlue)
        {
            InitializeComponent();

            _originalRed = defaultRed;
            _originalGreen = defaultGreen;
            _originalBlue = defaultBlue;

            sliderRed.Value = defaultRed;
            sliderGreen.Value = defaultGreen;
            sliderBlue.Value = defaultBlue;
        }

        public event EventHandler? OnColorChanged;

        public byte Red { get; private set; }

        public byte Green { get; private set; }

        public byte Blue { get; private set; }

        private void SliderRed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textRed.Text = ((byte)sliderRed.Value).ToString();
            UpdateColor();
        }

        private void SliderGreen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textGreen.Text = ((byte)sliderGreen.Value).ToString();
            UpdateColor();
        }

        private void SliderBlue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            textBlue.Text = ((byte)sliderBlue.Value).ToString();
            UpdateColor();
        }

        private void TextRed_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (byte.TryParse(textRed.Text, out var component))
            {
                sliderRed.Value = component;
            }
        }

        private void TextGreen_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (byte.TryParse(textGreen.Text, out var component))
            {
                sliderGreen.Value = component;
            }
        }

        private void TextBlue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (byte.TryParse(textBlue.Text, out var component))
            {
                sliderBlue.Value = component;
            }
        }

        private void UpdateColor()
        {
            Red = (byte)sliderRed.Value;
            Green = (byte)sliderGreen.Value;
            Blue = (byte)sliderBlue.Value;

            background.Background = new SolidColorBrush(Color.FromRgb(Red, Green, Blue));

            OnColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            sliderRed.Value = _originalRed;
            sliderGreen.Value = _originalGreen;
            sliderBlue.Value = _originalBlue;
        }

        private void ButtonDone_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
