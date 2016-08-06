using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace NRF24Debug
{
	public partial class MainWindow : Window
	{
		Radio radio;

		private bool _connected = false;
		public bool Connected
		{
			set
			{
				menuItemConnect.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
				menuItemDisconnect.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
				Title = "NRF24Debug" + (value ? " - Connected" : "");
				buttonWriteConfig.IsEnabled = value;
				_connected = value;
			}
			get { return _connected; }
		}

		ToggleButton[] pipesEN = new ToggleButton[5];
		CheckBox[] pipesAA = new CheckBox[5];
		ComboBox[] pipesPW = new ComboBox[5];
		ByteView[] pipesAddr = new ByteView[5];

		public List<RXPacket> Received = new List<RXPacket>();

		DispatcherTimer timer;

		ByteView[] txBuffer = new ByteView[32];
		ByteView[] txAddr = new ByteView[32];

		public MainWindow()
		{
			InitializeComponent();

			for (int pipe = 0; pipe < 5; pipe++)
			{
				ToggleButton tb = pipesEN[pipe] = new ToggleButton();
				tb.Content = $"P{pipe + 1}";
				Grid.SetColumn(tb, 0);
				Grid.SetRow(tb, pipe + 1);
				gridPipes.Children.Add(tb);

				CheckBox cb = pipesAA[pipe] = new CheckBox();
				cb.HorizontalAlignment = HorizontalAlignment.Center;
				cb.VerticalAlignment = VerticalAlignment.Center;
				Grid.SetColumn(cb, 1);
				Grid.SetRow(cb, pipe + 1);
				gridPipes.Children.Add(cb);

				ComboBox cob = pipesPW[pipe] = new ComboBox();
				for (int i = 0; i <= 32; i++)
					cob.Items.Add(i);
				cob.Items.Add("DYNPD");
				cob.SelectedIndex = 33;
				Grid.SetColumn(cob, 2);
				Grid.SetRow(cob, pipe + 1);
				gridPipes.Children.Add(cob);

				ByteView bv = pipesAddr[pipe] = new ByteView();
				bv.HorizontalAlignment = HorizontalAlignment.Right;
				Grid.SetColumn(bv, 3);
				Grid.SetRow(bv, pipe + 1);
				gridPipes.Children.Add(bv);


				txAddr[pipe] = new ByteView();
				stackPanelTX.Children.Add(txAddr[pipe]);
			}

			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(25);
			timer.Tick += PollTimer_Elapsed;

			for (int i = 0; i < 32; i++)
			{
				comboBoxSendCount.Items.Add(i + 1);
				var bv = txBuffer[i] = new ByteView();
				bv.Margin = new Thickness(1);
				wrapPanelTX.Children.Add(bv);
			}
			comboBoxSendCount.SelectionChanged += ComboBoxSendCount_SelectionChanged;
			comboBoxSendCount.SelectedIndex = 31;
		}

		private void ComboBoxSendCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			for (int i = 0; i < 32; i++)
			{
				txBuffer[i].Visibility = (i <= comboBoxSendCount.SelectedIndex) ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		void Disconnect()
		{
			timer.Stop();
			Connected = false;
			if (radio != null)
				radio.Close();
			radio = null;
		}

		private void PollTimer_Elapsed(object sender, EventArgs e)
		{
			try
			{
				var rx = radio.PollForRxPacket();

				if (rx != null)
				{
					dataGridRec.Items.Add(rx);
				}
			}
			catch (Exception ex)
			{
				Disconnect();
				MessageBox.Show("Error polling the Adapter:\n" + ex.Message);
			}
		}

		private void menuItemConnect_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				radio = new Radio();
				Connected = true;
				timer.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error connecting to device:\n" + ex.Message);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (radio != null)
				radio.Close();
		}

		private void menuItemDisconnect_Click(object sender, RoutedEventArgs e)
		{
			Disconnect();
		}

		private void ComboBoxHex_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool hex = ((ComboBox)sender).SelectedIndex == 0;
			foreach (ByteView b in FindVisualChildren<ByteView>(this))
			{
				b.Hex = hex;
			}
		}

		public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}

		private void ButtonWriteConfig_Click(object sender, RoutedEventArgs e)
		{
			RadioConfiguration c = new RadioConfiguration();
			c.Channel = byteViewChannel.Value;
			c.Rate = (BitRate)comboBoxDataRate.SelectedIndex;
			c.Power = (RFPower)comboBoxRFPower.SelectedIndex;
			c.AddressPrefix[0] = bvAP0.Value;
			c.AddressPrefix[1] = bvAP1.Value;
			c.AddressPrefix[2] = bvAP2.Value;
			c.AddressPrefix[3] = bvAP3.Value;

			for (int i = 0; i < 5; i++)
			{
				if (!(c.Pipes[i].Enabled = pipesEN[i].IsChecked.Value))
					continue;
				c.Pipes[i].Address = pipesAddr[i].Value;
				c.Pipes[i].AutoAcknowledge = pipesAA[i].IsChecked.Value;
				c.Pipes[i].DynamicPayload = pipesPW[i].SelectedIndex == 33;
				c.Pipes[i].PayloadWidth = (pipesPW[i].SelectedIndex == 33) ? (byte)0 : ((byte)pipesPW[i].SelectedIndex);
			}

			c.RetransmitCount = bvRetransmit.Value;

			try
			{
				radio.SetConfiguration(c);

				MessageBox.Show("Configuration written");
			}
			catch (Exception ex)
			{
				Disconnect();
				MessageBox.Show("Could not write Config:\n" + ex.Message);
			}
		}

		private void buttonSend_Click(object sender, RoutedEventArgs e)
		{
			TXPacket p = new TXPacket();
			p.Payload = new byte[comboBoxSendCount.SelectedIndex + 1];

			for (int i = 0; i < comboBoxSendCount.SelectedIndex + 1; i++)
			{
				p.Payload[i] = txBuffer[i].Value;
			}

			for (int i = 0; i < 5; i++)
			{
				p.Address[i] = txAddr[i].Value;
			}

			p.AutoAcknowledge = checkBoxAutoAcknowledge.IsChecked.Value;

			try
			{
				radio.Send(p);
			}
			catch (Exception ex)
			{
				Disconnect();
				MessageBox.Show("Error sending Packet:\n" + ex.Message);
			}
		}

		private void menuItemReset_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				radio.Reset();
			}
			catch (Exception ex)
			{
				Disconnect();
				MessageBox.Show("Error resetting device:\n" + ex.Message);
			}
		}

		private void MenuItemClearRX_Click(object sender, RoutedEventArgs e)
		{
			dataGridRec.Items.Clear();
		}
	}
}
