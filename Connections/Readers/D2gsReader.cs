using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BattleNet.Logging;

namespace BattleNet.Connections.Readers
{
    class D2gsReader : GenericHandler
    {
        static readonly Int16[] packetSizes =
	    {
		    1, 8, 1, 12, 1, 1, 1, 6, 6, 11, 6, 6, 9, 13, 12, 16,
		    16, 8, 26, 14, 18, 11, 0, 0, 15, 2, 2, 3, 5, 3, 4, 6,
		    10, 12, 12, 13, 90, 90, 0, 40, 103,97, 15, 0, 8, 0, 0, 0,
		    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 34, 8,
		    13, 0, 6, 0, 0, 13, 0, 11, 11, 0, 0, 0, 16, 17, 7, 1,
		    15, 14, 42, 10, 3, 0, 0, 14, 7, 26, 40, 0, 5, 6, 38, 5,
		    7, 2, 7, 21, 0, 7, 7, 16, 21, 12, 12, 16, 16, 10, 1, 1,
		    1, 1, 1, 32, 10, 13, 6, 2, 21, 6, 13, 8, 6, 18, 5, 10,
		    4, 20, 29, 0, 0, 0, 0, 0, 0, 2, 6, 6, 11, 7, 10, 33,
		    13, 26, 6, 8, 0, 13, 9, 1, 7, 16, 17, 7, 0, 0, 7, 8,
		    10, 7, 8, 24, 3, 8, 0, 7, 0, 7, 0, 7, 0, 0, 0, 0,
		    1
	    };

        protected String _character;
        public String Character { get { return _character; } set { _character = value; } }

        protected Byte _classByte;
        public Byte ClassByte { get { return _classByte; } set { _classByte = value; } }

        protected List<byte> _hash;
        protected List<byte> _token;

        public D2gsReader(ref D2gsConnection conn, String character)
            : base(conn)
        {
            _character = character;
        }

        public void SetInfo(List<byte> hash, List<byte> token)
        {
            _hash = hash;
            _token = token;
        }
        public delegate void NoParam();
        public event NoParam NextGame;
        public void Die()
        {
            _connection.Close();
            OnUpdateStatus(Client.Status.STATUS_NOT_IN_GAME);
            NextGame();
        }

        bool GetChatPacketSize(List<byte> input, out Int32 output)
        {
            output = 0;
	        if(input.Count < 12)
		        return false;

	        const Int32 initial_offset = 10;

            Int32 name_offset = input.IndexOf(0,initial_offset);

	        if(name_offset == -1)
		        return false;

            name_offset -= initial_offset;

	        Int32 message_offset = input.IndexOf(0,initial_offset + name_offset + 1);

	        if(message_offset == -1)
		        return false;

            message_offset = message_offset - initial_offset - name_offset -1;

	        output = initial_offset + name_offset + 1 + message_offset + 1;

	        return true;
        }

        // This was taken from Redvex according to qqbot source
        bool GetPacketSize(List<byte> input, out Int32 output)
        {
	        byte identifier = input[0];

	        Int32 size = input.Count;

	        switch(identifier)
	        {
	        case 0x26:
                    if (GetChatPacketSize(input,out output))
			        return true;
		        break;
	        case 0x5b:
		        if(size >= 3)
		        {
			        output = BitConverter.ToInt16(input.ToArray(), 1);
			        return true;
		        }
		        break;
	        case 0x94:
		        if(size >= 2)
		        {
			        output = input[1] * 3 + 6;
			        return true;
		        }
		        break;
	        case 0xa8:
	        case 0xaa:
		        if(size >= 7)
		        {
			        output = (byte)input[6];
			        return true;
		        }
		        break;
	        case 0xac:
		        if(size >= 13)
		        {
			        output = (byte)input[12];
			        return true;
		        }
		        break;
	        case 0xae:
		        if(size >= 3)
		        {
			        output = 3 + BitConverter.ToInt16(input.ToArray(), 1);
			        return true;
		        }
		        break;
	        case 0x9c:
		        if(size >= 3)
		        {
			        output = input[2];
			        return true;
		        }
		        break;
	        case 0x9d:
		        if(size >= 3)
		        {
			        output = input[2];
			        return true;
		        }
		        break;
	        default:
		        if(identifier < packetSizes.Length)
		        {
			        output = packetSizes[identifier];
			        return output != 0;
		        }
		        break;
	        }
            output = 0;
	        return false;
        }

        public override void ThreadFunction()
        {
            List<byte> buffer = new List<byte>();
            byte[] byteBuffer = new byte[4096];
            Int32 bytesRead = 0;
            while (_connection.Socket.Connected)
            {
                try
                {

                    if (_connection.Stream.DataAvailable)
                    {
                        bytesRead = _connection.Stream.Read(byteBuffer, 0, byteBuffer.Length);
                        buffer.AddRange(new List<byte>(byteBuffer).GetRange(0, bytesRead));
                    }
                    else
                    {
                        if (!_connection.Socket.Connected)
                        {
                            Die();
                            break;
                        }
                        Thread.Sleep(100);
                    }
                    while (_connection.Stream.DataAvailable)
                    {
                        buffer.Add((byte)_connection.Stream.ReadByte());
                    }
                }
                catch
                {
                    Logging.Logger.Write("Disconnected from game server");
                    Die();
                    break;
                }

                while (true)
                {
                    try
                    {
                        UInt16 receivedPacket = 0;
                        if (buffer.Count >= 2)
                            receivedPacket = BitConverter.ToUInt16(buffer.ToArray(), 0);
                        if (buffer.Count >= 2 && receivedPacket == (UInt16)0x01af)
                        {
                            Logger.Write("Logging on to game server");

                            byte[] temp = {0x50, 0xcc, 0x5d, 0xed, 
                                       0xb6, 0x19, 0xa5, 0x91};

                            Int32 pad = 16 -_character.Length;

                            byte[] padding = new byte[pad];
                            byte[] characterClass = { _classByte };
                            byte[] joinpacket = _connection.BuildPacket(0x68, _hash, _token, characterClass, BitConverter.GetBytes((UInt32)0xd), temp, zero, System.Text.Encoding.ASCII.GetBytes(_character), padding);
                            _connection.Write(joinpacket);
                            Logger.Write("Join packet sent to server");
                            buffer.RemoveRange(0, 2);
                        }

                        if (buffer.Count < 2 || (buffer[0] >= 0xF0 && buffer.Count < 3))
                        {
                            break;
                        }

                        Int32 headerSize;
                        Int32 length = Huffman.GetPacketSize(buffer, out headerSize);
                        if (length > buffer.Count)
                            break;

                        byte[] compressedPacket = buffer.GetRange(headerSize, length).ToArray();
                        buffer.RemoveRange(0, length + headerSize);


                        byte[] decompressedPacket;
                        Huffman.Decompress(compressedPacket, out decompressedPacket);
                        List<byte> packet = new List<byte>(decompressedPacket);
                        while (packet.Count != 0)
                        {
                            Int32 packetSize;
                            if (!GetPacketSize(packet, out packetSize))
                            {
                                Logger.Write("Failed to determine packet length");
                                break;
                            }
                            List<byte> actualPacket = new List<byte>(packet.GetRange(0, packetSize));
                            packet.RemoveRange(0, packetSize);

                            lock (_connection.Packets)
                            {
                                //Logger.Write("Adding a D2GS packet {0:X2}", actualPacket[0]);
                                _connection.Packets.Enqueue(1, actualPacket);
                            }
                            _connection.PacketsReady.Set();
                            
                        }
                    }
                    catch
                    {
                        OnUpdateStatus(Client.Status.STATUS_NOT_IN_GAME);

                        Logger.Write("Leaving the game.");
                        _connection.Write(new byte[] { 0x69 });

                        Thread.Sleep(500);

                        Die();
                        _connection.Kill();
                        
                        return;
                    }
                }
            }
            Die();
        }
    }
}
