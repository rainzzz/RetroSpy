﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroSpy.Readers
{
    sealed public class SSHControllerReader : IControllerReader 
    {
        public event StateEventHandler ControllerStateChanged;
        public event EventHandler ControllerDisconnected;

        Func <byte[], ControllerState> _packetParser;
        SSHMonitor _serialMonitor;

        public SSHControllerReader(string hostname, string arguments, Func <byte[], ControllerState> packetParser) 
        {
            _packetParser = packetParser;

            _serialMonitor = new SSHMonitor(hostname, arguments);
            _serialMonitor.PacketReceived += serialMonitor_PacketReceived;
            _serialMonitor.Disconnected += serialMonitor_Disconnected;
            _serialMonitor.Start ();
        }

        void serialMonitor_Disconnected(object sender, EventArgs e)
        {
            Finish ();
            if (ControllerDisconnected != null) ControllerDisconnected (this, EventArgs.Empty);
        }

        void serialMonitor_PacketReceived (object sender, byte[] packet)
        {
            if (ControllerStateChanged != null) {
                var state = _packetParser (packet);
                if (state != null) {
                    ControllerStateChanged (this, state);
                }
            }
        }

        public void Finish ()
        {
            if (_serialMonitor != null) {
                _serialMonitor.Stop ();
                _serialMonitor = null;
            }
        }
    }
}
