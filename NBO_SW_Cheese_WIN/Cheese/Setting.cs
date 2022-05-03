using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace Cheese
{
    public partial class Setting : Form
    {
        public int getComPortChecked()
        {
            if (ComportA_checkBox.Checked == true)
                return 1;
            else
                return 0;
        }
        public string getComPortSetting()
        {
            return ((string)this.comboBox_ComA_PortName.SelectedItem);
        }
        public string getComPortBaudRate()
        {
            return ((string)this.comboBox_ComA_BaudRate.SelectedItem);
        }
        public int getComPortDataLength()
        {
            int dataBits = Convert.ToInt32(this.comboBox_ComA_DataBits.SelectedItem);
            return dataBits;
        }
        public int getComPortDataBitsIndex()
        {
            return (this.comboBox_ComA_DataBits.SelectedIndex);
        }
        public int getComPortParity()
        {
            return (this.comboBox_ComA_Parity.SelectedIndex);
        }
        public int getComPortStopBits()
        {
            return (Convert.ToInt32((string)this.comboBox_ComA_StopBits.SelectedItem));
        }

        #region -- TCP/IP --
        public int getNetworkChecked()
        {
            if (Network_checkBox.Checked == true)
                return 1;
            else
                return 0;
        }
        public string getNetworkIP()
        {
            return ((string)this.textBox_IPAddr.Text);
        }
        public string getNetworkPort()
        {
            return ((string)this.textBox_PortNum.Text);
        }
        public int getNetworkTimeOut()
        {
            return (Convert.ToInt32((string)this.textBox_Timeout.Text));
        }
        #endregion
        public string getMailAddress()
        {
            return ((string)this.textBox_MailAddress.Text);
        }
        public int getCameraDevice()
        {
            return (this.cboCameraTypeList.SelectedIndex);
        }
        public int getCameraResolution()
        {
            return (this.cboResolutionList.SelectedIndex);
        }

        public Setting()
        {
            //
            string[] SerialPortName;
            int i;

            InitializeComponent();
            this.comboBox_ComA_PortName.Items.Clear();
            this.BTN_Connect.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.comboBox_ComA_BaudRate.SelectedIndex = 1;
            this.comboBox_ComA_DataBits.SelectedIndex = 3;
            this.comboBox_ComA_Parity.SelectedIndex = 0;
            this.comboBox_ComA_StopBits.SelectedIndex = 1;
            this.comboBox_ComA_Handshake.SelectedIndex = 0;

            this.comboBox_ComB_BaudRate.SelectedIndex = 5;
            this.comboBox_ComC_BaudRate.SelectedIndex = 5;
            this.comboBox_ComD_BaudRate.SelectedIndex = 5;
            this.comboBox_ComE_BaudRate.SelectedIndex = 1;

            SerialPortName = System.IO.Ports.SerialPort.GetPortNames();
            if (SerialPortName.Length >= 1)
            {
                for(i = 0; i < SerialPortName.Length; i++)
                {
                    this.comboBox_ComA_PortName.Items.Add(SerialPortName[i]);
                    this.comboBox_ComB_PortName.Items.Add(SerialPortName[i]);
                    this.comboBox_ComC_PortName.Items.Add(SerialPortName[i]);
                    this.comboBox_ComD_PortName.Items.Add(SerialPortName[i]);
                    this.comboBox_ComE_PortName.Items.Add(SerialPortName[i]);
                }
                this.comboBox_ComA_PortName.SelectedIndex = 0;
            }

            textBox_IPAddr.Enabled = false;
            textBox_PortNum.Enabled = false;
            textBox_Timeout.Enabled = false;
            textBox_MailAddress.Enabled = false;
            comboBox_ComA_PortName.Enabled = false;
            comboBox_ComA_BaudRate.Enabled = false;
            comboBox_ComA_Parity.Enabled = false;
            comboBox_ComA_DataBits.Enabled = false;
            comboBox_ComA_StopBits.Enabled = false;
            comboBox_ComA_Handshake.Enabled = false;

            comboBox_ComB_PortName.Enabled = false;
            comboBox_ComB_BaudRate.Enabled = false;

            comboBox_ComC_PortName.Enabled = false;
            comboBox_ComC_BaudRate.Enabled = false;

            comboBox_ComD_PortName.Enabled = false;
            comboBox_ComD_BaudRate.Enabled = false;

            comboBox_ComE_PortName.Enabled = false;
            comboBox_ComE_BaudRate.Enabled = false;
        }

        private void BTN_Connect_Click(object sender, EventArgs e)
        {
            if (ComportA_checkBox.Checked == true)
            {
                //SerialPort PortHandle = new SerialPort();
                string portName = getComPortSetting();
                int baudRate = Convert.ToInt32(getComPortBaudRate());
                int parity = getComPortParity();
                int dataLength = getComPortDataLength();
                int stopBit = getComPortStopBits();
                
                //PortHandle.PortName = (string)this.comboBox_Portname.SelectedItem;
                try
                {
                    if (!GlobalData.m_SerialPort_A.IsOpen())
                        GlobalData.m_SerialPort_A.OpenPort(portName, baudRate, parity, dataLength, stopBit);
                    /*
                    PortHandle.Open();
                    System.Threading.Thread.Sleep(1);
                    PortHandle.Close();
                    */
                }
                catch (Exception)
                {
                    MessageBox.Show((string)this.comboBox_ComA_PortName.SelectedItem + " is opened by other application.");
                }
            }

            if (ComportB_checkBox.Checked == true)
            {
                string portName = getComPortSetting();
                string baudRate = getComPortBaudRate();

                try
                {
                    if (!GlobalData.m_SerialPort_B.IsOpen())
                        GlobalData.m_SerialPort_B.OpenSerialPort(portName, baudRate);
                }
                catch (Exception)
                {
                    MessageBox.Show((string)this.comboBox_ComB_PortName.SelectedItem + " is opened by other application.");
                }
            }

            if (ComportC_checkBox.Checked == true)
            {
                string portName = getComPortSetting();
                string baudRate = getComPortBaudRate();

                try
                {
                    if (!GlobalData.m_SerialPort_C.IsOpen())
                        GlobalData.m_SerialPort_C.OpenSerialPort(portName, baudRate);
                }
                catch (Exception)
                {
                    MessageBox.Show((string)this.comboBox_ComC_PortName.SelectedItem + " is opened by other application.");
                }
            }

            if (ComportD_checkBox.Checked == true)
            {
                string portName = getComPortSetting();
                string baudRate = getComPortBaudRate();

                try
                {
                    if (!GlobalData.m_SerialPort_D.IsOpen())
                        GlobalData.m_SerialPort_D.OpenSerialPort(portName, baudRate);
                }
                catch (Exception)
                {
                    MessageBox.Show((string)this.comboBox_ComD_PortName.SelectedItem + " is opened by other application.");
                }
            }

            if (ComportE_checkBox.Checked == true)
            {
                string portName = getComPortSetting();
                string baudRate = getComPortBaudRate();

                try
                {
                    if (!GlobalData.m_SerialPort_E.IsOpen())
                        GlobalData.m_SerialPort_E.OpenSerialPort(portName, baudRate);
                }
                catch (Exception)
                {
                    MessageBox.Show((string)this.comboBox_ComE_PortName.SelectedItem + " is opened by other application.");
                }
            }
        }

        private void Network_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Network_checkBox.Checked == true)
            {
                textBox_IPAddr.Enabled = true;
                textBox_PortNum.Enabled = true;
                textBox_Timeout.Enabled = true;
                textBox_MailAddress.Enabled = true;
            }
            else
            {
                textBox_IPAddr.Enabled = false;
                textBox_PortNum.Enabled = false;
                textBox_Timeout.Enabled = false;
                textBox_MailAddress.Enabled = false;
            }
        }

        private void Comport_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            // ======= Com-A ======= //
            if (ComportA_checkBox.Checked == true)
            {
                comboBox_ComA_PortName.Enabled = true;
                comboBox_ComA_BaudRate.Enabled = true;
                comboBox_ComA_Parity.Enabled = true;
                comboBox_ComA_DataBits.Enabled = true;
                comboBox_ComA_StopBits.Enabled = true;
                comboBox_ComA_Handshake.Enabled = true;
            }
            else
            {
                comboBox_ComA_PortName.Enabled = false;
                comboBox_ComA_BaudRate.Enabled = false;
                comboBox_ComA_Parity.Enabled = false;
                comboBox_ComA_DataBits.Enabled = false;
                comboBox_ComA_StopBits.Enabled = false;
                comboBox_ComA_Handshake.Enabled = false;
            }
            // ======= Com-B ======= //
            if (ComportB_checkBox.Checked == true)
            {
                comboBox_ComB_PortName.Enabled = true;
                comboBox_ComB_BaudRate.Enabled = true;
            }
            else
            {
                comboBox_ComB_PortName.Enabled = false;
                comboBox_ComB_BaudRate.Enabled = false;
            }
            // ======= Com-C ======= //
            if (ComportC_checkBox.Checked == true)
            {
                comboBox_ComC_PortName.Enabled = true;
                comboBox_ComC_BaudRate.Enabled = true;
            }
            else
            {
                comboBox_ComC_PortName.Enabled = false;
                comboBox_ComC_BaudRate.Enabled = false;
            }
            // ======= Com-D ======= //
            if (ComportD_checkBox.Checked == true)
            {
                comboBox_ComD_PortName.Enabled = true;
                comboBox_ComD_BaudRate.Enabled = true;
            }
            else
            {
                comboBox_ComD_PortName.Enabled = false;
                comboBox_ComD_BaudRate.Enabled = false;
            }
            // ======= Com-E ======= //
            if (ComportE_checkBox.Checked == true)
            {
                comboBox_ComE_PortName.Enabled = true;
                comboBox_ComE_BaudRate.Enabled = true;
            }
            else
            {
                comboBox_ComE_PortName.Enabled = false;
                comboBox_ComE_BaudRate.Enabled = false;
            }
        }

        private void Save_Comport_ParameterValues()
        {
            this.comboBox_ComA_BaudRate.SelectedIndex = 1;
            this.comboBox_ComA_DataBits.SelectedIndex = 3;
            this.comboBox_ComA_Parity.SelectedIndex = 0;
            this.comboBox_ComA_StopBits.SelectedIndex = 0;
            this.comboBox_ComA_Handshake.SelectedIndex = 0;
            /*
            GlobalData.string_PortA_BaudRate = this.comboBox_ComA_BaudRate.SelectedItem.ToString();
            GlobalData.string_PortA_DataBits = this.comboBox_ComA_DataBits.SelectedItem.ToString();
            GlobalData.string_PortA_Parity = this.comboBox_ComA_Parity.SelectedItem.ToString();
            GlobalData.string_PortA_StopBits = this.comboBox_ComA_Parity.SelectedItem.ToString();
            GlobalData.value_PortA_BaudRate = Convert.ToInt32(this.comboBox_ComA_BaudRate.SelectedItem);
            GlobalData.value_PortA_DataBits = Convert.ToInt32(this.comboBox_ComA_DataBits.SelectedItem);
            GlobalData.id_PortA_Parity = this.comboBox_ComA_Parity.SelectedIndex;
            GlobalData.id_PortA_StopBits = this.comboBox_ComA_StopBits.SelectedIndex;
            */
        }
    }
}
