using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRF24Debug
{
	public enum RFPower
	{
		[Description("0 dBm")]
		P_0dBm,
		[Description("-6 dBm")]
		P_6dBm,
		[Description("-12 dBm")]
		P_12dBm,
		[Description("-18 dBm")]
		P_18dBm
	}

	public enum BitRate
	{
		[Description("2Mbit/s")]
		B_2M,
		[Description("1Mbit/s")]
		B_1M,
		[Description("500kbit/s")]
		B_500k,
	}

	public class RadioConfiguration
	{
		public Pipe[] Pipes = new Pipe[5];
		public byte Channel;
		public byte[] AddressPrefix = new byte[4];
		public byte RetransmitCount;

		public RFPower Power;
		public BitRate Rate;
	}

	public struct Pipe
	{
		public bool Enabled { get; set; }
		public byte Address { get; set; }
		public bool DynamicPayload { get; set; }
		public bool AutoAcknowledge { get; set; }
		public byte PayloadWidth { get; set; }
	}
}
