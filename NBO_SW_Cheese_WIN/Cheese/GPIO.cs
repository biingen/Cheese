using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using jini;

namespace Cheese
{
    public partial class GPIO : Form
    {
        private string button1_down = "";
        private string button1_up = "";
        private string button2_down = "";
        private string button2_up = "";
        private string button3_down = "";
        private string button3_up = "";
        private string button4_down = "";
        private string button4_up = "";
        private string button5_down = "";
        private string button5_up = "";
        private string button6_down = "";
        private string button6_up = "";
        private string button7_down = "";
        private string button7_up = "";
        private string button8_down = "";
        private string button8_up = "";
        private string button9_down = "";
        private string button9_up = "";
        private string button10_down = "";
        private string button10_up = "";
        private string button11_down = "";
        private string button11_up = "";
        private string button12_down = "";
        private string button12_up = "";
        private bool oneshot_mode = false;

        public GPIO()
        {
            InitializeComponent();
        }

        private void GPIO_Shown(object sender, EventArgs e)
        {
            GlobalData.GPIO = true;
        }

        private void GPIO_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalData.GPIO = false;
        }

        //判斷TextBox輸入值是否為數字
        public static bool IsNumeric(string TextBoxValue)
        {
            try
            {
                int i = Convert.ToInt16(TextBoxValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void LoadGpioDB(string xmlfile)
        {
            try
            {
                // Gpio指令檔案匯入
                if (System.IO.File.Exists(xmlfile) == true)
                {
                    var allGPIO = XDocument.Load(xmlfile).Root.Element("Keypad_Setting").Elements("GPIO");
                    foreach (var GPIOCode in allGPIO)
                    {
                        switch (GPIOCode.Attribute("Name").Value)
                        {
                            case "GPIO01":
                                button1.Text = GPIOCode.Element("GPIO_N").Value;
                                button1_down = GPIOCode.Element("GPIO_D").Value;
                                button1_up = GPIOCode.Element("GPIO_U").Value;
                                button1.MouseDown += new MouseEventHandler(button_MouseDown);
                                button1.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO02":// "GPIO02":
                                button2.Text = GPIOCode.Element("GPIO_N").Value;
                                button2_down = GPIOCode.Element("GPIO_D").Value;
                                button2_up = GPIOCode.Element("GPIO_U").Value;
                                button2.MouseDown += new MouseEventHandler(button_MouseDown);
                                button2.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO03":
                                button3.Text = GPIOCode.Element("GPIO_N").Value;
                                button3_down = GPIOCode.Element("GPIO_D").Value;
                                button3_up = GPIOCode.Element("GPIO_U").Value;
                                button3.MouseDown += new MouseEventHandler(button_MouseDown);
                                button3.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO04":// "GPIO04":
                                button4.Text = GPIOCode.Element("GPIO_N").Value;
                                button4_down = GPIOCode.Element("GPIO_D").Value;
                                button4_up = GPIOCode.Element("GPIO_U").Value;
                                button4.MouseDown += new MouseEventHandler(button_MouseDown);
                                button4.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO05":// "GPIO05":
                                button5.Text = GPIOCode.Element("GPIO_N").Value;
                                button5_down = GPIOCode.Element("GPIO_D").Value;
                                button5_up = GPIOCode.Element("GPIO_U").Value;
                                button5.MouseDown += new MouseEventHandler(button_MouseDown);
                                button5.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO06":// "GPIO06":
                                button6.Text = GPIOCode.Element("GPIO_N").Value;
                                button6_down = GPIOCode.Element("GPIO_D").Value;
                                button6_up = GPIOCode.Element("GPIO_U").Value;
                                button6.MouseDown += new MouseEventHandler(button_MouseDown);
                                button6.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO07":
                                button7.Text = GPIOCode.Element("GPIO_N").Value;
                                button7_down = GPIOCode.Element("GPIO_D").Value;
                                button7_up = GPIOCode.Element("GPIO_U").Value;
                                button7.MouseDown += new MouseEventHandler(button_MouseDown);
                                button7.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO08":// "GPIO08":
                                button8.Text = GPIOCode.Element("GPIO_N").Value;
                                button8_down = GPIOCode.Element("GPIO_D").Value;
                                button8_up = GPIOCode.Element("GPIO_U").Value;
                                button8.MouseDown += new MouseEventHandler(button_MouseDown);
                                button8.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO09":
                                button9.Text = GPIOCode.Element("GPIO_N").Value;
                                button9_down = GPIOCode.Element("GPIO_D").Value;
                                button9_up = GPIOCode.Element("GPIO_U").Value;
                                button9.MouseDown += new MouseEventHandler(button_MouseDown);
                                button9.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO10":
                                button10.Text = GPIOCode.Element("GPIO_N").Value;
                                button10_down = GPIOCode.Element("GPIO_D").Value;
                                button10_up = GPIOCode.Element("GPIO_U").Value;
                                button10.MouseDown += new MouseEventHandler(button_MouseDown);
                                button10.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO11":
                                button11.Text = GPIOCode.Element("GPIO_N").Value;
                                button11_down = GPIOCode.Element("GPIO_D").Value;
                                button11_up = GPIOCode.Element("GPIO_U").Value;
                                button11.MouseDown += new MouseEventHandler(button_MouseDown);
                                button11.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                            case "GPIO12":
                                button12.Text = GPIOCode.Element("GPIO_N").Value;
                                button12_down = GPIOCode.Element("GPIO_D").Value;
                                button12_up = GPIOCode.Element("GPIO_U").Value;
                                button12.MouseDown += new MouseEventHandler(button_MouseDown);
                                button12.MouseUp += new MouseEventHandler(button_MouseUp);
                                break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("GPIO code file does not exist", "File Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message.ToString(), "Keypad_GPIO library error!");
            }
        }

        private void button_MouseDown(object sender, EventArgs e)
        {
            int index = int.Parse(((Button)(sender)).Name.ToString().Replace("button", ""));
            if (GlobalData.sp_Arduino.IsOpen())
            {
                switch (index)
                {
                    case 1:
                        GlobalData.sp_Arduino.WriteDataOut(button1_down, button1_down.Length);
                        break;
                    case 2:
                        GlobalData.sp_Arduino.WriteDataOut(button2_down, button2_down.Length);
                        break;
                    case 3:
                        GlobalData.sp_Arduino.WriteDataOut(button3_down, button3_down.Length);
                        break;
                    case 4:
                        GlobalData.sp_Arduino.WriteDataOut(button4_down, button4_down.Length);
                        break;
                    case 5:
                        GlobalData.sp_Arduino.WriteDataOut(button5_down, button5_down.Length);
                        break;
                    case 6:
                        GlobalData.sp_Arduino.WriteDataOut(button6_down, button6_down.Length);
                        break;
                    case 7:
                        GlobalData.sp_Arduino.WriteDataOut(button7_down, button7_down.Length);
                        break;
                    case 8:
                        GlobalData.sp_Arduino.WriteDataOut(button8_down, button8_down.Length);
                        break;
                    case 9:
                        GlobalData.sp_Arduino.WriteDataOut(button9_down, button9_down.Length);
                        break;
                    case 10:
                        GlobalData.sp_Arduino.WriteDataOut(button10_down, button10_down.Length);
                        break;
                    case 11:
                        GlobalData.sp_Arduino.WriteDataOut(button11_down, button11_down.Length);
                        break;
                    case 12:
                        GlobalData.sp_Arduino.WriteDataOut(button12_down, button12_down.Length);
                        break;
                }
            }
        }

        private void button_MouseUp(object sender, EventArgs e)
        {
            int index = int.Parse(((Button)(sender)).Name.ToString().Replace("button", ""));
            if (GlobalData.sp_Arduino.IsOpen())
            {
                switch (index)
                {
                    case 1:
                        GlobalData.sp_Arduino.WriteDataOut(button1_up, button1_up.Length);
                        break;
                    case 2:
                        GlobalData.sp_Arduino.WriteDataOut(button2_up, button2_up.Length);
                        break;
                    case 3:
                        GlobalData.sp_Arduino.WriteDataOut(button3_up, button3_up.Length);
                        break;
                    case 4:
                        GlobalData.sp_Arduino.WriteDataOut(button4_up, button4_up.Length);
                        break;
                    case 5:
                        GlobalData.sp_Arduino.WriteDataOut(button5_up, button5_up.Length);
                        break;
                    case 6:
                        GlobalData.sp_Arduino.WriteDataOut(button6_up, button6_up.Length);
                        break;
                    case 7:
                        GlobalData.sp_Arduino.WriteDataOut(button7_up, button7_up.Length);
                        break;
                    case 8:
                        GlobalData.sp_Arduino.WriteDataOut(button8_up, button8_up.Length);
                        break;
                    case 9:
                        GlobalData.sp_Arduino.WriteDataOut(button9_up, button9_up.Length);
                        break;
                    case 10:
                        GlobalData.sp_Arduino.WriteDataOut(button10_up, button10_up.Length);
                        break;
                    case 11:
                        GlobalData.sp_Arduino.WriteDataOut(button11_up, button11_up.Length);
                        break;
                    case 12:
                        GlobalData.sp_Arduino.WriteDataOut(button12_up, button12_up.Length);
                        break;
                }
            }
        }

        private void button_XmlFile_Click(object sender, EventArgs e)
        {
            // Generator Command Path
            openFileDialog1.Filter = "XML files (*.xml)|*.xml";
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName == "")
            {
                textBox_XmlPath.Text = textBox_XmlPath.Text;
            }
            else
            {
                textBox_XmlPath.Text = openFileDialog1.FileName;
                LoadGpioDB(textBox_XmlPath.Text);
            }
        }

        private void button_oneshotMode_Click(object sender, EventArgs e)
        {
            string oneshot_command = "";
            if (oneshot_mode)
            {
                oneshot_mode = false;
                oneshot_command = "s o s 0";
                button_oneshotMode.Text = "High";
            }
            else
            {
                oneshot_mode = true;
                oneshot_command = "s o s 1";
                button_oneshotMode.Text = "Low";
            }
            if (GlobalData.sp_Arduino.IsOpen())
                GlobalData.sp_Arduino.WriteDataOut(oneshot_command, oneshot_command.Length);
        }

        private void button_oneshotSet_Click(object sender, EventArgs e)
        {
            byte oneshot_GPIO = 0x00;
            ushort oneshot_delay = 0x00;
            string oneshot_command = "";
            if (IsNumeric(textBox_GPIOnumber.Text) && IsNumeric(textBox_oneshotdelay.Text) && GlobalData.sp_Arduino.IsOpen())
            {
                oneshot_GPIO = Convert.ToByte(textBox_GPIOnumber.Text);
                oneshot_delay = Convert.ToUInt16(textBox_oneshotdelay.Text);
                oneshot_command = "io s " + oneshot_GPIO + " t " + oneshot_delay;
                GlobalData.sp_Arduino.WriteDataOut(oneshot_command, oneshot_command.Length);
            }
        }
    }
}
