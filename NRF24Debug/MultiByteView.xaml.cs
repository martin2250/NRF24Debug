using System.Windows;
using System.Windows.Controls;

namespace NRF24Debug
{
	public partial class MultiByteView : UserControl
	{
		public string Buffer
		{
			get { return (string)GetValue(BufferProperty); }
			set { SetValue(BufferProperty, value); Update(); }
		}

		public static readonly DependencyProperty BufferProperty = DependencyProperty.Register("Buffer", typeof(string), typeof(MultiByteView), new PropertyMetadata(""));


		private bool _isreadonly = false;
		public bool IsReadOnly
		{
			get { return _isreadonly; }
			set
			{
				_isreadonly = value;
			}
		}

		public MultiByteView()
		{
			InitializeComponent();
			this.Loaded += MultiByteView_Loaded;
		}

		private void MultiByteView_Loaded(object sender, RoutedEventArgs e)
		{
			Update();
		}

		public void Update()
		{
			using (var d = Dispatcher.DisableProcessing())
			{
				stack.Children.Clear();

				foreach (byte b in Buffer)
				{
					stack.Children.Add(new ByteView(b) { IsReadOnly = IsReadOnly});
				}
			}
		}
	}
}
