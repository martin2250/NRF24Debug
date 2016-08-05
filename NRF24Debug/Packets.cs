using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRF24Debug
{
	public class RXPacket
	{
		public int Pipe { get; set; }
		public string Time { get; set; }

		private byte[] _payload;
		public byte[] Payload
		{
			get
			{
				return _payload;
			}
			set
			{
				_payload = value;
				PayloadS = string.Join(" ", Payload.Select((b) => "0x" + b.ToString("X")));
				PayloadRaw = new string(Payload.Select(b => (char)b).ToArray());
			}
		}

		public string PayloadS { get; private set; }
		public string PayloadRaw { get; private set; }
	}

	public class TXPacket
	{
		public bool AutoAcknowledge { get; set; } = false;
		public byte[] Address { get; set; } = new byte[5];
		public byte[] Payload { get; set; }
	}
}
