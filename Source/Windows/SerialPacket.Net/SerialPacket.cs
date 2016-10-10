// (c) 2016 matsujirushi
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace matsujirushi.IO.Ports
{
    /// <summary>
    /// Send and receive a packet on SerialPort.
    /// </summary>
    public class SerialPacket : IDisposable
    {
        private readonly byte START_CODE = 0x01;    // SOH
        private readonly byte END_CODE = 0x1a;      // EOF
        private readonly byte ESCAPE_CODE = 0x1b;   // ESC

        private SerialPort _Port;
        private List<byte> _ReceivedData;

        /// <summary>
        /// Construct the SerialPacket.
        /// </summary>
        /// <param name="portName">The port to use.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">Number of the data bits.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">Number of the stop bits.</param>
        public SerialPacket(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            _ReceivedData = new List<byte>();

            _Port = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _Port.DataReceived += DataReceived;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public virtual void Dispose()
        {
            _Port.Dispose();
        }

        /// <summary>
        ///  Open the SerialPort.
        /// </summary>
        public void Open()
        {
            if (!_Port.IsOpen) _Port.Open();
        }

        /// <summary>
        /// Close the SerialPort.
        /// </summary>
        public void Close()
        {
            _Port.Close();
        }

        /// <summary>
        /// Append the packet to send queue.
        /// </summary>
        /// <param name="payload">The payload in packet.</param>
        public void Write(byte[] payload)
        {
            // Convert payload to packet.
            var packet = new List<byte>();
            packet.Add(START_CODE);
            foreach (var d in payload)
            {
                if (d == START_CODE || d == END_CODE || d == ESCAPE_CODE)
                {
                    packet.Add(ESCAPE_CODE);
                    packet.Add((byte)~d);
                }
                else
                {
                    packet.Add(d);
                }
            }
            packet.Add(END_CODE);

            // Write packet to serial port.
            var data = packet.ToArray();
            _Port.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Received the packet.
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        /// <summary>
        /// The args of PacketReceived event.
        /// </summary>
        public class PacketReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// The payload in packet.
            /// </summary>
            public byte[] Payload { get; set; }
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Read data from serial port.
            var data = new byte[_Port.BytesToRead];
            _Port.Read(data, 0, data.Length);

            // Append readed data to list.
            _ReceivedData.AddRange(data);

            for (;;)
            {
                // Pick out a packet.
                var startIndex = _ReceivedData.FindIndex(d => d == START_CODE);
                if (startIndex < 0) return;
                var endIndex = _ReceivedData.FindIndex(startIndex + 1, d => d == END_CODE);
                if (endIndex < 0) return;
                var packet = new byte[endIndex - startIndex + 1];
                _ReceivedData.CopyTo(startIndex, packet, 0, packet.Length);
                _ReceivedData.RemoveRange(0, packet.Length);

                // Convert packet to payload.
                var payload = new List<byte>();
                var escape = false;
                for (int i = 1; i < packet.Length - 1; i++)
                {
                    var d = packet[i];

                    if (escape)
                    {
                        escape = false;
                        payload.Add((byte)~d);
                    }
                    else
                    {
                        if (d == ESCAPE_CODE)
                        {
                            escape = true;
                        }
                        else
                        {
                            payload.Add(d);
                        }
                    }
                }
                if (escape) throw new ApplicationException();

                if (PacketReceived != null)
                {
                    PacketReceived(this, new PacketReceivedEventArgs { Payload = payload.ToArray(), });
                }
            }
        }

    }
}
