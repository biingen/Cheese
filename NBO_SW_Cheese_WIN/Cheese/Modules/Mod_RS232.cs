using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Ports;
using Cheese;
using log4net;
using System.Reflection;

namespace ModuleLayer
{
    public class Mod_RS232
    {
        const int bufferSize = 2048;
        private SerialPort _SerialPortHandle = new SerialPort();
        public Queue ReceiveQueue = new Queue();
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);        //log4net
        public Mod_RS232()
        {
            //reserved
        }

        //=== main Write function ===//
        public int WriteDataOut(byte[] InBuf, int DataLength)
        {
            if (_SerialPortHandle.IsOpen == true)
            {
                try
                {
                    ReceiveQueue.Clear();   //clear input buffer
                    _SerialPortHandle.DiscardInBuffer();
                    _SerialPortHandle.DiscardOutBuffer();
                    _SerialPortHandle.Write(InBuf, 0, DataLength);
                }
                catch (System.ArgumentException)
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
            return -1;
        }
        public int WriteDataOut(string InBuf, int DataLength)
        {
            if (_SerialPortHandle.IsOpen == true)
            {
                try
                {
                    ReceiveQueue.Clear();   //clear input buffer
                    //_SerialPortHandle.Write(InBuf);
                    log.Debug("[" + _SerialPortHandle + "] DataSent:  " + InBuf);
                    _SerialPortHandle.WriteLine(InBuf);     //for IO cmd doing /r/n
                }
                catch (System.ArgumentException)
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
            return -1;
        }
        public int WriteDataOut(char[] InBuf, int DataLength)
        {

            if (_SerialPortHandle.IsOpen == true)
            {
                try
                {
                    ReceiveQueue.Clear();   //clear input buffer
                    _SerialPortHandle.Write(InBuf, 0, DataLength);
                }
                catch (System.ArgumentException)
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
            return -1;
        }

        public int SpecificDequeue(int Len, ref byte[] retBuf)
        {
            int i, j;
            if ((ReceiveQueue.Count <= 0) || (retBuf.Length < Len))
            {
                return -1;
            }
            else
            {
                try
                {
                    if (Len > ReceiveQueue.Count)
                        j = ReceiveQueue.Count;
                    else
                        j = Len;

                    for (i = 0; i <= (j - 1); i++)
                    {
                        retBuf[i] = (byte)ReceiveQueue.Dequeue();
                        Console.Write("{0,2:X},", retBuf[i]);
                    }
                }
                catch (Exception)
                {
                    return 1;
                }

            }
            return 1;
        }
        public byte GeneralDequeue() { return ((byte)ReceiveQueue.Dequeue()); }
        public int ReceiveQueueLength() { return (ReceiveQueue.Count); }

        private int EnqueueData(Byte[] inBuf, int Length)
        {
            try
            {
                _SerialPortHandle.Read(inBuf, 0, Length);

                for (int i = 0; i < Length; i++)
                    ReceiveQueue.Enqueue(inBuf[i]);

                //Thread.Sleep(20);
            }
            catch (System.TimeoutException)
            {//Time out
                return -1;
            }
            catch (System.ArgumentException)
            {
                return -1;
            }
            catch (System.IO.IOException)
            {//Port number error
                return -1;
            }

            return 1;
        }

        public void Receive()
        {
            int dataToRead = _SerialPortHandle.BytesToRead;
            byte[] seriesOfData = new byte[dataToRead];
            if (dataToRead > 0)
            {
                EnqueueData(seriesOfData, dataToRead);
            }
        }

        //=== Used along with DataReceived Event ===//
        public void ReceiveData(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int i, j;
            byte[] InBuf = new byte[bufferSize];

            try
            {
                j = sp.BytesToRead;
                byte[] tempBuf = new byte[j];
                for (i = 0; i < j; i++)
                {
                    tempBuf[i] = (byte)sp.ReadByte();
                    //Console.Write("{0,2:X},", tempBuf[i]);
                    ReceiveQueue.Enqueue(tempBuf[i]);
                }

                sp.DiscardInBuffer();
            }
            catch (Exception ex)
            {
                //sp.Close();
                //sp.Dispose();
                log.Debug($"ReceiveDataEvent error!\n{ex}");
            }
        }

        public void DataReceivedByEvent(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;   //this is used with Forms-Serialport component inserted
            try
            {
                if (sp.IsOpen)
                {
                    byte[] byteRead = new byte[sp.BytesToRead];
					//it must use ReadLine() to do carriage-return and line-feed instead of using /r/n.
                    string dataValue = sp.ReadLine(); 
                    if (dataValue.Contains("io i"))
                        GlobalData.Arduino_Read_String = dataValue;
                    //log.Debug($"[{sp}] DataReceived: {dataValue}");

                    int recByteCount = byteRead.Count();
                    if (recByteCount > 0)
                        GlobalData.Arduino_recFlag = true;
                    else
                        GlobalData.Arduino_recFlag = false;

                    sp.DiscardInBuffer();                      //Clear buffer of SerialPort component
                }
                else
                {
                    log.Debug($"[{sp}] Arduino serialport is not opened!");
                }
            }
            catch (System.Exception ex)
            {
                sp.Close();
                sp.Dispose();
                log.Debug($"DataReceivedEvent error!\n{ex}");
            }
        }

        public int OpenSerialPort(string port_Name, string port_BR)
        {
            try
            {
                if (_SerialPortHandle.IsOpen == false)
                {
                    _SerialPortHandle.PortName = port_Name;
                    _SerialPortHandle.BaudRate = int.Parse(port_BR);
                    _SerialPortHandle.DataBits = 8;
                    _SerialPortHandle.StopBits = StopBits.One;
                    _SerialPortHandle.Handshake = Handshake.None;
                    _SerialPortHandle.Parity = Parity.None;
                    _SerialPortHandle.ReadTimeout = 2000;
                    _SerialPortHandle.WriteTimeout = 100;
                    _SerialPortHandle.ReadBufferSize = 1024;
                    ReceiveQueue.Clear();
                    _SerialPortHandle.Open();
					
                    log.Debug($"[Mod_RS232] {_SerialPortHandle.PortName} is successfully opened.");
                    Thread.Sleep(20);
                    //_SerialPortHandle.DataReceived += new SerialDataReceivedEventHandler(ReceiveData);
                }
                else
                {
                    //log.Debug($"[Mod_RS232] {_SerialPortHandle.PortName} is not yet opened.");
                    return -3;
                }
            }
            catch (System.IO.IOException)
            {   //Port number error
                return -1;
            }
            catch (System.UnauthorizedAccessException)
            {   //Port is used by another application
                return -2;
            }
            catch (Exception e)
            {
                log.Debug($"[Mod_RS232]\n{e}");
            }

            return 1;
        }

        public int OpenPort_Arduino(string port_Name, string port_BR = "9600")
        {
            try
            {
                if (_SerialPortHandle.IsOpen == false)
                {
                    _SerialPortHandle.PortName = port_Name;
                    _SerialPortHandle.BaudRate = int.Parse(port_BR);
                    _SerialPortHandle.DataBits = 8;
                    _SerialPortHandle.StopBits = StopBits.One;
                    _SerialPortHandle.Handshake = Handshake.None;
                    _SerialPortHandle.Parity = Parity.None;
                    _SerialPortHandle.ReadTimeout = 2000;
                    _SerialPortHandle.WriteTimeout = 100;
                    _SerialPortHandle.ReadBufferSize = 1024;
                    ReceiveQueue.Clear();
                    _SerialPortHandle.Open();

                    log.Debug($"[Mod_RS232] {_SerialPortHandle.PortName} is successfully opened.");
                    //======= ARDUINO receives data by using ReceivedByEvent instead of Thread =======//
                    _SerialPortHandle.DataReceived += new SerialDataReceivedEventHandler(DataReceivedByEvent);
                }
                else
                {
                    log.Debug($"[Mod_RS232] {_SerialPortHandle.PortName} is not yet opened.");
                    return -3;
                }
            }
            catch (System.IO.IOException)
            {   //Port number error
                return -1;
            }
            catch (System.UnauthorizedAccessException)
            {   //Port is used by another application
                return -2;
            }
            catch (Exception e)
            {
                log.Debug($"[Mod_RS232]\n{e}");
            }

            return 1;
        }

        public int OpenPort(string PortNumber, int BaudRate, int ParityBit, int DataLen, int StopBit)
        {
            try
            {
                if (_SerialPortHandle.IsOpen == false)
                {
                    
                    int i = 0;
                    _SerialPortHandle.PortName = PortNumber;
                    _SerialPortHandle.BaudRate = BaudRate;
                    _SerialPortHandle.DataBits = DataLen;
                    _SerialPortHandle.StopBits = (StopBits)(StopBit);
                    _SerialPortHandle.Handshake = (Handshake)(i);
                    _SerialPortHandle.Parity = (Parity)(ParityBit);
                    _SerialPortHandle.ReadTimeout = 100;
                    _SerialPortHandle.WriteTimeout = 100;
                    _SerialPortHandle.ReadBufferSize = 1024;
                    ReceiveQueue.Clear();
                    _SerialPortHandle.Open();
					
                    log.Debug($"[Mod_RS232] {_SerialPortHandle.PortName} is successfully opened.");
                    Thread.Sleep(20);
                    /*
                    if (_SerialPortHandle.IsOpen)
                    {
                        readingThread = new Thread(DataReceiving);
                        readingThread.Start();
                        //readingThread.IsBackground = true;
                    }
                    */
                    //======= RS232 receives data by using Thread instead of ReceivedByEvent =======//
                    //_SerialPortHandle.DataReceived += new SerialDataReceivedEventHandler(ReceiveData);
                }
                else
                {
                    log.Debug($"[Mod_RS232] {_SerialPortHandle.PortName} is not yet opened.");
                    return -3;
                }
            }
            catch (System.IO.IOException)
            {//Port number error
                return -1;
            }
            catch (System.UnauthorizedAccessException)
            {//Port is used by  another APP
                return -2;
            }
            return 1;
        }

        public int ClosePort()
        {
            ReceiveQueue.Clear();
            GlobalData.RS232_receivedRaw = "";
            GlobalData.RS232_receivedAscii = "";
            _SerialPortHandle.Dispose();    //added by YFC
            _SerialPortHandle.Close();
			
            return 1;
        }

        public bool IsOpen()
        {
            if (_SerialPortHandle.IsOpen == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
