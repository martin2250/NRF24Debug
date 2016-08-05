using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NRF24Debug
{
	public partial class ByteView : UserControl
	{
		private byte _value;
		public byte Value
		{
			get { return _value; }
			set
			{
				_value = value;
				UpdateText();
			}
		}

		private byte _max = 255;
		public byte Max
		{
			get { return _max; }
			set
			{
				_max = value;
			}
		}

		private bool _hex = true;
		public bool Hex
		{
			get { return _hex; }
			set
			{
				_hex = value;
				textBlockHex.Visibility = Hex ? Visibility.Visible : Visibility.Collapsed;
				UpdateText();
			}
		}

		private bool _isreadonly = false;
		public bool IsReadOnly
		{
			get { return _isreadonly; }
			set
			{
				_isreadonly = value;
				textBoxValue.IsReadOnly = value;
			}
		}

		public ByteView() : this(0) { }
		public ByteView(byte value)
		{
			InitializeComponent();
			Value = value;
			UpdateText();

			this.AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(UserControl_PreviewMouseDown), true);
		}

		private bool TextChangeInternal = false;
		private void UpdateText()
		{
			TextChangeInternal = true;

			int start = textBoxValue.SelectionStart;
			textBoxValue.Text = Value.ToString(Hex ? "X" : "");
			textBoxValue.SelectionStart = start;

			TextChangeInternal = false;
		}

		private void textBoxValue_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (TextChangeInternal)
				return;

			byte res;

			if (byte.TryParse("0" + textBoxValue.Text, Hex ? NumberStyles.HexNumber : NumberStyles.Number, null, out res))
			{
				if (res <= Max)
					Value = res;
			}

			UpdateText();
		}

		private void textBoxValue_GotFocus(object sender, RoutedEventArgs e)
		{
			TextBox tb = (TextBox)e.OriginalSource;
			tb.Dispatcher.BeginInvoke(
				new Action(delegate
				{
					tb.SelectAll();
				}), System.Windows.Threading.DispatcherPriority.Input);
		}

		private void UserControl_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			FocusManager.SetFocusedElement(this, textBoxValue);
		}
	}
}
