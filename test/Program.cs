﻿using LibUsbDotNet;
using LibUsbDotNet.Info;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
	class Program
	{
		static void Main(string[] args)
		{
			UsbDevice MyUsbDevice;

			UsbRegDeviceList allDevices = UsbDevice.AllDevices;
			foreach (UsbRegistry usbRegistry in allDevices)
			{
				if (usbRegistry.Open(out MyUsbDevice))
				{
					Console.WriteLine(MyUsbDevice.Info.ToString());
					for (int iConfig = 0; iConfig < MyUsbDevice.Configs.Count; iConfig++)
					{
						UsbConfigInfo configInfo = MyUsbDevice.Configs[iConfig];
						Console.WriteLine(configInfo.ToString());

						ReadOnlyCollection<UsbInterfaceInfo> interfaceList = configInfo.InterfaceInfoList;
						for (int iInterface = 0; iInterface < interfaceList.Count; iInterface++)
						{
							UsbInterfaceInfo interfaceInfo = interfaceList[iInterface];
							Console.WriteLine(interfaceInfo.ToString());

							ReadOnlyCollection<UsbEndpointInfo> endpointList = interfaceInfo.EndpointInfoList;
							for (int iEndpoint = 0; iEndpoint < endpointList.Count; iEndpoint++)
							{
								Console.WriteLine(endpointList[iEndpoint].ToString());
							}
						}
					}
				}
			}


			// Free usb resources.
			// This is necessary for libusb-1.0 and Linux compatibility.
			UsbDevice.Exit();

			// Wait for user input..
			Console.ReadKey();
		}
	}
}
