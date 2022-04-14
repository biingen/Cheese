using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using log4net;
using ModuleLayer;
using System.Net.Sockets;
using System.Timers;
using System.Net.Mail;
using AForge;
using AForge.Controls;
using AForge.Video.DirectShow;
using AForge.Video;
using jini;
using Universal_Toolkit.Types;

namespace Cheese
{
    public partial class Main : Form
    {
        string TargetFilePath;
        public int FlagComPortStauts; 
        bool playState, pauseState, flagLoopTimes;
        int FlagPause, FlagStop, loopTimes = 0, loopCounter = 0;
        int forLoopCount = 0;
        List<Tuple<int, int>> forLoopIndexList = new List<Tuple<int, int>>();
        static string Cmdsend, Cmdreceive;
        int Device, Resolution;
        public double timeout;
        public static double tmout = 0.0;
        public TimeoutTimer timeOutTimer;
		private static ILog log = GlobalData.Log;        //log4net

        // ----------------------------------------------------------------------------------------------- //
        private FilterInfoCollection videoDevices = null;
        //private VideoCaptureDevice videoSource = null;
        //private VideoCapabilities[] videoCapabilities;
        //static IVideoSource iVideoSource1, iVideoSource2; //VideoCaptureDevice object only is enough for snapshot purpose
        static VideoCaptureDevice videoSource1, videoSource2;
        private static Bitmap bitmap = null;
        //public delegate void dPassBitmapOn(ref Bitmap bmp, string remarkString);
        //public static dPassBitmapOn passBitmapOn;
        private object objLock = new object();
        static int cameraIndex = -1;
        // ----------------------------------------------------------------------------------------------- //
        DataTypeConversion dataConv = new DataTypeConversion();
        static Mod_TCPIP_Client NetworkHandle = new Mod_TCPIP_Client();
        Thread ExecuteCmd_Thread, SerialPort_Receive_Thread, SerialPort_CmdHandling_Thread;
        // ----------------------------------------------------------------------------------------------- //
        public DataGridView tempDataGrid;
        private delegate void dUpdateDataGrid(int x, int y, string data);
        //private delegate void ProcessLoopText(int Cmd, ref int result);
        private delegate void ProcessLoopText(int Cmd, int result);
        public delegate void dUpdateUI(int status);
        public delegate void dUpdateUIBtn(int Btn, int Status);
        public delegate void dUpdateUiString(int camIdx, string data);
        // ---------------------------------- Arduino parameter ---------------------------------- //
        string Read_Arduino_Data = "";
        bool serial_receive = false;
        int GPIO_Read_Data = 0x0000;
        // ----------------------------------------------------------------------------------------------- //
        static int cmdVarStep = 0;

        public Main()
        {
            log.Debug("Main");
            InitializeComponent();
            tempDataGrid = this.dataGridView1;
            FlagComPortStauts = 0;
            this.VerLabel.Text = "Version: 00.00.010";
            playState = false;
            pauseState = false;
            flagLoopTimes = false;
            FlagPause = 0;
            FlagStop = 0;
        }
        // ----------------------------------------------------------------------------------------------- //
        public void Form1UpdateArduinoLedStatus(int status)
        {
            log.Debug("Form1UpdateArduinoLedStatus: " + status);
            if (status == 1)
            {
                this.PIC_Arduino.Image = ImageResource.GleenLed;
            }
            else
            {
                this.PIC_Arduino.Image = ImageResource.BlackLED;
            }
        }
        public void Form1UpdateComportLedStatus(int status)
        {
            log.Debug("Form1UpdateComportLedStatus: " + status);
            if (status == 1)
            {
                this.PIC_ComPortStatus.Image = ImageResource.GleenLed;
            }
            else
            {
                this.PIC_ComPortStatus.Image = ImageResource.BlackLED;
            }
        }
        public void Form1UpdateNetworkLedStatus(int status)
        {
            log.Debug("Form1UpdateNetworkLedStatus: " + status);
            if (status == 1)
            {
                this.PIC_NetworkStatus.Image = ImageResource.GleenLed;
            }
            else
            {
                this.PIC_NetworkStatus.Image = ImageResource.BlackLED;
            }
        }
        public void Form1UpdateFTDILedStatus(int status)
        {
            log.Debug("Form1UpdateFTDILedStatus: " + status);
            if (status == 1)
            {
                this.PIC_FTDI.Image = ImageResource.GleenLed;
            }
            else
            {
                this.PIC_FTDI.Image = ImageResource.BlackLED;
            }
        }

        private void UpdateUiData(int x, int y, string data)
        {
            log.Debug("UpdateUiData: " + x + ", " + y + ", " + data);
            int i;
            if (y == -1)
            {
                //add one row
                this.dataGridView1.Rows.Add();
                i = this.dataGridView1.Rows.Count;
                //Console.WriteLine("Current Rows Count:" + i.ToString());
            }
            else if (y == -2)
            {
                this.dataGridView1.Rows.Clear();
            }
            else if (y == -3)
            {
                this.dataGridView1.Refresh();
            }
            else if (y == -4)
            {
                this.dataGridView1.FirstDisplayedScrollingRowIndex = x;
            }
            else if (y == -5)
            {//unslect target row
                this.dataGridView1.Rows[x].Selected = false;
                //this.dataGridView1.Refresh();
            }
            else if (y == -6)
            {//slect target row
                this.dataGridView1.Rows[x].Selected = true;
            }
            else if (y == -7)
            {
                this.dataGridView1.Refresh();
            }
            else
            {
                this.dataGridView1.Rows[y].Cells[x].Value = data;
            }
            
        }
        
        private void UpdateUIBtnFun(int Btn, int Status)
        {
            log.Debug("UpdateUIBtnFun: " + Btn + ", " + Status);
            switch (Btn)
            {
                case 0:     //Start BTN
                    if (Status == 1)
                    {
                        BTN_StartTest.Enabled = true;
                    }
                    else
                    {
                        BTN_StartTest.Enabled = false;
                    }
                    break;
                case 1:     //Pause BTN
                    if (Status == 1)
                    {
                        BTN_Pause.Enabled = true;
                    }
                    else
                    {
                        BTN_Pause.Enabled = false;
                    }
                    break;
                case 2:     //Stop BTN
                    if (Status == 1)
                    {
                        BTN_Stop.Enabled = true;
                    }
                    else
                    {
                        BTN_Stop.Enabled = false;
                    }
                    break;
                case 3:     //Update status picture box
                    if (Status == 1)
                    {
                        Picbox_CurrentStatus.Image = ImageResource.Testing;
                    }
                    else if (Status == 2)
                    {
                        Picbox_CurrentStatus.Image = ImageResource.pause;
                    }
                    else if (Status == 3)
                    {
                        Picbox_CurrentStatus.Image = ImageResource.Finish;
                    }
                    else
                    {
                        Picbox_CurrentStatus.Image = null;
                    }
                    //this.Picbox_CurrentStatus.Refresh();
                    break;
                case 4:     //Camera BTN
                    if (Status == 1)
                    {
                        button_Camera.Enabled = true;
                    }
                    else
                    {
                        button_Camera.Enabled = false;
                    }
                    break;
                case 5:     //Snapshot BTN
                    if (Status == 1)
                    {
                        button_Snapshot.Enabled = true;
                    }
                    else
                    {
                        button_Snapshot.Enabled = false;
                    }
                    break;
                case 6:     //Schedule BTN
                    if (Status == 1)
                    {
                        button_Schedule.Enabled = true;
                    }
                    else
                    {
                        button_Schedule.Enabled = false;
                    }
                    break;
                case 7:     //AC_On BTN
                    if (Status == 1)
                    {
                        button_AcOn.Enabled = true;
                    }
                    else
                    {
                        button_AcOn.Enabled = false;
                    }
                    break;
                case 8:     //AC_Off BTN
                    if (Status == 1)
                    {
                        button_AcOff.Enabled = true;
                    }
                    else
                    {
                        button_AcOff.Enabled = false;
                    }
                    break;
                case 9:     //Update cboxCameraList enable state
                    if (Status == 1)
                    {
                        cboxCameraList.Enabled = true;
                    }
                    else
                    {
                        cboxCameraList.Enabled = false;
                    }
                    break;
                case 10:    //PLAY/STOP one-button
                    if (Status == 0)
                    {
                        BTN_StartTest.Image = ImageResource.stop;
                    }
                    else if (Status == 1)
                    {
                        BTN_StartTest.Image = ImageResource.play_button;
                    }
                    break;
            }
            //this.Refresh();
        }
        //private void UpdateLoopTxt(int Cmd, ref int result)
        private void UpdateLoopTxt(int Cmd, int valueOfLoop)
        {
            log.Debug("UpdateLoopTxt: " + Cmd + ", " + valueOfLoop);
            if (Cmd == 0)
            {
                this.Txt_LoopTimes.Enabled = false;
                this.Txt_LoopCounter.Enabled = false;
            }
            else if (Cmd == 1)
            {
                this.Txt_LoopTimes.Enabled = true;
                this.Txt_LoopCounter.Enabled = true;
            }
            else if (Cmd == 2)  //read loopCounter value from textBox
            {
                //result = Convert.ToInt32(this.Txt_LoopTimes.Text);
                loopTimes = Convert.ToInt32(this.Txt_LoopTimes.Text);
                loopCounter = loopTimes;
            }
            else if (Cmd == 3)  //write loopCounter value to textBox
            {
                this.Txt_LoopCounter.Text = valueOfLoop.ToString();
            }
            else if (Cmd == 4)  //uncheck the chk_loopTimes checkBox
            {
                //this.Txt_LoopCounter.Visible = false;
                loopCounter = valueOfLoop;
            }
            else if (Cmd == 5)
                this.chkBox_LoopTimes.Enabled = false;
            else if (Cmd == 6)
                this.chkBox_LoopTimes.Enabled = true;

        }
		
        private void UpdateUiString(int camIdx, string dataString)  //used to display current camera resolution
        {
            log.Debug("UpdateUiString: " + camIdx + ", " + dataString);
            if (camIdx == 1)
                textBox_cam1Res.Text = dataString;
            else if (camIdx == 2)
                textBox_cam2Res.Text = dataString;
        }

        private void UpdateDataGrid()
        {
            string contentLine; //headerLine
            string[] tempStr;
            string[] cmdStr;
            char[] CRCStr = new char[5];
            int i = 0, j = 0, y = 0;
            int colIndex, colBoundary = 9, forStringCount = 0;
            byte HighHalfByte, LowHalfByte;
            byte[] tempData = new byte[100];
            ushort CRCResult;
            //DataTypeConversion dataConv = new DataTypeConversion();
            dUpdateDataGrid updateDataGrid = new dUpdateDataGrid(UpdateUiData);
            System.IO.StreamReader rFile = new System.IO.StreamReader(@TargetFilePath);
            updateDataGrid.Invoke(0, -2, "");   //Clear datagrid

            if (forLoopIndexList.Count > 0) //Clear count of FOR Loop Items
                forLoopIndexList.Clear();

            rFile.ReadLine();      //just read the header line and do nothing with it
            while ((contentLine = rFile.ReadLine()) != null)
            {
                try
                {
                    //Console.WriteLine(contentLine + "\n");     //print first row of grid content
                    tempStr = contentLine.Split(',');
                    updateDataGrid.Invoke(0, -1, "");   //Add one line

                    for (colIndex = 0; colIndex <= colBoundary; colIndex++)
                    {
                        updateDataGrid.Invoke(colIndex, y, tempStr[colIndex]);
                        if (tempStr.Length >= 1)
                        {
                            //if (colIndex == 6 && tempStr[colIndex] != null || tempStr[colIndex] != "")
                            if (colIndex == 6 && tempStr[colIndex] != "")
                            {
                                //1.W/R field
                                //updateDataGrid.Invoke(1, y, tempStr[colIndex++]);
                                //2.Cmd string field
                                //updateDataGrid.Invoke(2, y, tempStr[colIndex++]);
                                //CRC field - convert string to byte array
                                cmdStr = tempStr[colIndex].Split(' ');

                                if (tempStr[4] == "XOR8" && cmdStr.Length >= 2)
                                {
                                    string CmdLine = tempStr[colIndex];
                                    //calculate XOR8 CRC
                                    string[] CmdStringArray = CmdLine.Split(' ');
                                    byte[] CmdBytes = new byte[CmdStringArray.Count() + 1];     //Plus 1 is reserved for checksum Byte
                                    var tstStr = dataConv.XOR8_BytesWithChksum(CmdLine, CmdBytes, CmdBytes.Length);
                                    tstStr = CmdLine + " " + tstStr;

                                    updateDataGrid.Invoke(13, y, tstStr);    //Auto fill CRC Field column after opening schedule file
                                }
                                else if (tempStr[4] == "GENERAL" && cmdStr.Length >= 2)
                                {
                                    j = 0;
                                    for (i = 0; i <= (cmdStr.Length - 1); i++)
                                    {
                                        HighHalfByte = (byte)cmdStr[i][0];
                                        LowHalfByte = (byte)cmdStr[i][1];
                                        tempData[i] = (byte)((dataConv.AsciiToByte(HighHalfByte) * 16) + dataConv.AsciiToByte(LowHalfByte));
                                        //Console.Write("{0,2:X}", tempData[i]);
                                        j++;
                                    }
                                    //Console.Write("\n");
                                    CRCResult = dataConv.CalculateCRC(j, tempData);
                                    //Console.Write("{0,4:X}\n", CRCResult);

                                    CRCStr[0] = (char)dataConv.BytetoAscii((byte)((CRCResult & 0x00F0) >> 4));
                                    CRCStr[1] = (char)dataConv.BytetoAscii((byte)(CRCResult & 0x000F));
                                    CRCStr[2] = (char)0x20;
                                    CRCStr[3] = (char)dataConv.BytetoAscii((byte)((CRCResult & 0xF000) >> 12));
                                    CRCStr[4] = (char)dataConv.BytetoAscii((byte)((CRCResult & 0x0F00) >> 8));

                                    updateDataGrid.Invoke(13, y, new string(CRCStr));    //Auto fill CRC Field column after opening schedule file
                                    /*  previous SerialPortTest judgement
                                    if (tempStr.Length >= 5)
                                    {
                                        updateDataGrid.Invoke(9, y, tempStr[4]);
                                    }*/
                                }
                            }

                            // Record pair indices of FOR Loop start and end
                            if (colIndex == 0 && tempStr[0] == "_FOR")
                            {
                                forLoopIndexList.Add(new Tuple<int, int>(forStringCount, y));
                                forStringCount++;
                            }
                        }
                        
                    }   //end of For loop
                    
                    y++;
                }
                catch (Exception)
                {
                    MessageBox.Show("Cmd file has illegal field..");
                    break;
                }
            }   //end of While loop

            if (forLoopIndexList.Count % 2 != 0)
            {
                MessageBox.Show("For Loop must be specified pairs of start and end index!!!");
            }
            else
            {
                rFile.Close();
                updateDataGrid.Invoke(0, -3, "");   //Fresh datagrid
                //forStringCount = 0;
            }
        }
        // ------------------------------------------------------------------------------------------------ //
        private void ExecuteCmd()
        {
            dUpdateUIBtn UpdateUIBtn = new dUpdateUIBtn(UpdateUIBtnFun);
            dUpdateDataGrid WriteDataGrid = new dUpdateDataGrid(UpdateUiData);
            dUpdateDataGrid updateDataGrid = new dUpdateDataGrid(UpdateUiData);
            ProcessLoopText LoopText = new ProcessLoopText(UpdateLoopTxt);
            //DataTypeConversion ProStr = new DataTypeConversion();
            List<MonitorControl> monControlSets = new List<MonitorControl>();
            Setting form2 = new Setting();
            string resultLine = "";
            string cmdString = "";
            string[] CmdStringArray = new string[128];
            string[] tempStr = new string[100];
            byte[] Cmdbuf = new byte[100];
            byte[] retBuf = new byte[100];
            byte[] finBuf = new byte[100];
            ushort arduino_input_status;
            int delayTime, RowCount, ExeIndex = 0, forLoopItem = 0, cmdVar_int = -1;
            
            RowCount = this.dataGridView1.Rows.Count;
            if (RowCount <= 1) 
            {
                MessageBox.Show("No Row Data");
                //Invoke(UpdateUIBtn, 0, 1);  //this.BTN_StartTest.Enabled = true;
                //UpdateLoopTxt(1, ref loopCounter);//Enable Loop Text
                //Invoke(LoopText, 1, loopCounter);
                //Invoke(LoopText, 3, loopCounter);
            }
            else if (GlobalData.m_SerialPort == null)
            {
                MessageBox.Show("Check Com-Port Status First");
            }
            else
            {
                //Invoke(LoopText, 0, loopCounter);
                //UpdateLoopTxt(0, ref loopCounter);  //disable Loop Test
                //UpdateLoopTxt(2, ref loopCounter);  //get Loop counter
                //Invoke(LoopText, 2, loopCounter);
                Invoke(LoopText, 0, 0);
                Invoke(LoopText, 2, 0);
                Invoke(LoopText, 5, 0);
                if (loopCounter < 0)
                    loopCounter = 0;

                while (loopCounter > 0 && playState)    //(loopCounter > 0 && FlagStop == 0)
                {
                    GlobalData.caption_Num = 0;
                    Invoke(UpdateUIBtn, 3, 1);  //display testing
                    // ============= Clear old data ============= //
                    for (ExeIndex = 0; ExeIndex < (RowCount-1); ExeIndex++)
                    {
                        dataGridView1.Rows[ExeIndex].Cells[10].Value = "";
                        dataGridView1.Rows[ExeIndex].Cells[11].Value = "";
                        dataGridView1.Rows[ExeIndex].Cells[12].Value = "";
                        dataGridView1.Rows[ExeIndex].Cells[13].Value = "";
                        Invoke(updateDataGrid, ExeIndex, -5, "");   //unselect current row
                    }
                    // ============= Start to test ============= //
                    //for (ExeIndex = this.dataGridView1.CurrentRow.Index; ExeIndex < RowCount; ExeIndex++)
                    for (ExeIndex = 0; ExeIndex < (RowCount-1); ExeIndex++)
                    {
                        string columns_command = dataGridView1.Rows[ExeIndex].Cells[0].Value == null ? string.Empty : dataGridView1.Rows[ExeIndex].Cells[0].Value.ToString().Trim();
                        string columns_times = dataGridView1.Rows[ExeIndex].Cells[1].Value == null ? string.Empty:dataGridView1.Rows[ExeIndex].Cells[1].Value.ToString().Trim();
                        string columns_interval = dataGridView1.Rows[ExeIndex].Cells[2].Value == null ? string.Empty : dataGridView1.Rows[ExeIndex].Cells[2].Value.ToString().Trim();
                        string columns_comport = dataGridView1.Rows[ExeIndex].Cells[3].Value == null ? string.Empty : dataGridView1.Rows[ExeIndex].Cells[3].Value.ToString().Trim();
                        string columns_function = dataGridView1.Rows[ExeIndex].Cells[4].Value == null ? string.Empty : dataGridView1.Rows[ExeIndex].Cells[4].Value.ToString().Trim();
                        string columns_subFunction = dataGridView1.Rows[ExeIndex].Cells[5].Value == null ? string.Empty : dataGridView1.Rows[ExeIndex].Cells[5].Value.ToString().Trim();
                        string columns_cmdLine = dataGridView1.Rows[ExeIndex].Cells[6].Value == null ? string.Empty : dataGridView1.Rows[ExeIndex].Cells[6].Value.ToString().Trim();
                        string columns_switch = dataGridView1.Rows[ExeIndex].Cells[7].Value == null ? string.Empty : dataGridView1.Rows[ExeIndex].Cells[7].Value.ToString().Trim();
                        string columns_wait = dataGridView1.Rows[ExeIndex].Cells[8].Value == null ? string.Empty : dataGridView1.Rows[ExeIndex].Cells[8].Value.ToString().Trim();
                        string columns_remark = dataGridView1.Rows[ExeIndex].Cells[9].Value == null ? string.Empty : dataGridView1.Rows[ExeIndex].Cells[9].Value.ToString().Trim();

                        if (ExeIndex >= 3)
                        {
                            Invoke(updateDataGrid, (ExeIndex - 2), -4, "");
                        }
                        else
                        {
                            Invoke(updateDataGrid, 0, -4, "");
                        }
                        
                        if (ExeIndex >= 1)
                        {
                            Invoke(updateDataGrid, (ExeIndex - 1), -5, "");      //unselect previous row
                        }
                        Invoke(updateDataGrid, ExeIndex, -6, "");
                        this.dataGridView1.Rows[ExeIndex].Cells[10].Value = "";
                        this.dataGridView1.Rows[ExeIndex].Cells[11].Value = "";
                        this.dataGridView1.Rows[ExeIndex].Cells[12].Value = "";
                        this.dataGridView1.Rows[ExeIndex].Cells[13].Value = "";

                        #region -- Schedule for Snapshot Command --
                        if ((columns_command == "_shot") || (columns_command == "PHOTO") || (columns_command == "photo"))
                        {
                            int camSelectMode = 0;
                            GlobalData.caption_Num++;
                            //byte[] decodeFromStr = new byte[2];
                            //decodeFromStr = Encoding.ASCII.GetBytes(columns_subFunction);

                            if (columns_subFunction == "all" || columns_subFunction == "")
                                camSelectMode = -1;
                            else if (columns_subFunction != "")// && decodeFromStr[0] > 0x30 && decodeFromStr[0] < 0x39)
                                camSelectMode = Convert.ToInt32(columns_subFunction);

                            if (videoDevices.Count >= 1)
                            {
                                Snapshot(camSelectMode, columns_wait, columns_remark);
                            }
                            else
                            {
                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                            }

                        }
                        #endregion
                        #region -- Schedule for Robot Command --
                        else if ((columns_command == "_tcpip") || (columns_command == "ROBOT") || (columns_command == "robot"))
                        {
                            if (NetworkHandle.IsConnected())
                            {
                                Cmdsend = (string)this.dataGridView1.Rows[ExeIndex].Cells[6].Value;
                                NetworkHandle.Send(Cmdsend);
                                
                                Invoke(WriteDataGrid, 10, ExeIndex, "");
                                Thread.Sleep(100);
                                timeOutTimer = new TimeoutTimer(timeout);
                                timeOutTimer.StartTimeoutTimer();
                                Invoke(WriteDataGrid, 10, ExeIndex, Cmdreceive);
                                Thread.Sleep(Convert.ToInt32(this.dataGridView1.Rows[ExeIndex].Cells[8].Value));
                                Cmdreceive = "";
                            }
                        }
                        #endregion
                        #region -- Schedule for ASCII W/R --
                        else if ((columns_command == "_ASCII") || (columns_command == "_ascii"))
                        {
                            GlobalData.m_SerialPort.WriteDataOut(columns_cmdLine, columns_cmdLine.Length);
                            Thread.Sleep(50);
                            Invoke(WriteDataGrid, 10, ExeIndex, GlobalData.RS232_receivedText);
                            //GetSerialData(GlobalData.m_SerialPort_A);
                            /*
                            int numOfBytes = GlobalData.m_SerialPort_A.GetRxBytes();
                            byte[] recBytes = new byte[numOfBytes];
                            for (int i = 0; i < numOfBytes; i++)
                            {
                                recBytes[i] = GlobalData.m_SerialPort_A.GeneralDequeue();
                            }*/
                            Thread.Sleep(Convert.ToInt32(this.dataGridView1.Rows[ExeIndex].Cells[8].Value));
                        }
                        #endregion
                        #region -- Schedule for HEX W/R --
                        else if ((columns_command == "_HEX") || (columns_command == "_HEX_R"))
                        {
                            // ----------------------- Process data string from csv file ----------------------- //
                            if (columns_command == "_HEX")
                            {
                                //caluate CRC field
                                if (columns_function == "XOR8")
                                {
                                    cmdString = columns_cmdLine;
                                    byte[] cmdBytes = new byte[CmdStringArray.Count() + 1];     //Plus 1 is reserved for checksum Byte
                                    var tstStr = dataConv.XOR8_BytesWithChksum(columns_cmdLine, cmdBytes, cmdBytes.Length);
                                    GlobalData.m_SerialPort.WriteDataOut(cmdBytes, cmdBytes.Length);
                                }
                                else if (columns_function == "GENERAL" || columns_function == "BENQ")
                                {
                                    byte[] cmdBytes = new byte[columns_cmdLine.Count()];
                                    cmdBytes = dataConv.StrToByte(columns_cmdLine);
                                    GlobalData.m_SerialPort.WriteDataOut(cmdBytes, cmdBytes.Length);
                                    Task.Delay(2000).Wait();     //delay long enough by a Task to receive serialport data
                                    //Thread.Sleep(2000) is not a suggested way;
                                    Invoke(WriteDataGrid, 10, ExeIndex, GlobalData.Measure_Backlight);
                                    Invoke(WriteDataGrid, 11, ExeIndex, GlobalData.Measure_Thermal);
                                }
                            }

                            if (columns_command == "_HEX_R")
                            {
                                int rxLength = GlobalData.m_SerialPort.ReceiveQueueLength();

                                if (columns_function == "XOR8")
                                {
                                    for (int index = 0; index < rxLength; index++)
                                    {
                                        GlobalData.returnBytes[index] = GlobalData.m_SerialPort.GeneralDequeue();
                                        resultLine += GlobalData.returnBytes[index].ToString("X2").PadLeft(2, '0');
                                        if (index != (rxLength - 1))
                                            resultLine += ' ';
                                    }
                                }
                                else if (columns_function == "GENERAL")
                                {
                                    for (int index = 0; index < rxLength; index++)
                                    {
                                        resultLine += (char)dataConv.BytetoAscii((byte)((GlobalData.returnBytes[index] >> 4) & 0x0F));
                                        resultLine += (char)dataConv.BytetoAscii((byte)(GlobalData.returnBytes[index] & 0x0F));
                                        if (index != (rxLength - 1))
                                            resultLine += ' ';
                                    }
                                }

                                Invoke(WriteDataGrid, 10, ExeIndex, resultLine);
                            }
                            
                            //Delay Time
                            if (this.dataGridView1.Rows[ExeIndex].Cells[8].Value != null)
                            {
                                delayTime = Convert.ToInt32(columns_wait);
                            }
                            else
                            {
                                delayTime = 1000;
                            }

                            #region -- Send cmd out via RS232 port (original SerialPortTest design) --
                            /*
                            //SerialPortHandle.WriteDataOut(Cmdbuf, j);

                            //wait data return
                            j = 0;
                            while (SerialPortHandle.ReceivedBufferLength() < 2)
                            {

                                Thread.Sleep(1);
                                j++;
                                if (j >= 500)
                                {
                                    break;
                                }
                            }

                            if (j >= 500)
                            {//time out
                                Invoke(WriteDataGrid, 10, ExeIndex, "Time out");
                                Invoke(WriteDataGrid, 11, ExeIndex, "Time out");
                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                Invoke(WriteDataGrid, 13, ExeIndex, "Fail");
                            }
                            else
                            {//process return data
                                SerialPortHandle.SpecificDequeue(2, ref retBuf);
                                ResultLine = "";
                                ResultLine += (char)ProStr.BytetoASCII((byte)((retBuf[0] >> 4) & 0x0F));
                                ResultLine += (char)ProStr.BytetoASCII((byte)(retBuf[0] & 0x0F));
                                ResultLine += ' ';
                                ResultLine += (char)ProStr.BytetoASCII((byte)((retBuf[1] >> 4) & 0x0F));
                                ResultLine += (char)ProStr.BytetoASCII((byte)(retBuf[1] & 0x0F));
                                ResultLine += ' ';
                                finBuf[0] = retBuf[0];
                                finBuf[1] = retBuf[1];
                                if ((retBuf[1] == 0x83) || (retBuf[1] == 0x86) || (retBuf[1] == 0x90))
                                {
                                    while (SerialPortHandle.ReceivedBufferLength() < (retDataLen - 2))
                                    {

                                        Thread.Sleep(1);
                                        j++;
                                        if (j >= 500)
                                        {
                                            break;
                                        }
                                    }
                                    SerialPortHandle.SpecificDequeue(3, ref retBuf);

                                    for (i = 0; i <= 2; i++)
                                    {
                                        ResultLine += (char)ProStr.BytetoASCII((byte)((retBuf[i] >> 4) & 0x0F));
                                        ResultLine += (char)ProStr.BytetoASCII((byte)(retBuf[i] & 0x0F));
                                        ResultLine += ' ';
                                    }
                                    Invoke(WriteDataGrid, 10, ExeIndex, ResultLine);
                                    Invoke(WriteDataGrid, 11, ExeIndex, "NACK");
                                    Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                    Invoke(WriteDataGrid, 13, ExeIndex, "Fail");
                                }
                                else
                                {
                                    while (SerialPortHandle.ReceivedBufferLength() < (retDataLen - 2))
                                    {

                                        Thread.Sleep(1);
                                        j++;
                                        if (j >= 500)
                                        {
                                            break;
                                        }
                                    }

                                    SerialPortHandle.SpecificDequeue((retDataLen - 2), ref retBuf);

                                    for (i = 0; i <= (retDataLen - 3); i++)
                                    {
                                        finBuf[i + 2] = retBuf[i];
                                        ResultLine += (char)ProStr.BytetoASCII((byte)((retBuf[i] >> 4) & 0x0F));
                                        ResultLine += (char)ProStr.BytetoASCII((byte)(retBuf[i] & 0x0F));
                                        ResultLine += ' ';
                                    }
                                    Invoke(WriteDataGride, 10, ExeIndex, ResultLine);

                                    retCRC = (ushort)((finBuf[retDataLen - 1] * 256) + finBuf[retDataLen - 2]);
                                    us_data = ProcessStr.CalculateCRC((retDataLen - 2), finBuf);

                                    if (retCRC != us_data)
                                    {   //CRC fail
                                        Invoke(WriteDataGrid, 11, ExeIndex, "CRC Fail");
                                        Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                        Invoke(WriteDataGrid, 13, ExeIndex, "Fail");
                                    }
                                    else
                                    {
                                        //output result string
                                        us_data = (ushort)((finBuf[retDataLen - 4] * 256) + finBuf[retDataLen - 3]);
                                        ResultLine = String.Format("0x{0:X}", us_data, us_data);
                                        Invoke(WriteDataGride, 11, ExeIndex, ResultLine);
                                        ResultLine = String.Format("{0}", us_data);
                                        Invoke(WriteDataGride, 12, ExeIndex, ResultLine);

                                        if (CmdType == "W")
                                        {
                                            if ((Cmdbuf[4] == finBuf[4]) && (Cmdbuf[5] == finBuf[5]))
                                            {
                                                Invoke(WriteDataGrid, 13, ExeIndex, "Pass");
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 13, ExeIndex, "Pass");
                                            }
                                        }
                                        else if (CmdType == "R")
                                        {
                                            

                                            CmdLine = (string)this.dataGridView1.Rows[ExeIndex].Cells[9].Value;
                                            if ((CmdLine != null) && (CmdLine.Length >= 1))
                                            {
                                                CmdStringArray = CmdLine.Split('/');
                                                if (CmdStringArray.Length >= 1)
                                                {
                                                    j = 0;
                                                    for (i = 0; i <= (CmdStringArray.Length - 1); i++)
                                                    {
                                                        tempStr = CmdStringArray[i].Split(':');
                                                        retCRC = Convert.ToUInt16(tempStr[0]);
                                                        if (retCRC == us_data)
                                                        {
                                                            Invoke(WriteDataGrid, 13, ExeIndex, tempStr[1]);
                                                            j = 1;
                                                            break;
                                                        }
                                                    }
                                                    if (j == 0)
                                                    {
                                                        Invoke(WriteDataGrid, 13, ExeIndex, "Unknow");
                                                    }
                                                }
                                            }
                                        }

                                    }   //else (return CRC = us_data)

                                }   //else (while loop)

                            }   //else (process return data)
                            */
                            #endregion

                            Invoke(updateDataGrid, 0, -3, " ");   //refresh datagrid
                            Thread.Sleep(delayTime);
                        }
                        #endregion
                        #region -- GPIO_INPUT_OUTPUT --
                        else if (columns_command == "_Arduino_Input")
                        {
                            delayTime = Convert.ToInt32(columns_wait);
                            Arduino_Get_GPIO_Input(ref GPIO_Read_Data, delayTime);
                        }
                        else if (columns_command == "_Arduino_Output")
                        {
                            string GPIO_string = columns_cmdLine;
                            byte GPIO_B = 0x00; // = Convert.ToByte(GPIO_string, 2);
                            //if ((GPIO_B & 0x02) == 0x00)
                            if (GPIO_string == "OFF" || GPIO_string == "Off" || GPIO_string == "off")
                            {
                                GlobalData.Arduino_relay_status = false;
                                GPIO_B = 0x00;
                            }
                            else if (GPIO_string == "ON" || GPIO_string == "On" || GPIO_string == "on")
                            {
                                GlobalData.Arduino_relay_status = true;
                                GPIO_B = 0x02;
                            }
                            Arduino_Set_GPIO_Output(GPIO_B, 100);

                            delayTime = Convert.ToInt32(columns_wait);
                            Thread.Sleep(delayTime);

                            Arduino_Get_GPIO_Input(ref GPIO_Read_Data, delayTime);
                        }
                        else if (columns_command == "_Arduino_Command")
                        {
                            if (columns_cmdLine != "")
                            {
                                delayTime = Convert.ToInt32(columns_wait);
                                Arduino_Set_Value(columns_cmdLine, delayTime);
                            }
                            else
                                MessageBox.Show("Please check the Arduino command.", "Arduino command Error!");
                        }
                        #endregion
                        #region -- Schedule for Normal I/O --
                        else if (columns_command == "_Pin")
                        {

                        }
                        #endregion
                        #region -- Schedule for Arduino I/O --
                        else if (columns_command == "_Arduino_Pin"
                            && columns_comport.Length >=6 && columns_comport.Substring(0,3) == "_P0")
                        {
                            Arduino_Get_GPIO_Input(ref GPIO_Read_Data, 300);
                            
                            arduino_input_status = GlobalData.Arduino_IO_INPUT_status;
                            switch (columns_comport.Substring(3, 1))
                            {
                                #region -- No.1 / P02 --
                                case "2":
                                    // --- pin_0: expect getting Low value when monitor turns On --- //
                                    if (columns_comport.Substring(5, 1) == "0" && (GlobalData.Arduino_IO_INPUT_value & 0x01U) == 0)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino2_0_COUNT++;
                                            //label_Command.Text = "IO CMD_ACCUMULATE";
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;
                                            
                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                //bitmap = videoSourcePlayer1.GetCurrentVideoFrame();
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD();
                                        }
                                    }
                                    // --- pin 1: expect getting High value when monitor turns off --- //
                                    else if (columns_comport.Substring(5, 1) == "1" && (GlobalData.Arduino_IO_INPUT_value & 0x01U) == 1)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino2_1_COUNT++;
                                            //label_Command.Text = "IO CMD_ACCUMULATE";
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;
                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if(columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD(columns_cmdLine, cameraIdx, columns_wait);
                                        }
                                    }
                                    else
                                    {
                                        //SysDelay = 0;
                                    }

                                    if ((arduino_input_status & 0x01U) == 1)
                                        Invoke(WriteDataGrid, 10, ExeIndex, "Module-01 fail!");

                                    //Invoke(WriteDataGrid, 11, ExeIndex, GlobalData.Arduino_IO_INPUT_value.ToString("X2"));
                                    log.Info($"[Input Table Value] 0x{GlobalData.Arduino_IO_INPUT_value.ToString("X2")}");
                                    break;
                                #endregion

                                #region -- No.2 / P03 --
                                case "3":
                                    if (columns_comport.Substring(5, 1) == "0" && ((GlobalData.Arduino_IO_INPUT_value >> 1) & 0x01U) == 0)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino3_0_COUNT++;
                                            //label_Command.Text = "IO CMD_ACCUMULATE";
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;

                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD();
                                        }
                                    }
                                    else if (columns_comport.Substring(5, 1) == "1" && ((GlobalData.Arduino_IO_INPUT_value >> 1) & 0x01U) == 1)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino3_1_COUNT++;
                                            //label_Command.Text = "IO CMD_ACCUMULATE";
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;
                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD(columns_cmdLine, cameraIdx, columns_wait);
                                        }
                                    }
                                    else
                                    {
                                        //SysDelay = 0;
                                    }
                                    
                                    if ((arduino_input_status >> 1 & 0x01U) == 1)
                                        Invoke(WriteDataGrid, 10, ExeIndex, "Module-02 fail!");

                                    //Invoke(WriteDataGrid, 11, ExeIndex, GlobalData.Arduino_IO_INPUT_value.ToString("X2"));
                                    log.Info($"[Input Table Value] 0x{GlobalData.Arduino_IO_INPUT_value.ToString("X2")}");
                                    break;
                                #endregion

                                #region -- No.3 / P04 --
                                case "4":
                                    if (columns_comport.Substring(5, 1) == "0" && ((GlobalData.Arduino_IO_INPUT_value >> 2) & 0x01U) == 0)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino4_0_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;

                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD();
                                        }
                                    }
                                    // --- pin 1: expect getting High value when monitor turns off --- //
                                    else if (columns_comport.Substring(5, 1) == "1" && ((GlobalData.Arduino_IO_INPUT_value >> 2) & 0x01U) == 1)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino4_1_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;
                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD(columns_cmdLine, cameraIdx, columns_wait);
                                        }
                                    }
                                    else
                                    {
                                        //SysDelay = 0;
                                    }

                                    if ((arduino_input_status >> 2 & 0x01U) == 1)
                                        Invoke(WriteDataGrid, 10, ExeIndex, "Module-03 fail!");

                                    //Invoke(WriteDataGrid, 11, ExeIndex, GlobalData.Arduino_IO_INPUT_value.ToString("X2"));
                                    log.Info($"[Input Table Value] 0x{GlobalData.Arduino_IO_INPUT_value.ToString("X2")}");
                                    break;
                                #endregion

                                #region -- No.4 / P05 --
                                case "5":
                                    if (columns_comport.Substring(5, 1) == "0" && ((GlobalData.Arduino_IO_INPUT_value >> 3) & 0x01U) == 0)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino5_0_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;

                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD();
                                        }
                                    }
                                    else if (columns_comport.Substring(5, 1) == "1" && ((GlobalData.Arduino_IO_INPUT_value >> 3) & 0x01U) == 1)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino5_1_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;
                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD(columns_cmdLine, cameraIdx, columns_wait);
                                        }
                                    }
                                    else
                                    {
                                        //SysDelay = 0;
                                    }

                                    if ((arduino_input_status >> 3 & 0x01U) == 1)
                                        Invoke(WriteDataGrid, 10, ExeIndex, "Module-04 fail!");

                                    //Invoke(WriteDataGrid, 11, ExeIndex, GlobalData.Arduino_IO_INPUT_value.ToString("X2"));
                                    log.Info($"[Input Table Value] 0x{GlobalData.Arduino_IO_INPUT_value.ToString("X2")}");
                                    break;
                                #endregion

                                #region -- No.5 / P06 --
                                case "6":
                                    if (columns_comport.Substring(5, 1) == "0" && ((GlobalData.Arduino_IO_INPUT_value >> 4) & 0x01U) == 0)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino6_0_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;

                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD();
                                        }
                                    }
                                    else if (columns_comport.Substring(5, 1) == "1" && ((GlobalData.Arduino_IO_INPUT_value >> 4) & 0x01U) == 1)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino6_1_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;
                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD(columns_cmdLine, cameraIdx, columns_wait);
                                        }
                                    }
                                    else
                                    {
                                        //SysDelay = 0;
                                    }

                                    if ((arduino_input_status >> 4 & 0x01U) == 1)
                                        Invoke(WriteDataGrid, 10, ExeIndex, "Module-05 fail!");

                                    //Invoke(WriteDataGrid, 11, ExeIndex, GlobalData.Arduino_IO_INPUT_value.ToString("X2"));
                                    log.Info($"[Input Table Value] 0x{GlobalData.Arduino_IO_INPUT_value.ToString("X2")}");
                                    break;
                                #endregion

                                #region -- No.6 / P07 --
                                case "7":
                                    if (columns_comport.Substring(5, 1) == "0" && ((GlobalData.Arduino_IO_INPUT_value >> 5) & 0x01U) == 0)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino7_0_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;

                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD();
                                        }
                                    }
                                    else if (columns_comport.Substring(5, 1) == "1" && ((GlobalData.Arduino_IO_INPUT_value >> 5) & 0x01U) == 1)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino7_1_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;
                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD(columns_cmdLine, cameraIdx, columns_wait);
                                        }
                                    }
                                    else
                                    {
                                        //SysDelay = 0;
                                    }

                                    if ((arduino_input_status >> 5 & 0x01U) == 1)
                                        Invoke(WriteDataGrid, 10, ExeIndex, "Module-06 fail!");

                                    //Invoke(WriteDataGrid, 11, ExeIndex, GlobalData.Arduino_IO_INPUT_value.ToString("X2"));
                                    log.Info($"[Input Table Value] 0x{GlobalData.Arduino_IO_INPUT_value.ToString("X2")}");
                                    break;
                                #endregion

                                #region -- No.7 / P08 --
                                case "8":
                                    if (columns_comport.Substring(5, 1) == "0" && ((GlobalData.Arduino_IO_INPUT_value >> 6) & 0x01U) == 0)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino8_0_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;

                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD();
                                        }
                                    }
                                    else if (columns_comport.Substring(5, 1) == "1" && ((GlobalData.Arduino_IO_INPUT_value >> 6) & 0x01U) == 1)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino8_1_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;
                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD(columns_cmdLine, cameraIdx, columns_wait);
                                        }
                                    }
                                    else
                                    {
                                        //SysDelay = 0;
                                    }

                                    if ((arduino_input_status >> 6 & 0x01U) == 1)
                                        Invoke(WriteDataGrid, 10, ExeIndex, "Module-07 fail!");

                                    //Invoke(WriteDataGrid, 11, ExeIndex, GlobalData.Arduino_IO_INPUT_value.ToString("X2"));
                                    log.Info($"[Input Table Value] 0x{GlobalData.Arduino_IO_INPUT_value.ToString("X2")}");
                                    break;
                                #endregion

                                #region -- No.8 / P09 --
                                case "9":
                                    if (columns_comport.Substring(5, 1) == "0" && ((GlobalData.Arduino_IO_INPUT_value >> 7) & 0x01U) == 0)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino9_0_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;

                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD();
                                        }
                                    }
                                    else if (columns_comport.Substring(5, 1) == "1" && ((GlobalData.Arduino_IO_INPUT_value >> 7) & 0x01U) == 1)
                                    {
                                        if (columns_cmdLine == "_accumulate")
                                        {
                                            GlobalData.IO_Arduino9_1_COUNT++;
                                        }
                                        else if (columns_cmdLine == "_shot")
                                        {
                                            int cameraIdx = 0;
                                            if (columns_subFunction == "all" || columns_subFunction == "")
                                                cameraIdx = -1;
                                            else if (columns_subFunction != "")
                                                cameraIdx = Convert.ToInt32(columns_subFunction);

                                            if (videoDevices.Count >= 1)
                                            {
                                                Snapshot(cameraIdx, columns_wait, columns_remark);
                                            }
                                            else
                                            {
                                                Invoke(WriteDataGrid, 10, ExeIndex, "Can't find Camera");
                                                Invoke(WriteDataGrid, 11, ExeIndex, "Fail");
                                                Invoke(WriteDataGrid, 12, ExeIndex, "Fail");
                                            }
                                        }
                                        else
                                        {
                                            //IO_CMD(columns_cmdLine, cameraIdx, columns_wait);
                                        }
                                    }
                                    else
                                    {
                                        //SysDelay = 0;
                                    }

                                    if ((arduino_input_status >> 7 & 0x80U) == 1)
                                        Invoke(WriteDataGrid, 10, ExeIndex, "Module-08 fail!");

                                    //Invoke(WriteDataGrid, 11, ExeIndex, GlobalData.Arduino_IO_INPUT_value.ToString("X2"));
                                    log.Info($"[Input Table Value] 0x{GlobalData.Arduino_IO_INPUT_value.ToString("X2")}");
                                    break;
                                    #endregion
                            }
                        }
                        #endregion
						#region -- Schedule for DDC/CI --
                        else if ((columns_command == "_MCCS"))
                        {
                            log.Debug("Enter schedule of " + columns_command);
                            byte byteValue = 0x00;
                            
                            if (columns_function == "Auto")
                            {
                                monControlSets = MonitorControl.GetMonitorsAndFeatures();
                                //Thread.Sleep(1000);
                                uint setValue = 0;
                                int keyCount = 0, posValsCount = 0;
                                List<Tuple<byte, uint>> dictionaryList = new List<Tuple<byte, uint>>();
                                
                                foreach (var monControlSet in monControlSets)
                                {
                                    keyCount = monControlSet.Capabilities.VCPCodes.Count;
                                    if (keyCount > 0)
                                    {
                                        for (int ii = 0; ii < keyCount; ii++)
                                        {
                                            var vcpCodeByte = monControlSet.Capabilities.VCPCodes.Keys.ElementAt(ii);
                                            var vcpCodeValue = monControlSet.Capabilities.VCPCodes.Values.ElementAt(ii);

                                            if (vcpCodeByte >= 0x10 && vcpCodeByte < 0xFF && vcpCodeByte != 0xD6 && vcpCodeByte != 0x60)  //10h: Luminance; D6h: Power Mode
                                            {
                                                if (vcpCodeValue.PossibleValues.Count > 0)
                                                {
                                                    //=== Non-continuous: varying with all possible values ===//
                                                    posValsCount = vcpCodeValue.PossibleValues.Count;
                                                    for (int jj = 0; jj < ((int)posValsCount); jj++)
                                                    {
                                                        setValue = (uint)vcpCodeValue.PossibleValues[jj];
                                                        dictionaryList.Add(new Tuple<byte, uint>(vcpCodeByte, setValue));
                                                        //log.Debug("[vcpCodeByte] " + vcpCodeByte + " [Value_set] " + setValue);
                                                    }
                                                }
                                                else
                                                {
                                                    //=== Continuous: varying with 3 values (0, current value, maximum value) ===//
                                                    setValue = 0x00;
                                                    dictionaryList.Add(new Tuple<byte, uint>(vcpCodeByte, setValue));
                                                    setValue = vcpCodeValue.MaximumValue;
                                                    dictionaryList.Add(new Tuple<byte, uint>(vcpCodeByte, setValue));
                                                    setValue = vcpCodeValue.CurrentValue; //0x2D;
                                                    dictionaryList.Add(new Tuple<byte, uint>(vcpCodeByte, setValue));
                                                    //log.Debug("[vcpCodeByte] " + vcpCodeByte + " [Value_set] " + setValue);
                                                }
                                            }
                                        }

                                        SetMonitorFeatures(monControlSet.Handle, dictionaryList);
                                        dictionaryList.Clear();
                                    }
                                    else
                                        break;
                                }
                            }
                            else if ((columns_function != "") && (columns_cmdLine != ""))
                            {
                                monControlSets = MonitorControl.GetMonitors();
                                byteValue = byte.Parse(columns_function, System.Globalization.NumberStyles.AllowHexSpecifier);
                                uint setValue = 0;
                                if (byteValue >= 0x02 && byteValue <= 0xFF)
                                {
                                    try
                                    {
                                        if (byteValue == 0x60)
                                            setValue = (uint)Enum.Parse(typeof(ModuleLayer.VideoInput), columns_cmdLine);
                                        else
                                            setValue = uint.Parse(columns_cmdLine);
                                    }
                                    catch (Exception exc)
                                    {
                                        MessageBox.Show(exc.ToString(), "DDCCI");
                                    }
                                }
                                else
                                    MessageBox.Show("Not supported VCP code!", "Schedule");

                                SetMonitorFeatures(monControlSets[0].Handle, byteValue, setValue);
                                //log.Debug("[vcpCodeByte] " + columns_function + " [Value_set] " + columns_subFunction);
                            }
                            else if ((columns_function == "Brightness") && (columns_cmdLine != ""))
                            {
                                Dictionary<FeatureRange, uint> brVal = new Dictionary<FeatureRange, uint>();
                                Dictionary<Feature<ModuleLayer.VideoInput>, uint> tupleVal = new Dictionary<Feature<ModuleLayer.VideoInput>, uint>();
                                uint valueToBeSet = uint.Parse(columns_subFunction);
                                //monFeature.TryParse(columns_function, columns_subFunction, out feat, out valueToBeSet);
                                FeatureRange fea_vi = new FeatureRange("Brightness", 0x10, 0, 100);
                                //int vcpNum = cap.VCPCodes.Count;
                                brVal.Add(fea_vi, uint.Parse(columns_subFunction));
                                SetMonitorFeatures(monControlSets[0].Handle, brVal);
                                brVal.Remove(fea_vi);
                            }

                            log.Debug("Leave schedule of " + columns_command);
                            if (!monControlSets[0].Handle.IsClosed)
                                monControlSets[0].Handle.Close();
                            else if (!monControlSets[1].Handle.IsClosed)
                                monControlSets[1].Handle.Close();

                            Thread.Sleep(Convert.ToInt32(this.dataGridView1.Rows[ExeIndex].Cells[8].Value));
                        }
                        #endregion
						#region -- FTDI Read/Write --
                        else if (columns_command == "_FTDI")
                        {
                            if (GlobalData.portinfo.ftStatus == FtResult.Ok)
                            {
                                if (columns_function == "Write" && columns_cmdLine != "")
                                {
                                    //log.Debug("FTDI Write Log: _FTDI_Write");
                                    int packetLen = columns_cmdLine.Split(' ').Count();

                                    if (cmdVar_int >= 0 && columns_cmdLine.Contains("XX"))
                                    {
                                        string cmdVariable = cmdVarStep.ToString("X2").PadLeft(2, '0');
                                        string columns_cmdLine_subPart = columns_cmdLine.Substring(0, columns_cmdLine.Length - 2);
                                        columns_cmdLine = columns_cmdLine_subPart + cmdVariable;
                                    }

                                    byte[] cmdBytes = new byte[packetLen + 1];     //Plus 1 is reserved for checksum Byte
                                    var tstStr = dataConv.XOR8_BytesWithChksum(columns_cmdLine, cmdBytes, cmdBytes.Length);
                                    
                                    byte DeviceAddr = cmdBytes[0];
                                    byte[] DeviceData = new byte[packetLen];
                                    Array.Copy(cmdBytes, 1, DeviceData, 0, packetLen);
									
                                    FtResult writeResult = GlobalData.Ftdi_lib.I2C_SEQ_Write(GlobalData.portinfo.ftHandle, DeviceAddr, DeviceData);
                                    columns_cmdLine += " " + tstStr;

                                    log.Info($"[{columns_remark}]_{columns_cmdLine}_");
                                    log.Info($"[==>Write Result] {(FtResult)writeResult}");
                                    Invoke(WriteDataGrid, 10, ExeIndex, writeResult.ToString());
                                    columns_cmdLine = "";
                                }
                                else if (columns_function == "Read" && columns_cmdLine != "")
                                {
                                    //log.Debug("FTDI Write Log: _FTDI_Read");
                                    string dataContent = "";
                                    int packetLen = columns_cmdLine.Split(' ').Count();
                                    byte recLength = 0x00;
                                    byte[] readBytes = new byte[128];   //128-Byte is already designated in ftdi library
                                    byte[] cmdBytes = new byte[packetLen + 1];     //Plus 1 is reserved for checksum Byte
                                    var tstStr = dataConv.XOR8_BytesWithChksum(columns_cmdLine, cmdBytes, cmdBytes.Length);
                                    byte DeviceAddr = cmdBytes[0];
                                    byte[] DeviceData = new byte[packetLen];
                                    
                                    Array.Copy(cmdBytes, 1, DeviceData, 0, packetLen);
                                    resultLine = "";
                                    for (int index = 0; index < packetLen; index++)
                                    {
                                        resultLine += cmdBytes[index].ToString("X2");
                                        if (index != (packetLen - 1))
                                            resultLine += " ";
                                    }
                                    
                                    log.Info($"[{columns_remark}]_{columns_cmdLine}_");
                                    columns_cmdLine = "";
                                    Thread.Sleep(500);
                                    if (GlobalData.Ftdi_lib.I2C_SEQ_Read(GlobalData.portinfo.ftHandle, DeviceAddr, DeviceData, readBytes, out recLength) == FtResult.Ok)
                                    {
                                        // Below part is used to print data content with raw data and ascii data formats
                                        //string dataContent = "";
                                        resultLine = "";
                                        DeviceAddr = 0x50;
                                        string ascii_replyData = dataConv.XOR8_FtdiDataParsing(ref resultLine, ref dataContent, cmdBytes, readBytes, ref DeviceAddr, recLength);
										
                                        log.Info($"[FTDI-Reply]_{resultLine}_");
                                        log.Info($"[==>Raw Data]_{dataContent}_");
                                        log.Info($"[=====>ASCII] ({ascii_replyData})");
                                    }
                                }

                                Thread.Sleep(Convert.ToInt32(this.dataGridView1.Rows[ExeIndex].Cells[8].Value));
                            }
                        }
                        #endregion
                        #region -- DOS command --
                        else if (columns_command == "_DOS")
                        {
                            log.Debug("DOS command: _DOS");
                            if (columns_cmdLine != "")
                            {
                                string Command = columns_cmdLine;

                                System.Diagnostics.Process p = new Process();
                                p.StartInfo.FileName = "cmd.exe";
                                p.StartInfo.WorkingDirectory = GlobalData.Dos_Path;
                                p.StartInfo.UseShellExecute = false;
                                p.StartInfo.RedirectStandardInput = true;
                                p.StartInfo.RedirectStandardOutput = true;
                                p.StartInfo.RedirectStandardError = true;
                                p.StartInfo.CreateNoWindow = true; //不跳出cmd視窗
                                string strOutput = null;

                                try
                                {
                                    p.Start();
                                    p.StandardInput.WriteLine(Command);
                                    //p.StandardInput.WriteLine("exit");
                                    //strOutput = p.StandardOutput.ReadToEnd();//匯出整個執行過程
                                    //p.WaitForExit();
                                    //p.Close();
                                }
                                catch (Exception e)
                                {
                                    strOutput = e.Message;
                                }
                            }
                        }
                        #endregion
                        #region -- FOR Loop --
                        else if (columns_command == "_FOR")
                        {
                            try
                            {
                                if (forLoopItem < forLoopIndexList.Count)
                                {
                                    if (forLoopIndexList.ElementAt(forLoopItem).Item1 % 2 == 0 && ExeIndex == forLoopIndexList.ElementAt(forLoopItem).Item2)
                                    {
                                        //FOR Loop start index:
                                        if (columns_times != "")
                                        {
                                            forLoopCount = Convert.ToInt32(columns_times);
                                            // ...... FOR Loop Write Index Byte is ready to start a local loop ...... //
                                            if (columns_interval != "" && columns_cmdLine != "")
                                            {
                                                cmdVar_int = forLoopCount;
                                                cmdVarStep = Convert.ToInt32(columns_interval);
                                            }
                                        }
                                        else
                                            forLoopCount = 1;

                                        forLoopItem++;  //go to end row
                                    }

                                    if (forLoopIndexList.ElementAt(forLoopItem).Item1 % 2 == 1 && ExeIndex == forLoopIndexList.ElementAt(forLoopItem).Item2)
                                    {
                                        //FOR Loop end index:
                                        if (forLoopCount > 1)
                                        {
                                            forLoopCount--;
                                            Invoke(updateDataGrid, ExeIndex, -5, "");   //unselect forLoopEnd row
                                            ExeIndex = forLoopIndexList.ElementAt(forLoopItem - 1).Item2;    //go back to start row
                                            // ...... FOR Loop Write Index Byte is going a local loop ...... //
                                            cmdVar_int--;
                                            cmdVarStep++;
                                            log.Info("FOR Loop - Row " + ExeIndex.ToString() + " remaining " + forLoopCount.ToString());
                                        }
                                        else if (forLoopCount == 1)
                                        {
                                            forLoopCount--;
                                            forLoopItem++;  //go to next start row
                                            // ...... FOR Loop Write Index Byte is resumed as initial ...... //
                                            cmdVar_int = -1;
                                            //cmdVarStep = 0;
                                        }
                                    }
                                }
                                
                            }
                            catch (SystemException se)
                            {
                                MessageBox.Show(se.ToString());
                                break;
                            }
                            /*
                            if (ExeIndex == forLoopEnd1 && forLoopCount > 1)
                            {
                                forLoopCount--;
                                ExeIndex = forLoopStart1;
                                Invoke(updateDataGrid, forLoopEnd1, -5, "");   //unselect forLoopEnd1 row
                            }
                            else if (ExeIndex == forLoopEnd1 && forLoopCount == 1)
                            {
                                forLoopCount--;
                                ExeIndex = tempExeIndex;
                            }
                            */
                        }
                        #endregion
                        //if (FlagPause == 1)
                        if (pauseState)
                        {
                            //Invoke(UpdateUIBtn, 10, 1); //this.BTN_StartTest.Image = global::Cheese.ImageResource.play_button;
                            //playState = false;
                            //Invoke(UpdateUIBtn, 0, 1);  //this.BTN_StartTest.Enabled = true;
                            Invoke(UpdateUIBtn, 3, 2);  //display pause
                            //while (FlagPause == 1)
                            while (pauseState)
                            {
                                Thread.Sleep(10);
                            }
                            Invoke(UpdateUIBtn, 3, 1);  //display testing
                        }
                        //else if (FlagStop == 1)
                        else if (!playState)
                        {
                            //All other stop actions are going at BTN_Stop
                            break;  //stop for loop
                        }
                    }

                    //--------------------- Save test result ----------------//
                    /*
                    DateStr = Directory.GetCurrentDirectory() + "\\" + "TestResult_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                    System.IO.StreamWriter wFile = new System.IO.StreamWriter(DateStr);
                    ResultLine = "CMD,Out String,Delay(ms),CRC Field,Reply String,Result_1,Result_2,Judge,Judge Criterion";
                    wFile.WriteLine(ResultLine);
                    for (ExeIndex = 0; ExeIndex < RowCount; ExeIndex++)
                    {
                        ResultLine = "";
                        for (i = 0; i <= 9; i++)
                        {
                            ResultLine += (((string)this.dataGridView1.Rows[ExeIndex].Cells[i].Value) + ",");
                        }
                        wFile.WriteLine(ResultLine);

                    }
                    wFile.Close();*/

                    loopCounter--;
                    log.Info($"Round-{loopTimes - loopCounter} is done.");
					Invoke(LoopText, 3, loopCounter);
                }
                //----------------------------------------------------//
                MessageBox.Show("All schedules finished");
                //CloseCamera();
                //UpdateUIBtn(3, 3);        //display finish
                Invoke(UpdateUIBtn, 3, 3);  //display finish
                Invoke(UpdateUIBtn, 4, 1);  //button_Camera.Enabled = true;
                Invoke(UpdateUIBtn, 5, 0);  //button_Snapshot.Enabled = false;
                Invoke(UpdateUIBtn, 6, 0);  //button_Schedule.Enabled = false;
                Invoke(UpdateUIBtn, 9, 1);  //cboxCameraList.Enabled = true;
                Invoke(LoopText, 6, 0);
                if (flagLoopTimes)
                    Invoke(LoopText, 1, 0);
            }

            Invoke(UpdateUIBtn, 10, 1); //this.BTN_StartTest.Image = global::Cheese.ImageResource.play_button;
            Invoke(UpdateUIBtn, 0, 1);  //this.BTN_StartTest.Enabled = true;
            Invoke(UpdateUIBtn, 1, 0);  //this.BTN_Pause.Enabled = false;
            Invoke(UpdateUIBtn, 2, 0);  //this.BTN_Stop.Enabled = false;

            DisposeRam();
        }

        public void Arduino_Get_GPIO_Input(ref int GPIO_Read_Data, int delay_time)
        {
            log.Debug("Arduino_Get_GPIO_Input: " + GPIO_Read_Data + ", " + delay_time);
            int retry_cnt = 5;
            //GPIO_Read_Data = 0xFFFF;

            if (GlobalData.sp_Arduino.IsOpen())
            {
                try
                {
                    string dataString = "io i";
                    byte[] tmpBt = new byte[10];
					
					//serial_receive = true;
                    do
                    {
                        //serialPort_Arduino.WriteLine(dataValue);
                        GlobalData.sp_Arduino.WriteDataOut(dataString, dataString.Length);
                        
                        /*
                        int tmpLen = GlobalData.sp_Arduino.ReceiveQueue.Count;
                        for (int i = 0; i < tmpLen; i++)
                        {
                            tmpBt[i] = (byte)GlobalData.m_SerialPort.ReceiveQueue.Dequeue();
                        }
                        for (int i = 0; i < 6; i++)
                        {
                            if (i <= 4)
                                chkStr += tmpBt[i].ToString();
                            else if (i >= 5)
                                valStr += tmpBt[i].ToString();
                        }*/
                        
                        Thread.Sleep(delay_time);
                        
                        serial_receive = GlobalData.Arduino_recFlag;
                        if (!serial_receive && retry_cnt == 0)
                        {
                            MessageBox.Show("Arduino_IO_INPUT: no data received. Please plug the USB wire again.", "Error");
                        }
                        else if (serial_receive == true)
                        {
                            Read_Arduino_Data = GlobalData.Arduino_Read_String;
                            string l_strResult = Read_Arduino_Data.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("ioi", "");
                            GPIO_Read_Data = Convert.ToInt32(l_strResult, 16);
                            
                            GlobalData.Arduino_IO_INPUT_value = (ushort)(GPIO_Read_Data & 0X00FF);
                            GlobalData.Arduino_IO_INPUT_status = (ushort)((GPIO_Read_Data & 0XFF00) >> 8);
                        }
                        retry_cnt--;
                    }
                    while (!serial_receive && retry_cnt > 0);
                }
                catch (System.FormatException)
                {
                    MessageBox.Show("Received byte value format error", "Format Error");
                }
            }
            
        }

        public void Arduino_Set_GPIO_Output(byte outputbyte, int delay_time)
        {
            log.Debug("Arduino_Set_GPIO_Output: " + outputbyte + ", " + delay_time);
            int retry_cnt = 5;

            if (GlobalData.sp_Arduino.IsOpen())
            {
                try
                {
                    string dataString = "io x " + outputbyte;
                    
                    do
                    {
                        //serialPort_Arduino.WriteLine(dataValue);
                        GlobalData.sp_Arduino.WriteDataOut(dataString, dataString.Length);
                        Task.Delay(delay_time).Wait();
                        //Thread.Sleep(delay_time);

                        serial_receive = GlobalData.Arduino_recFlag;
                        if (serial_receive && retry_cnt == 0)
                        {
                            MessageBox.Show("Arduino_IO_OUTPUT_ERROR, Please replug the Arduino board.", "Error");
                        }
                        else if (serial_receive == true)
                        {
                            //reserved
                        }
                        retry_cnt--;
                    }
                    while (!serial_receive && retry_cnt > 0);
                }
                catch (System.FormatException)
                {
                    //MessageBox.Show("Received byte value format error", "Format Error");
                }
            }

        }

        public void Arduino_Set_Value(string input_value, int delay_time)
        {
            log.Debug("Arduino_Set_GPIO_Output: " + input_value + ", " + delay_time);
            uint retry_cnt = 5;

            if (GlobalData.sp_Arduino.IsOpen())
            {
                try
                {
                    string dataValue = input_value;
                    serial_receive = GlobalData.Arduino_recFlag;
                    do
                    {
                        GlobalData.sp_Arduino.WriteDataOut(dataValue, dataValue.Length);
                        Thread.Sleep(delay_time);
                        if (serial_receive == false && retry_cnt == 0)
                        {
                            MessageBox.Show("Arduino response output timeout and please replug the Arduino board.", "Connection Error");
                        }
                        else if (serial_receive == true)
                        {
                            //reserved
                        }
                        retry_cnt--;
                    }
                    while (GlobalData.Arduino_recFlag == false && retry_cnt > 0);
                }
                catch (System.FormatException)
                {
                    MessageBox.Show("Received byte value format error", "Format Error");
                }
            }
        }

        //bool PauseFlag = false;
        //bool ShotFlag = false;
        #region -- IO CMD 指令集 --
        private void IO_CMD(string cmdLine, int cameraIdx, string columns_wait, string columns_remark)
        {
            log.Debug("IO_CMD: " + cmdLine + ", " + cameraIdx + ", " + columns_wait + ", " + columns_remark);
            if (cmdLine == "_shot")
            {
                /*
                ShotFlag = true;
                GlobalData.caption_Num++;
                if (GlobalData.Loop_Number == 1)
                    GlobalData.caption_Sum = GlobalData.caption_Num;
                    */
                Snapshot(cameraIdx, columns_wait, columns_remark);
                //label_Command.Text = "IO CMD_SHOT";
            }/*
            else if (cmdLine == "_pause")
            {
                PauseFlag = true;
                button_Pause.PerformClick();
                label_Command.Text = "IO CMD_PAUSE";
            }
            else if (cmdLine == "_stop")
            {
                button_Start.PerformClick();
                label_Command.Text = "IO CMD_STOP";
            }
            else if (cmdLine == "_ac_restart")
            {
                GP0_GP1_AC_OFF_ON();
                Thread.Sleep(10);
                GP0_GP1_AC_OFF_ON();
                label_Command.Text = "IO CMD_AC_RESTART";
            }
            */
        }
        #endregion

        private void BTN_StartTest_Click(object sender, EventArgs e)
        {
            dataGridView1.Visible = true;
            dUpdateUIBtn UpdateUIBtn = new dUpdateUIBtn(UpdateUIBtnFun);
            //ProcessLoopText LoopText = new ProcessLoopText(UpdateLoopTxt);
            Thread ExecuteCmd_Thread = new Thread(new ThreadStart(ExecuteCmd));
            Thread SerialPort_Receive_Thread = new Thread(new ThreadStart(SerialRxThread));
            //Thread SerialPort_CmdHandling_Thread = new Thread(new ThreadStart(SerialPort_Cmd_Handling));

            //if (playBtnPressed == false && GlobalData.m_SerialPort.IsOpen())
            //if (GlobalData.Arduino_openFlag || GlobalData.m_SerialPort.IsOpen())    //( || TCPIP_socket.IsConnected())
            {
                if (!playState)
                {   //--- Press PLAY button ---//
                    dataGridView1.Visible = true;
                    Invoke(UpdateUIBtn, 1, 1);  //this.BTN_Pause.Enabled = true;
                    //Invoke(UpdateUIBtn, 2, 1);  //this.BTN_Stop.Enabled = true;
                    Invoke(UpdateUIBtn, 9, 0);  //cboxCameraList.Enabled = false;
                    //Invoke(LoopText, 0, 0);
                    //FlagPause = 0;
                    if (!ExecuteCmd_Thread.IsAlive)
                        ExecuteCmd_Thread.Start();
                    //Console.WriteLine("Playing");
                    //Console.WriteLine("Thread ID = {0}", ExecuteCmd_Thread.ManagedThreadId.ToString());

                    if (!SerialPort_Receive_Thread.IsAlive)
                        SerialPort_Receive_Thread.Start();
                    //SerialPort_Receive_Thread.IsBackground = true;
                    //Console.WriteLine("Listening");
                    //Console.WriteLine("Thread ID = {0}", SerialPort_Receive_Thread.ManagedThreadId.ToString());

                    Invoke(UpdateUIBtn, 10, 0);     //this.BTN_StartTest.Image = global::Cheese.ImageResource.stop;
                    //FlagStop = 0;
                    lock(this)
                    {
                        playState = true;
                        pauseState = false;
                    }
                }
                //else if (playBtnPressed == true && GlobalData.m_SerialPort.IsOpen())
                //else if (stopBtnPressed && (GlobalData.Arduino_openFlag || GlobalData.m_SerialPort.IsOpen()))
                else if (playState)
                {   //--- Press PLAY button again (i.e. press STOP button) ---//
                    //Invoke(UpdateUIBtn, 2, 0);  //this.BTN_Stop.Enabled = false;
                    Invoke(UpdateUIBtn, 1, 0);  //this.BTN_Pause.Enabled = false;
                    if (timeOutTimer != null && timeOutTimer.TimerOnIndicator())
                    {
                        timeOutTimer.StopTimeoutTimer(-9.9);
                        timeOutTimer.DisposeTimeoutTimer();
                    }

                    if (pauseState)   //(FlagPause == 1)
                        pauseState = false;    //FlagPause = 0;

                    if (ExecuteCmd_Thread.IsAlive)
                        ExecuteCmd_Thread.Abort();
                    
                    if (SerialPort_Receive_Thread.IsAlive)
                        SerialPort_Receive_Thread.Abort();
                    /*
                    if (SerialPort_CmdHandling_Thread.IsAlive)
                        SerialPort_CmdHandling_Thread.Abort();
                    */
                    //Console.WriteLine("Stopping");
                    //Console.WriteLine("Thread ID = {0}", ExecuteCmd_Thread.ManagedThreadId.ToString());
                    //Console.WriteLine("Thread ID = {0}", SerialPort_Receive_Thread.ManagedThreadId.ToString());

                    Invoke(UpdateUIBtn, 10, 1); //this.BTN_StartTest.Image = global::Cheese.ImageResource.play_button;
                    Invoke(UpdateUIBtn, 0, 1);  //this.BTN_StartTest.Enabled = true;
                    //FlagStop = 1;
                    lock (this)
                        playState = false;
                }
            }/*
            else
            {
                MessageBox.Show("SerialPort or Socket is not connected!", "Info");
                Invoke(UpdateUIBtn, 0, 1);  //this.BTN_StartTest.Enabled = true;
            }*/

            //Invoke(UpdateUIBtn, 0, 0);  //*** This must be remarked for condition of PLAY & STOP co-button. ***//
            /*
            if (ExecuteCmd_Thread == null)
            {
                ExecuteCmd_Thread = new Thread(new ThreadStart(ExecuteCmd));
                ExecuteCmd_Thread.Start();
            }
            else if (ExecuteCmd_Thread.IsAlive == false)
            {
                ExecuteCmd_Thread.Abort();
                ExecuteCmd_Thread = new Thread(new ThreadStart(ExecuteCmd));
                ExecuteCmd_Thread.Start();
            }

            if (SerialPort_Receive_Thread == null)
            {
                SerialPort_Receive_Thread = new Thread(new ThreadStart(SerialRxThread));
                SerialPort_Receive_Thread.IsBackground = true;
                SerialPort_Receive_Thread.Start();
            }
            else if (SerialPort_Receive_Thread.IsAlive == false)
            {
                SerialPort_Receive_Thread.Abort();
                SerialPort_Receive_Thread = new Thread(new ThreadStart(SerialRxThread));
                SerialPort_Receive_Thread.IsBackground = true;
                SerialPort_Receive_Thread.Start();
            }

            if (SerialPort_CmdHandling_Thread == null)
            {
                SerialPort_CmdHandling_Thread = new Thread(new ThreadStart(SerialPort_Cmd_Handling));
                SerialPort_CmdHandling_Thread.IsBackground = true;
                SerialPort_CmdHandling_Thread.Start();
            }
            else if (SerialPort_CmdHandling_Thread.IsAlive == false)
            {
                SerialPort_CmdHandling_Thread.Abort();
                SerialPort_CmdHandling_Thread = new Thread(new ThreadStart(SerialPort_Cmd_Handling));
                SerialPort_CmdHandling_Thread.IsBackground = true;
                SerialPort_CmdHandling_Thread.Start();
            }
            */
        }

        private void BTN_Pause_Click(object sender, EventArgs e)
        {
            dUpdateUIBtn UpdateUIBtn = new dUpdateUIBtn(UpdateUIBtnFun);
            if (!pauseState)
            {
                Invoke(UpdateUIBtn, 0, 0);  //this.BTN_StartTest.Enabled = false;
                pauseState = true;    //FlagPause = 1;
            }
            else
            {
                Invoke(UpdateUIBtn, 0, 1);  //this.BTN_StartTest.Enabled = true;
                pauseState = false;
            }
            /*
            //Invoke(UpdateUIBtn, 1, 0);  //this.BTN_Pause.Enabled = false;
            Invoke(UpdateUIBtn, 2, 0);  //this.BTN_Stop.Enabled = false;
            Invoke(UpdateUIBtn, 0, 0);  //this.BTN_StartTest.Enabled = false;

            //Invoke(UpdateUIBtn, 10, 1); //this.BTN_StartTest.Image = global::Cheese.ImageResource.play_button;

            //Invoke(UpdateUIBtn, 0, 1);  //this.BTN_StartTest.Enabled = true;
            lock(this)
            {
                pauseBtnPressed = true;    //FlagPause = 1;
                playState = false;
            }*/
        }

        private void BTN_Stop_Click(object sender, EventArgs e)
        {
            dUpdateUIBtn UpdateUIBtn = new dUpdateUIBtn(UpdateUIBtnFun);
            ProcessLoopText LoopText = new ProcessLoopText(UpdateLoopTxt);
            Invoke(UpdateUIBtn, 2, 0);  //this.BTN_Stop.Enabled = false;
            Invoke(UpdateUIBtn, 1, 0);  //this.BTN_Pause.Enabled = false;
            Invoke(LoopText, 1, 0);

            if (timeOutTimer != null && timeOutTimer.TimerOnIndicator())
            {
                timeOutTimer.StopTimeoutTimer(-9.9);
                timeOutTimer.DisposeTimeoutTimer();
            }

            if (pauseState)   //(FlagPause == 1)
                pauseState = false;    //FlagPause = 0;

            playState = false;
            //FlagStop = 1;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {   // == Open File BTN == //
            OpenFileDialog dialog = new OpenFileDialog();
            dUpdateUIBtn UpdateUIBtn = new dUpdateUIBtn(UpdateUIBtnFun);
            string schedule_opened = ini12.INIRead(GlobalData.MainSettingPath, "Schedule1", "Exist", "");

            dialog.Filter = "csv files (*.*) |*.csv";
            dialog.Title = "Select One Schedule File";
            if (schedule_opened == "" || schedule_opened == "0")
                dialog.InitialDirectory = "D:\\";
            else if (schedule_opened == "1")
                dialog.InitialDirectory = ini12.INIRead(GlobalData.MainSettingPath, "Schedule1", "Path", "");

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TargetFilePath = dialog.FileName;
                string safeFileName = dialog.SafeFileName;
                Console.WriteLine(TargetFilePath);
                //Task task = new Task();
                //task.Start();
                UpdateDataGrid();

                ini12.INIWrite(GlobalData.MainSettingPath, "Schedule1", "Exist", "1");
                ini12.INIWrite(GlobalData.MainSettingPath, "Schedule1", "Path", TargetFilePath);

                Invoke(UpdateUIBtn, 4, 0);  //button_Camera.Enabled = false;
                Invoke(UpdateUIBtn, 5, 0);  //button_Snapshot.Enabled = false;
                Invoke(UpdateUIBtn, 6, 1);  //button_Schedule.Enabled = true;
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {   // == Setting BTN == //
            Setting form2 = new Setting();
            dUpdateUI ArduinoLED = new dUpdateUI(Form1UpdateArduinoLedStatus);
            dUpdateUI UILED = new dUpdateUI(Form1UpdateComportLedStatus);
            dUpdateUI UINetworkLED = new dUpdateUI(Form1UpdateNetworkLedStatus);
            int ComportStatus;
            string PortNumber;
            int BaudRate, ParityBit, StopBit, DataLen;
            int NetworkStatus;
            string IP;
            int NetworkPort;

            //---------------- display RS232 setting panel ------------------//
            //form2.Owner = this;
            if (NetworkHandle.IsConnected())
            {
                NetworkHandle.Close();
                UINetworkLED.Invoke(0);
            }

            if (GlobalData.m_SerialPort.IsOpen())
            {
                GlobalData.m_SerialPort.ClosePort();
                UILED.Invoke(0);
            }

            /*
            if (GlobalData.sp_Arduino.IsOpen())
            {
                GlobalData.sp_Arduino.ClosePort();
                ArduinoLED.Invoke(0);
            }
            */

            form2.ShowDialog(this);
            if (form2.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                /*  === do this job at Setting.cs ===
                ComportStatus = form2.getComPortChecked();
                PortNumber = form2.getComPortSetting();
                BaudRate = Convert.ToInt32(form2.getComPortBaudRate());
                ParityBit = form2.getComPortParity();
                StopBit = form2.getComPortStopBit();
                DataLen = form2.getComPortByteCount();
                */
                NetworkStatus = form2.getNetworkChecked();
                IP = form2.getNetworkIP();
                NetworkPort = int.Parse(form2.getNetworkPort());
                timeout = form2.getNetworkTimeOut();
                Device = form2.getCameraDevice();
                Resolution = form2.getCameraResolution();

                //if (ComportStatus == 1)
                {
                    //if (GlobalData.m_SerialPort.OpenPort(PortNumber, BaudRate, ParityBit, DataLen, StopBit) >= 1)
                    if (GlobalData.m_SerialPort.IsOpen())
                    {
                        UILED.Invoke(1);
                    }
                    else
                    {
                        UILED.Invoke(0);
                        MessageBox.Show("Fail to open ComPort!");
                    }
                }

                if (NetworkStatus == 1)
                {
                    //if (NetworkHandle.TestConnection(IP, NetworkPort, 500) == true)
                    if (!NetworkHandle.IsConnected())
                    {
                        NetworkHandle.SetIpAddr(IP);
                        NetworkHandle.SetPortNumber(NetworkPort);
                        //if (!NetworkHandle.IsConnected())
                        {
                            NetworkHandle.Start();
                            UINetworkLED.Invoke(1);
                        }
                    }
                    //else if (NetworkHandle.TestConnection(IP, NetworkPort, 500) == false)
                    else if (NetworkHandle.IsConnected())
                    {
                        UINetworkLED.Invoke(0);
                        MessageBox.Show("Please Check the TCPIP server status.");
                    }
                    else
                    {
                        UINetworkLED.Invoke(0);
                        MessageBox.Show("Open Socket fail.");
                    }
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            string delimiter = ",";

            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName, false))
                {
                    //output header data
                    string strHeader = "";
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        strHeader += dataGridView1.Columns[i].HeaderText + delimiter;
                    }
                    sw.WriteLine(strHeader.Replace("\r\n", "~"));
                    
                    //output rows data
                    for (int j = 0; j < dataGridView1.Rows.Count - 1; j++)
                    {
                        string strRowValue = "";

                        for (int k = 0; k < dataGridView1.Columns.Count; k++)
                        {
                            string scheduleOutput = dataGridView1.Rows[j].Cells[k].Value + "";
                            if (scheduleOutput.Contains(","))
                            {
                                scheduleOutput = String.Format("\"{0}\"", scheduleOutput);
                            }
                            strRowValue += scheduleOutput + delimiter;
                        }
                        sw.WriteLine(strRowValue);
                    }
                    sw.Close();
                }
            }
        }
		
        public void SerialRxThread()
        {
            //GetSerialData(GlobalData.m_SerialPort);
            while (GlobalData.m_SerialPort.IsOpen())
            {
                GlobalData.m_SerialPort.Receive();

                //while (playState)
                {
                    while (GlobalData.m_SerialPort.ReceiveQueue.Count > 0)
                    {
                        byte[] queueBuffer = new byte[GlobalData.m_SerialPort.ReceiveQueue.Count];
                        string byteToString = "";
                        for (int i = 0; i < queueBuffer.Length; i++)
                        {
                            queueBuffer[i] = GlobalData.m_SerialPort.GeneralDequeue();
                            if (i < queueBuffer.Length - 1)
                                byteToString += queueBuffer[i].ToString("X2").PadLeft(2, '0') + " ";
                            else if (i == queueBuffer.Length - 1)
                                byteToString += queueBuffer[i].ToString("X2").PadLeft(2, '0');
                            //benQ.PacketDequeuedToList(GlobalData.m_SerialPort, ref cmdByteList);
                            //===benQ.PacketDequeuedToList(GlobalData.m_SerialPort);
                            //Thread.Sleep(500);
                            //benQ.QueueAddedToList_ODM_BenQ(ref cmdByteList, ref packetQueueList);
                            //===benQ.QueueAddedToList_ODM_BenQ(GlobalData.m_SerialPort);
                        }

                        GlobalData.Measure_Backlight = Encoding.ASCII.GetString(queueBuffer);
                        GlobalData.Measure_Thermal = byteToString;
                    }
                    Task.Delay(1000).Wait();
                }
            }
        }

        private void SerialPort_Cmd_Handling()
        {
                /*while (playState)
                {
                    while (GlobalData.m_SerialPort.ReceiveQueue.Count > 0)
                    {
                        byte[] queueBuffer = new byte[GlobalData.m_SerialPort.ReceiveQueue.Count];
                        string byteToString = "";
                        for (int i = 0; i < queueBuffer.Length; i++)
                        {
                            queueBuffer[i] = GlobalData.m_SerialPort.GeneralDequeue();
                            if (i < queueBuffer.Length - 1)
                                byteToString += queueBuffer[i].ToString("X2").PadLeft(2, '0') + " ";
                            else if (i == queueBuffer.Length - 1)
                                byteToString += queueBuffer[i].ToString("X2").PadLeft(2, '0');
                            //benQ.PacketDequeuedToList(GlobalData.m_SerialPort, ref cmdByteList);
                            //===benQ.PacketDequeuedToList(GlobalData.m_SerialPort);
                            //Thread.Sleep(500);
                            //benQ.QueueAddedToList_ODM_BenQ(ref cmdByteList, ref packetQueueList);
                            //===benQ.QueueAddedToList_ODM_BenQ(GlobalData.m_SerialPort);
                        }

                        GlobalData.Measure_Backlight = Encoding.ASCII.GetString(queueBuffer);
                        GlobalData.Measure_Thermal = byteToString;
                    }
                    Thread.Sleep(1000);
                }*/
            }

            public void Snapshot(int cameraSelectMode, string delayTimeString, string remark)
        {
            //log.Debug("Snapshot: " + cameraSelectMode + ", " + delayTimeString + ", " + remark);
            string image_currentPath = System.Environment.CurrentDirectory;
            string deviceName = "";
            int camera_counter = 0, camera_startNum = 0;

            if (cameraSelectMode > 0)
            {   //designate cam number
                camera_counter = cameraSelectMode - 1;
                camera_startNum = camera_counter;
            }
            else if (cameraSelectMode < 0)
            {   //snapshot one by one for all cameras
                camera_counter = videoDevices.Count - 1;
                camera_startNum = 0;
            }

            for (int i = camera_startNum; i <= camera_counter; i++)
            {
                try
                {
                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssff") + "_" + Convert.ToString(Int32.Parse(Txt_LoopTimes.Text) - Int32.Parse(Txt_LoopCounter.Text) + 1) + "_" + GlobalData.caption_Num + ".jpeg";

                    if (cameraSelectMode < 0 && i == 0)
                    {
                        deviceName = (i + 1).ToString() + "_" + videoDevices[i].Name.ToString();
                        //iVideoSource1 = videoSourcePlayer1.VideoSource;
                        Bitmap bmp = videoSourcePlayer1.GetCurrentVideoFrame();
                        string saveName = image_currentPath + "\\" + deviceName + "\\" + deviceName + "_" + fileName;
                        DrawOnBitmap(ref bmp, remark, delayTimeString, deviceName, saveName);
                    }
                    else if (cameraSelectMode < 0 && i == 1)
                    {
                        deviceName = (i + 1).ToString() + "_" + videoDevices[i].Name.ToString();
                        //iVideoSource2 = videoSourcePlayer2.VideoSource;
                        Bitmap bmp = videoSourcePlayer2.GetCurrentVideoFrame();
                        string saveName = image_currentPath + "\\" + deviceName + "\\" + deviceName + "_" + fileName;
                        DrawOnBitmap(ref bmp, remark, delayTimeString, deviceName, saveName);
                    }
                    else if (cameraSelectMode > 0)
                    {
                        deviceName = videoDevices[i].Name.ToString();
                        Bitmap bmp = null;
                        if (0 == i)
                        {
                            //iVideoSource1 = videoSourcePlayer1.VideoSource;
                            bmp = videoSourcePlayer1.GetCurrentVideoFrame();
                        }
                        if (1 == i)
                        {
                            //iVideoSource2 = videoSourcePlayer2.VideoSource;
                            bmp = videoSourcePlayer2.GetCurrentVideoFrame();
                        }

                        string saveName = image_currentPath + "\\" + deviceName + "\\" + deviceName + "_" + fileName;
                        DrawOnBitmap(ref bmp, remark, delayTimeString, deviceName, saveName);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Actual number of WebCam is lower than setting!\n" + ex);
                }
                
            }

            //CloseCamera();
        }

        public class TimeoutTimer
        {
            private System.Timers.Timer Counter_Timer;
            public TimeoutTimer(double tmOut)
            {
                Counter_Timer = new System.Timers.Timer(tmOut);
                Counter_Timer.Interval = tmOut;
                Counter_Timer.Elapsed += new ElapsedEventHandler(Counter_Delay_OnTimedEvent);
            }

            static bool TimeoutIndicator = false;
            static UInt64 TimeoutCounter_Count = 0;
            private void Counter_Delay_OnTimedEvent(object source, ElapsedEventArgs e)
            {
                TimeoutCounter_Count++;
                TimeoutIndicator = true;
            }

            public bool TimerOnIndicator()
            {
                return TimeoutIndicator;
            }

            public void StartTimeoutTimer()
            {
                if (Counter_Timer.Interval >= 0.0)
                {
                    Counter_Timer.Enabled = true;
                    Counter_Timer.Start();
                    Counter_Timer.AutoReset = true;

                    TimeoutCounter_Delay();
                }
            }

            public void StopTimeoutTimer(double tout)
            {
                if (tout == -9.9)
                    Counter_Timer.Stop();
            }

            public void DisposeTimeoutTimer()
            {
                Counter_Timer.Dispose();
            }

            private void TimeoutCounter_Delay()
            {
                bool network_receive = false;
                if (tmout >= 0)
                    network_receive = true;
                
                while (TimeoutIndicator == false && network_receive == true)
                {
                    //Application.DoEvents();
                    //Thread.Sleep(50);
                    Cmdreceive = NetworkHandle.Receive();
                    
                    if (Cmdreceive == Cmdsend + "_RobotDone")     //e.g. Path_10_RobotDone"
                    {
                        network_receive = false;
                    	Cmdsend = "";
                        StopTimeoutTimer(-9.9);
                        DisposeTimeoutTimer();
                    }
                }

                while (TimeoutIndicator == true && network_receive == true)
                {
                    Setting form2 = new Setting();
                    sendMail(form2.getMailAddress());
                    StopTimeoutTimer(-9.9);
                    DisposeTimeoutTimer();
                    network_receive = false;
                    TimeoutIndicator = false;
                    Cmdreceive = "Mail notification already sends.";
                }

            }   //The end of TimeoutTimer.TimeoutCounter_Delay()
        }

        private void button_AcOn_Click(object sender, EventArgs e)
        {
            string dataString = "io x 7";     //  111: pin12/pin11/pin10
            GlobalData.sp_Arduino.WriteDataOut(dataString, dataString.Length);
            button_AcOn.Enabled = false;
            button_AcOff.Enabled = true;
        }

        private void button_AcOff_Click(object sender, EventArgs e)
        {
            string dataString = "io x 5";     //  101: pin12/pin11/pin10
            GlobalData.sp_Arduino.WriteDataOut(dataString, dataString.Length);
            button_AcOff.Enabled = false;
            button_AcOn.Enabled = true;
        }

        public static void sendMail(string mailTo)
        {
            log.Debug("sendMail: " + mailTo);
            string To = mailTo + ",";
            int z = 0;
            string[] to = To.Split(new char[] { ',' });
            List<string> MailList = new List<string> { };

            while (to[z] != "")
            {
                MailList.Add(to[z]);
                z++;
            }

            string Subject = "Robot client receive timeout !!!";

            string Body =
                                    "<br>" + "<br>" +

                                    "Robot client received message timeout !!! " + "<br>" + "<br>" +

                                    "Please note this E-mail is sent by Google mail system automatically, do not reply. If you have any questions please contact system administrator.";

            SendMail(MailList, Subject, Body);
        }

        public static void SendMail(List<string> MailList, string Subject, string Body)
        {
            log.Debug("SendMail: " + Subject + ", " + Subject + ", " + Body);
            MailMessage msg = new MailMessage();

            msg.To.Add(string.Join(",", MailList.ToArray()));       //收件者，以逗號分隔不同收件者
            msg.From = new MailAddress("nbosss.dqa@gmail.com", "NBOSSS_DQA", System.Text.Encoding.UTF8);
            msg.Subject = Subject;      //郵件標題 
            msg.SubjectEncoding = System.Text.Encoding.UTF8;        //郵件標題編碼  
            msg.Body = Body;        //郵件內容

            msg.IsBodyHtml = true;
            msg.BodyEncoding = System.Text.Encoding.UTF8;       //郵件內容編碼 
            msg.Priority = MailPriority.High;     //郵件優先級 

            //建立 SmtpClient 物件 並設定 Gmail的smtp主機及Port 

            #region 其它 Host
            /*
            ~~~~~~~~~~~~~~~~~       outlook.com smtp.live.com port:25
            ~~~~~~~~~~~~~~~~~       yahoo smtp.mail.yahoo.com.tw port:465
            ~~~~~~~~~~~~~~~~~       smtp.gmail.com port:587
            ~~~~~~~~~~~~~~~~~       tpmx.tpvaoc.com port: 25        //公司內部的SMTP
            ~~~~~~~~~~~~~~~~~       msa.hinet.net port: 25
            */
            #endregion

            try
            {
                SmtpClient MySmtp = new SmtpClient("smtp.gmail.com", 587);
                MySmtp.Credentials = new System.Net.NetworkCredential("nbosss.dqa@gmail.com", "Auo+1234");     //設定你的帳號密碼
                MySmtp.EnableSsl = true;      //Gmail 的 smtp 需打開 SSL
                MySmtp.Send(msg);
            }
            catch (Exception)
            {
                MessageBox.Show("Connect the google smtp server error and mail setting value is disabled. Please check the network connect status.", "Mail send error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //Fill Camera ListBox and set default value
            CameraFilterCollection();

            if (cboxCameraList.Items.Count > 0)
            {
                button_Camera.Enabled = true;
                //VideoPlayerInitializing(cameraIndex);
            }
            else
            {
                button_Camera.Enabled = false;
                button_Schedule.Enabled = false;
                //MessageBox.Show("No camera exist");
            }

            dUpdateUI ArduinoLED = new dUpdateUI(Form1UpdateArduinoLedStatus);
            if (GlobalData.Arduino_openFlag)
                ArduinoLED.Invoke(1);
            else
                ArduinoLED.Invoke(0);
			
            dUpdateUI FTDILED = new dUpdateUI(Form1UpdateFTDILedStatus);
            if (GlobalData.FTDI_openFlag == true)
            {
                GlobalData.portinfo.I2C_Channel_Conf.ClockRate = Ft_I2C_ClockRate.I2C_CLOCK_STANDARD_MODE;
                GlobalData.portinfo.I2C_Channel_Conf.LatencyTimer = 200;
                GlobalData.portinfo.I2C_Channel_Conf.Options = 0x00000001;     //FtConfigOptions.I2C_DISABLE_3PHASE_CLOCKING;
                GlobalData.portinfo.PortNum = 0;
                GlobalData.portinfo.ftStatus = GlobalData.Ftdi_lib.I2C_Init(out GlobalData.portinfo.ftHandle, GlobalData.portinfo);
                FTDILED.Invoke(1);
            }
            else
                FTDILED.Invoke(0);

            this.Closing += new CancelEventHandler(Main_FormClosing);
        }

        private void Main_FormClosing(object sender, CancelEventArgs e)
        {
            CloseCamera();
            if (GlobalData.sp_Arduino.IsOpen())
                GlobalData.sp_Arduino.ClosePort();
            
            if (GlobalData.m_SerialPort.IsOpen())
                GlobalData.m_SerialPort.ClosePort();

            /*  Thread resource collection is aleady hadnled by VS system
            if (ExecuteCmd_Thread != null)
                ExecuteCmd_Thread.Abort();
            if (SerialPort_Receive_Thread != null)
                SerialPort_Receive_Thread.Abort();
            if (SerialPort_CmdHandling_Thread.IsAlive)
                SerialPort_CmdHandling_Thread.Abort();
            */
            //Environment.Exit(0);
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*if (SerialPort_Receive_Thread != null && SerialPort_Receive_Thread.IsAlive)
                SerialPort_Receive_Thread.Abort();*/
            //CloseCamera();
            
            Environment.Exit(0); //exit the Application process
            //Application.Exit();
        }

        private void chkBox_LoopTimes_CheckedChanged(object sender, EventArgs e)
        {
            ProcessLoopText LoopText = new ProcessLoopText(UpdateLoopTxt);
            if (this.chkBox_LoopTimes.Checked == true)
            {
                //this.Txt_LoopTimes.Enabled = true;
                //this.Txt_LoopCounter.Enabled = true;
                Invoke(LoopText, 1, 0);
                //loopTimes = Convert.ToInt32(Txt_LoopTimes.Text);
                //loopCounter = Convert.ToInt32(Txt_LoopCounter.Text);
                Invoke(LoopText, 2, 0);
                Invoke(LoopText, 3, loopCounter);
                flagLoopTimes = true;
            }
            else
            {
                //this.Txt_LoopTimes.Enabled = false;
                //this.Txt_LoopCounter.Enabled = false;
                Invoke(LoopText, 0, 0);
                //Invoke(LoopText, 4, 1);
                //loopTimes = 1;
                //loopCounter = 1;
                flagLoopTimes = false;
            }
        }

        private void Txt_LoopTimes_TextChanged(object sender, EventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            if (this.chkBox_LoopTimes.Checked == true)
            {
                Txt_LoopCounter.Text = txtBox.Text;
                //loopCounter = Convert.ToInt32(Txt_LoopCounter.Text);
            }
            //else
            //{ No need since Txt_LoopCounter is disabled. }
        }

        private void button_Camera_Click(object sender, EventArgs e)
        {
            dUpdateUIBtn UpdateUIBtn = new dUpdateUIBtn(UpdateUIBtnFun);
            if (videoDevices.Count > 0)     //(cboxCameraList.Items.Count > 0)
                {
                //VideoPlayerInitializing(cboxCameraList.SelectedIndex);
                if (videoSource1 != null && !videoSource1.IsRunning)
                    VideoPlayerInitializing(0);

                //if (!videoSourcePlayer2.IsRunning)
                if (videoSource2 != null && !videoSource2.IsRunning)
                    VideoPlayerInitializing(1);

                dataGridView1.Visible = false;
                Invoke(UpdateUIBtn, 4, 0);  //button_Camera.Enabled = false;
                Invoke(UpdateUIBtn, 5, 1);  //button_Snapshot.Enabled = true;
                Invoke(UpdateUIBtn, 6, 1);  //button_Schedule.Enabled = true;
            }
            else
            {
                MessageBox.Show("No Device selected.", "Error");
            }
        }

        private void button_Schedule_Click(object sender, EventArgs e)
        {
            dUpdateUIBtn UpdateUIBtn = new dUpdateUIBtn(UpdateUIBtnFun);
            dataGridView1.Visible = true;
            if (videoDevices.Count > 0)
            {
                Invoke(UpdateUIBtn, 4, 1);  //button_Camera.Enabled = true;
                Invoke(UpdateUIBtn, 5, 0);  //button_Snapshot.Enabled = false;
                Invoke(UpdateUIBtn, 6, 0);  //button_Schedule.Enabled = false;
                //CloseCamera();
            }
        }

        private void button_Snapshot_Click(object sender, EventArgs e)
        {
            switch (cboxCameraList.SelectedIndex)
            {
                case 0:
                    bitmap = videoSourcePlayer1.GetCurrentVideoFrame();
                    //iVideoSource1 = videoSourcePlayer1.VideoSource;
                    //iVideoSource1.NewFrame += new NewFrameEventHandler(playerControl_Snapshot);
                    videoSource1.NewFrame += new NewFrameEventHandler(playerControl_Snapshot);
                    Thread.Sleep(100);
                    picBox_preview.Image = bitmap;
                    picBox_preview.SizeMode = PictureBoxSizeMode.StretchImage;
                    //bitmap.Dispose(); //will cause error of Application.Run(new Main())
                    break;
                case 1:
                    bitmap = videoSourcePlayer2.GetCurrentVideoFrame();
                    videoSource2.NewFrame += new NewFrameEventHandler(playerControl_Snapshot);
                    Thread.Sleep(100);
                    picBox_preview.Image = bitmap;
                    picBox_preview.SizeMode = PictureBoxSizeMode.StretchImage;
                    break;
            }
        }

        //Iterate all cameras and put into list
        public void CameraFilterCollection()
        {
            try
            {
                cboxCameraList.Items.Clear();
                videoDevices = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                    throw new Exception();
                
                foreach (AForge.Video.DirectShow.FilterInfo device in videoDevices)
                {
                    cboxCameraList.Items.Add(device.Name);
                }

                cboxCameraList.SelectedIndex = 0;
                cameraIndex = cboxCameraList.SelectedIndex;
            }
            catch
            {
                cameraIndex = -1;
                //videoDevices = null;
                cboxCameraList.Items.Add("No camera found");
                cboxCameraList.Enabled = false;
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            GPIO GPIO = new GPIO();
            GPIO.Owner = this;
            if (GlobalData.GPIO == false)
            {
                GPIO.Show();
            }
        }

        private void cboxCameraList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxCameraList.SelectedIndex < 0)
            {
                CloseCamera();
            }
            else
            {
                // Set videoSource for camera
                if (cboxCameraList.Items.Count > 0)
                {
                    cameraIndex = cboxCameraList.SelectedIndex;
                    VideoPlayerInitializing(cameraIndex);
                }
            }
        }

        //eventhandler if new frame is ready
        //public void playerControl_Snapshot(object sender, ref Bitmap bitmap)
        public void playerControl_Snapshot(object sender, NewFrameEventArgs eventArgs)
        {
            //lock (this)
            {
                Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
                string image_CurrentPath = System.Environment.CurrentDirectory;
                string fileName = DateTime.Now.ToString("yyyyMMd-HHmmssff") + ".jpg";

                bmp.Save(image_CurrentPath + "\\" + fileName, ImageFormat.Jpeg);
                bmp.Dispose();
                
                if (sender.Equals(videoSource1))
                    videoSource1.NewFrame -= new NewFrameEventHandler(playerControl_Snapshot);
                else if (sender.Equals(videoSource2))
                    videoSource2.NewFrame -= new NewFrameEventHandler(playerControl_Snapshot);
            }
        }

        public void DrawOnBitmap(ref Bitmap bmp, string remarkStr, string delayTimeStr, string deviceName, string saveName)
        {
            log.Debug("DrawOnBitmap: " + remarkStr + ", " + delayTimeStr + ", " + deviceName + ", " + saveName);
            //Rectangle rect = new Rectangle(70, 90, 90, 50);
            Graphics gph = Graphics.FromImage(bmp);
            // 1. Draw time
            DateTime dt = DateTime.Now;
            gph.DrawString(string.Format("{0:R}", dt), new Font("Arial", 16), Brushes.Yellow, 0, 0);
            // 2. Draw AC on/off status
            if (GlobalData.Arduino_relay_status)
                gph.DrawString("AC Source: On", new Font("Arial", 16), Brushes.Yellow, 0, 20);
            else
                gph.DrawString("AC Source: Off", new Font("Arial", 16), Brushes.Yellow, 0, 20);
            // 3. Draw schedule remark
            gph.DrawString(string.Format("{0}", remarkStr), new Font("Arial", 16), Brushes.Yellow, 0, 40);
            gph.Flush();

            //Delay Time
            int delayTime;
            if (delayTimeStr != null)
            {
                delayTime = Convert.ToInt32(delayTimeStr);
            }
            else
            {
                delayTime = 500;
            }

            Thread.Sleep(delayTime);
            if (CreateSavingfolder(deviceName))
            {
                bmp.Save(saveName, ImageFormat.Jpeg);
                bmp.Dispose();
            }
            else
                MessageBox.Show("Error happened when creating folders.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        public void VideoPlayerInitializing(int camIndex)
        {
            log.Debug("VideoPlayerInitializing: " + camIndex);
            dUpdateUiString updateString = new dUpdateUiString(UpdateUiString);
            if (videoSourcePlayer1.IsRunning)
            {
                videoSourcePlayer1.Stop();
                videoSource1 = null;
            }
            if (videoSourcePlayer2.IsRunning)
            {
                videoSourcePlayer2.Stop();
                videoSource2 = null;
            }
            
            if (bitmap != null)
                bitmap.Dispose();
            int res_Index = 1; //8;
            if (videoDevices.Count > 0 && videoSource1 == null && videoSource2 == null)
            {
                videoSource1 = new VideoCaptureDevice(videoDevices[0].MonikerString);
                //videoSource1.DesiredFrameRate = 10;
                int resolution_Index_1 = videoSource1.VideoCapabilities.Count();
                // C310 [13/19]: 960x544; // C310 [15/19]: 1024x576; //C615 [11/15]: 960x720
                //do not set too high resolution in case of crash issue
                if (resolution_Index_1 >= res_Index)
                {
                    videoSource1.VideoResolution = videoSource1.VideoCapabilities[resolution_Index_1 - res_Index];
                    string resString = videoSource1.VideoResolution.FrameSize.Width.ToString() + " x " +
                                        videoSource1.VideoResolution.FrameSize.Height.ToString();

                    Invoke(updateString, 1, resString);
                }
                else
                    MessageBox.Show("Not enough items present in resolution list");

                videoSourcePlayer1.VideoSource = videoSource1;
                videoSourcePlayer1.Start();
            }
            
            if (videoDevices.Count > 1 && videoSource1 != null && videoSource2 == null)
            {
                Thread.Sleep(300);
                videoSource2 = new VideoCaptureDevice(videoDevices[1].MonikerString);
                //videoSource2.DesiredFrameRate = 10;
                int resolution_Index_2 = videoSource2.VideoCapabilities.Count();
                if (resolution_Index_2 >= res_Index)
                {
                    videoSource2.VideoResolution = videoSource2.VideoCapabilities[resolution_Index_2 - res_Index];
                    string resString = videoSource2.VideoResolution.FrameSize.Width.ToString() + " x " +
                                        videoSource2.VideoResolution.FrameSize.Height.ToString();

                    Invoke(updateString, 2, resString);
                }
                else
                    MessageBox.Show("Not enough items present in resolution list");

                videoSourcePlayer2.VideoSource = videoSource2;
                videoSourcePlayer2.Start();
            }
        }

        public void CloseCamera()
        {
            log.Debug("CloseCamera");
            //if (videoSource1 != null)
            if (videoSourcePlayer1.IsRunning)
            {
                videoSourcePlayer1.SignalToStop();
                videoSourcePlayer1.WaitForStop();
                
                //videoSource1.SignalToStop();
                //videoSource1.WaitForStop();
                //videoSource1 = null;
                //videoSource_1.SignalToStop();
                //videoSource_1.Stop();
                //videoSource_1 = null;
            }

            //if (videoSource2 != null)
            if (videoSourcePlayer2.IsRunning)
            {
                videoSourcePlayer2.SignalToStop();
                videoSourcePlayer2.WaitForStop();
                //videoSource2.SignalToStop();
                //videoSource2.Stop();
                //videoSource2.WaitForStop();
                //videoSource_2.SignalToStop();
                //videoSource_2.Stop();
            }

            if (bitmap != null)
                bitmap.Dispose();
        }

        private bool CreateSavingfolder(string deviceName)
        {
            log.Debug("CreateSavingfolder: " + deviceName);
            bool status = false;
            string picFolder = System.Environment.CurrentDirectory + "\\" + deviceName;

            if (Directory.Exists(picFolder))
            {
                status = true;
            }
            else
            {
                Directory.CreateDirectory(picFolder);
                status = true;
            }

            return status;
        }

        // ====== used along with SerialRxThread ====== //
        /*
        private void GetSerialData(Mod_RS232 SpHandler)
        {
            while (SpHandler.IsOpen())
            {
                int data_to_read = SpHandler.GetRxBytes();
                if (data_to_read > 0)
                {
                    byte[] dataset = new byte[data_to_read];
                    SpHandler.ReadDataIn(dataset, data_to_read);
                }
            }
        }

        public void SerialRxThread()
        {
            GetSerialData(GlobalData.m_SerialPort);
        }*/
		
		private static void SetMonitorFeatures(SafePhysicalMonitorHandle spmHandle, List<Tuple<byte, uint>> dicList)
        {
            foreach (var dList in dicList)
            {
                var featureCode = dList.Item1;
                var valueToBeSet = dList.Item2;
                uint possibleVal = 0, maxVal = 0;
                NativeMethods.MC_VCP_CODE_TYPE pvct;

                try
                {
                    if (!NativeMethods.SetVCPFeature(spmHandle, featureCode, valueToBeSet))
                        throw new InvalidOperationException($"{nameof(NativeMethods.SetVCPFeature)} returned error {Marshal.GetLastWin32Error()}");

                    log.Info($"[vcpCodeByte] {featureCode:X2}h [Value_Set] {valueToBeSet}");
                }
                catch
                {
                    log.Debug($" Cannot set the value for VCPCode {featureCode:X2}h!");   //{featureCode:X2} is simplified as {featureCode.ToString("X2")}
                }

                Task.Delay(2000).Wait();// Task.Delay(feature.Delay).Wait();

                try
                {
                    if (!NativeMethods.GetVCPFeatureAndVCPFeatureReply(spmHandle, featureCode, out pvct, out possibleVal, out maxVal))
                        throw new InvalidOperationException($"{nameof(NativeMethods.GetVCPFeatureAndVCPFeatureReply)} returned error {Marshal.GetLastWin32Error()}");

                    log.Info($"[vcpCodeByte] {featureCode:X2}h [Value_get] {possibleVal} [MaxValue] {maxVal}");
                }
                catch
                {
                    log.Debug("Cannot get the value for such vcpCode!");
                }
            }
        }
        private static void SetMonitorFeatures(SafePhysicalMonitorHandle spmHandle, byte featureCode, uint valueToBeSet)
        {
            uint possibleVal = 0, maxVal = 0;
            NativeMethods.MC_VCP_CODE_TYPE pvct;

            try
            {
                if (!NativeMethods.SetVCPFeature(spmHandle, featureCode, valueToBeSet))
                    throw new InvalidOperationException($"{nameof(NativeMethods.SetVCPFeature)} returned error {Marshal.GetLastWin32Error()}");

                log.Info($"[vcpCodeByte] {featureCode:X2}h [Value_Set] {valueToBeSet}");
            }
            catch
            {
                log.Debug($" Cannot set the value for VCPCode {featureCode:X2}h!");
                //Console.WriteLine($"VCP code {featureCode.ToString()} is not supported!");
            }

            Task.Delay(2000).Wait();
            
            try
            {
                if (!NativeMethods.GetVCPFeatureAndVCPFeatureReply(spmHandle, featureCode, out pvct, out possibleVal, out maxVal))
                    throw new InvalidOperationException($"{nameof(NativeMethods.GetVCPFeatureAndVCPFeatureReply)} returned error {Marshal.GetLastWin32Error()}");

                log.Info($"[vcpCodeByte] {featureCode:X2}h [Value_get] {possibleVal} [MaxValue] {maxVal}");
            }
            catch
            {
                log.Debug("Cannot get the value for such vcpCode!");
                //Console.WriteLine($"Something wrong when getting vcp values!");
            }
        }
        private static void SetMonitorFeatures(SafePhysicalMonitorHandle spmHandle, Dictionary<FeatureRange, uint> settings)
        {
            foreach (var setting in settings)
            {
                var feature = setting.Key;
                uint currentValue = 0, maxValue;
                uint newValue = setting.Value;
                NativeMethods.MC_VCP_CODE_TYPE pvct;

                try
                {
                    if (!NativeMethods.GetVCPFeatureAndVCPFeatureReply(spmHandle, feature.Code, out pvct, out currentValue, out maxValue))
                        throw new InvalidOperationException($"{nameof(NativeMethods.GetVCPFeatureAndVCPFeatureReply)} returned error {Marshal.GetLastWin32Error()}");

                    if (newValue == currentValue)
                        continue;
                }
                catch
                {
                    Console.WriteLine($"GetVCPFeature fault for {feature.Name}");
                }

                Console.WriteLine($"Update {feature.Name} {feature.ValueName(currentValue)}->{feature.ValueName(newValue)} (0x{feature.Code:X2} {currentValue}->{newValue})");

                if (!NativeMethods.SetVCPFeature(spmHandle, feature.Code, newValue))
                    throw new InvalidOperationException($"{nameof(NativeMethods.SetVCPFeature)} returned error {Marshal.GetLastWin32Error()}");

                Task.Delay(feature.Delay).Wait();
            }
        }
		
		//釋放記憶體//
        [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);
        private void DisposeRam()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

    }   //=== End of Main() ===//
}
