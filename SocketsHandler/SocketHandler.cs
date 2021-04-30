using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SocketsHandler
{
    public class SocketHandler
    {
        private TcpClient _TcpClient;

        private NetworkStream _NetworkStream;

        public SocketHandler(TcpClient tcpClient)
        {
            _TcpClient = tcpClient;

            _NetworkStream = _TcpClient.GetStream();
        }

        public void SendMessage(byte[] Message, int? FirstFlag = null, int? SecondFlag = null)
        {
            try
            {
                int MessageLength = Message.Length;

                bool UseFirstFlag = (FirstFlag != null);
                bool UseSecondFlag = ((SecondFlag != null) && UseFirstFlag);

                int HeaderMessageLength = 4 + (UseFirstFlag ? 4 : 0) + (UseSecondFlag ? 4 : 0);

                var HeaderStream = new MemoryStream();

                HeaderStream.Write(BitConverter.GetBytes(MessageLength), 0, 4);

                if (UseFirstFlag)
                    HeaderStream.Write(BitConverter.GetBytes((int)FirstFlag), 0, 4);
                if (UseSecondFlag)
                    HeaderStream.Write(BitConverter.GetBytes((int)SecondFlag), 0, 4);

                byte[] HeaderArr = HeaderStream.ToArray();

                _NetworkStream.Write(HeaderArr, 0, HeaderArr.Length);

                _NetworkStream.Write(Message, 0, MessageLength);
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public byte[] ReceiveMessage()
        {
            try
            {
                byte[] HeaderLength = new byte[4];

                if (_NetworkStream.Read(HeaderLength, 0, 4) > 0)
                {
                    int MessageLength = BitConverter.ToInt32(HeaderLength, 0);

                    return AggregateChuncks(MessageLength).ToArray();
                }
                else
                    return new byte[0];
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public byte[] ReceiveMessage(ref int Flag)
        {
            try
            {
                byte[] HeaderArr = new byte[8];

                if (_NetworkStream.Read(HeaderArr, 0, 8) > 0)
                {
                    byte[] MessageLegthArr = new byte[4];
                    byte[] FlagArr = new byte[4];
                    Array.Copy(HeaderArr, 0, MessageLegthArr, 0, 4);
                    Array.Copy(HeaderArr, 4, FlagArr, 0, 4);

                    int MessageLength = BitConverter.ToInt32(MessageLegthArr, 0);
                    Flag = BitConverter.ToInt32(FlagArr, 0);

                    return AggregateChuncks(MessageLength).ToArray();
                }
                else
                    return new byte[0];
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public byte[] ReceiveMessage(ref int FirstFlag, ref int SecondFlag)
        {
            try
            {
                byte[] HeaderArr = new byte[12];

                if (_NetworkStream.Read(HeaderArr, 0, 12) > 0)
                {
                    byte[] MessageLegthArr = new byte[4];
                    byte[] FirstFlagArr = new byte[4];
                    byte[] SecondFlagArr = new byte[4];

                    Array.Copy(HeaderArr, 0, MessageLegthArr, 0, 4);
                    Array.Copy(HeaderArr, 4, FirstFlagArr, 0, 4);
                    Array.Copy(HeaderArr, 8, SecondFlagArr, 0, 4);

                    int MessageLength = BitConverter.ToInt32(MessageLegthArr, 0);
                    FirstFlag = BitConverter.ToInt32(FirstFlagArr, 0);
                    SecondFlag = BitConverter.ToInt32(SecondFlagArr, 0);

                    return AggregateChuncks(MessageLength).ToArray();
                }
                else
                    return new byte[0];
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        private MemoryStream AggregateChuncks(int MessageLength)
        {
            try
            {
                var MessageStram = new MemoryStream();

                int Accumulator = 0;
                int LengthLeft = MessageLength;

                while (Accumulator < MessageLength)
                {
                    byte[] ChunckArr = new byte[LengthLeft];
                    int ChunckSize = _NetworkStream.Read(ChunckArr, 0, LengthLeft);

                    if (ChunckSize > 0)
                        MessageStram.Write(ChunckArr, 0, ChunckSize);

                    Accumulator += ChunckSize;
                    LengthLeft -= ChunckSize;
                }

                return MessageStram;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
    }
}
