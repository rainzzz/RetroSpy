﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroSpy.Readers
{
    static public class SuperNESandNES
    {
        static ControllerState readPacketButtons(byte[] packet, string[] buttons)
        {
            if (packet.Length < buttons.Length) return null;

            var state = new ControllerStateBuilder();

            for (int i = 0; i < buttons.Length; ++i) {
                if (string.IsNullOrEmpty(buttons[i])) continue;
                state.SetButton(buttons[i], packet[i] != 0x00);
            }

            return state.Build();
        }


        static ControllerState readPacketButtons_ascii(byte[] packet, string[] buttons)
        {
            if (packet.Length < buttons.Length) return null;

            var state = new ControllerStateBuilder();

            for (int i = 0; i < buttons.Length; ++i)
            {
                if (string.IsNullOrEmpty(buttons[i])) continue;
                state.SetButton(buttons[i], packet[i] != '0');
            }

            return state.Build();
        }
        static readonly string[] BUTTONS_NES = {
            "a", "b", "select", "start", "up", "down", "left", "right"
        };

        static readonly string[] BUTTONS_SNES = {
            "b", "y", "select", "start", "up", "down", "left", "right", "a", "x", "l", "r", null, null, null, null
        };

        static readonly string[] BUTTONS_INTELLIVISION = {
            "n", "nne", "ne", "ene", "e", "ese", "se", "sse", "s", "ssw", "sw", "wsw", "w", "wnw", "nw", "nnw", "1", "2", "3", "4", "5", "6", "7", "8", "9", "clear", "0", "enter", "topleft", "topright", "bottomleft", "bottomright"
        };

        static readonly string[] BUTTONS_CD32 =
        {
            null, "blue", "red", "yellow", "green", "forward", "backward", "pause", null
        };

        static readonly string[] BUTTONS_PSCLASSIC =
        {
            "r1", "l1", "r2", "l2", "square", "x", "circle", "triangle", null, null, "down", "up", "right", "left", "start", "select"
        };

        static public ControllerState ReadFromPacket_Intellivision(byte[] packet)
        {
            return readPacketButtons(packet, BUTTONS_INTELLIVISION);
        }

        static public ControllerState ReadFromPacket_NES (byte[] packet) {
            return readPacketButtons(packet, BUTTONS_NES);
        }

        static public ControllerState ReadFromPacket_PSClassic(byte[] packet) {
                return readPacketButtons_ascii(packet, BUTTONS_PSCLASSIC);
        }

        static public ControllerState ReadFromPacket_CD32(byte[] packet)
        {
            ControllerStateBuilder state = null;
            if (packet.Length == 13)
            {
                return Classic.ReadFromPacket(packet);
            }
            if (packet.Length == BUTTONS_CD32.Length)
            {
                state = new ControllerStateBuilder();

                for (int i = 0; i < BUTTONS_CD32.Length; ++i)
                {
                    if (string.IsNullOrEmpty(BUTTONS_CD32[i])) continue;
                    state.SetButton(BUTTONS_CD32[i], (packet[i] & 0b10000000) == 0x00);
                }

                state.SetButton("up", (packet[8] & 0b00000001) == 0);
                state.SetButton("down", (packet[0] & 0b00000100) == 0);
                state.SetButton("left", (packet[0] & 0b00001000) == 0);
                state.SetButton("right", (packet[0]& 0b00010000) == 0);
            }
            else if (packet.Length == 19)
            {
                state = new ControllerStateBuilder();

                state.SetButton("left", packet[0] != 0x00);
                state.SetButton("right", packet[2] != 0x00);

                sbyte xVal = (sbyte)SignalTool.readByteBackwards(packet, 3);
                sbyte yVal = (sbyte)SignalTool.readByteBackwards(packet, 11);

                SignalTool.SetMouseProperties( xVal / -128.0f, yVal / 128.0f, state);
            }

            return state != null ? state.Build() : null;
        }

        static public ControllerState ReadFromPacket_SNES (byte[] packet) {
            if (packet.Length < BUTTONS_SNES.Length) return null;

            var state = new ControllerStateBuilder();

            for (int i = 0; i < BUTTONS_SNES.Length; ++i)
            {
                if (string.IsNullOrEmpty(BUTTONS_SNES[i])) continue;
                state.SetButton(BUTTONS_SNES[i], packet[i] != 0x00);
            }

            if (state != null && packet.Length == 32 && packet[15] != 0x00)
            {
                float y = (float)(SignalTool.readByte(packet, 17, 7, 0x1) * ((packet[16] & 0x1) != 0 ? 1 : -1)) / 127;
                float x = (float)(SignalTool.readByte(packet, 25, 7, 0x1) * ((packet[24] & 0x1) != 0 ? -1 : 1)) / 127;
                SignalTool.SetMouseProperties(x, y, state);

            }

            return state.Build();
        }

    }
}
