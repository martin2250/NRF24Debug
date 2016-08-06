using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet.Main;
using LibUsbDotNet;

namespace NRF24Debug
{
	class Radio
	{
		static UsbDeviceFinder filter = new UsbDeviceFinder(0x1209, 0x2250);
		UsbDevice dev;

		public Radio()
		{
			dev = UsbDevice.OpenUsbDevice(filter);

			if (dev == null)
				throw new Exception("No Device Found!");

			dev.Open();
		}

		public void Close()
		{
			dev.Close();
		}

		public void SetConfiguration(RadioConfiguration conf)
		{
			byte[] c = new byte[16];

			for (int i = 0; i < 5; i++)
			{
				if (!conf.Pipes[i].Enabled)
					continue;
				byte info = 1 << 7;

				if (conf.Pipes[i].AutoAcknowledge)
					info |= 1 << 6;

				if (conf.Pipes[i].DynamicPayload)
					info |= 0x3F;
				else
					info |= conf.Pipes[i].PayloadWidth;

				c[i * 2] = info;

				c[(i * 2) + 1] = conf.Pipes[i].Address;
			}

			c[10] = conf.Channel;

			byte misc = 0;
			misc |= (byte)(((byte)conf.Rate) << 6);
			misc |= (byte)(((byte)conf.Power) << 4);
			misc |= (byte)(conf.RetransmitCount & 0xF);

			c[11] = misc;

			for (int b = 0; b < 4; b++)
			{
				c[12 + b] = conf.AddressPrefix[b];
			}

			UsbSetupPacket sup = new UsbSetupPacket(0x40, 0x01, 0, 0, 16);

			int l;

			dev.ControlTransfer(ref sup, c, 16, out l);

			if (l != 16)
				throw new Exception($"{l} bytes written");
		}

		public RXPacket PollForRxPacket()
		{
			byte[] buffer = new byte[1];
			UsbSetupPacket sup = new UsbSetupPacket(0xC0, 0x03, 0, 0, 1);

			int len;

			dev.ControlTransfer(ref sup, buffer, buffer.Length, out len);

			if (len != buffer.Length)
				throw new Exception($"{len} bytes transferred instead of {buffer.Length}");

			if (buffer[0] == 0)
				return null;

			byte payloadwidth = (byte)(buffer[0] & 0x3F);

			buffer = new byte[payloadwidth + 2];

			sup = new UsbSetupPacket(0xC0, 0x20, 0, 0, (short)buffer.Length);


			dev.ControlTransfer(ref sup, buffer, buffer.Length, out len);

			if (len != buffer.Length)
				throw new Exception($"{len} bytes transferred instead of {buffer.Length}");

			RXPacket p = new RXPacket();
			p.Pipe = buffer[0];
			p.Time = DateTime.Now.ToLongTimeString();
			p.Payload = buffer.Skip(2).ToArray();

			return p;
		}

		public void Send(TXPacket p)
		{
			UsbSetupPacket sup = new UsbSetupPacket(0x40, 0x10, 0, 0, (short)(p.Payload.Length + 6));

			byte[] buffer = new byte[p.Payload.Length + 6];

			buffer[0] = (byte)p.Payload.Length;

			if (p.AutoAcknowledge)
				buffer[0] |= 1 << 6;

			p.Address.CopyTo(buffer, 1);
			p.Payload.CopyTo(buffer, 6);

			int len;

			dev.ControlTransfer(ref sup, buffer, buffer.Length, out len);

			if (len != buffer.Length)
				throw new Exception($"{len} bytes transferred instead of {buffer.Length}");
		}

		public void Reset()
		{
			UsbSetupPacket sup = new UsbSetupPacket(0x40, 0x02, 0, 0, 0);
			byte[] buffer = new byte[0];
			int len;
			dev.ControlTransfer(ref sup, buffer, 0, out len);
		}
	}
}
