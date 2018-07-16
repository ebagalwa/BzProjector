using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TagLib;
using System.Globalization;


namespace BzProjector
{
    //The commands for interaction between the server and the client
    enum Command
    {
        Login,      //Log into the server
        Logout,     //Logout of the server
        Message,    //Send a text message to all the chat clients
        List,       //Get a list of users in the chat room from the server
        Null        //No command
    }

    public partial class Form1 : Form
    {
        Form2 frm = new Form2();
        Screen[] screens = Screen.AllScreens;
        // Get Monitor Count
        int iMonitorCount = Screen.AllScreens.Length;
        Rectangle rectMonitor;
        string rootpath = System.IO.Path.GetDirectoryName(Application.StartupPath);
        int currentVol;
        int currentAction = 0;
        int currentSong = 0;
        int random = 0;
        int netwrk = 0;
        string server = "";
        string[] lines;
        string mediapath = "";
        string[,] langa = new string[27, 4];

        //Network
        public Socket clientSocket;
        public EndPoint epServer;
        public string strName;
        byte[] byteData = new byte[1024];
        List<string> lstChatters = new List<string>();
        Data msgToSend = new Data();
        List<string> extraMedia = new List<string>();
        int netPlay = 0;
        string netMedia = "CANT";
        string netCMD = "PLAY";
        string netID = "001";
        long netcmdTime = 0;
        long netAlive = 0;
        int netState = 0;
        int netPause = 0;

        public Form1()
        {
            InitializeComponent();
            frm.axWindowsMediaPlayer1.Visible = false;
            trackBar1.Value = (frm.axWindowsMediaPlayer1.settings.volume / 10);
            currentVol = trackBar1.Value;
            frm.pictureBox1.Visible = false;
            frm.pnlTitle.Visible = false;
        }
       
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Screen[] screens = Screen.AllScreens;
            if (checkBox1.Checked)
            {

                if (iMonitorCount > 1) // If monitor Count 2 or more
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[1].WorkingArea;
                }
                else
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[0].WorkingArea;
                }
                //frm.WindowState = FormWindowState.Maximized;
                frm.Show();
                frm.Location = new Point(rectMonitor.X, rectMonitor.Y);
                frm.WindowState = FormWindowState.Maximized;
            }
            else
            {
                frm.Hide();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(netwrk == 0)
            { 
                currentAction = 1;
                label10.Text = "";
                frm.axWindowsMediaPlayer1.Visible = false;
                frm.pictureBox1.Visible = false;
                frm.pnlTitle.Visible = false;
                checkBox3.Checked = false;
                checkBox5.Checked = false;
                random = 0;
                if (iMonitorCount > 1) // If monitor Count 2 or more
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[1].WorkingArea;
                }
                else
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[0].WorkingArea;
                }
                frm.Show();
                frm.axWindowsMediaPlayer1.URL = lines[0] + @"\sjjm_F_" + comboBox1.Text + "_r720P.mp4";
                var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);
                TagLib.IPicture pic = file.Tag.Pictures[0];
                MemoryStream stream = new MemoryStream(pic.Data.Data);
                var image = Image.FromStream(stream);
                pictureBox1.Image = image;
                //pictureBox1.ImageLocation = @"C:\Users\Adminstrator-Tek\Documents\JW\Cantiques\sjjm_F_" + comboBox1.Text + "_r720P.mp4";
                //frm.axWindowsMediaPlayer1.Visible = true;
                frm.Location = new Point(rectMonitor.X, rectMonitor.Y);
                frm.WindowState = FormWindowState.Maximized;
                frm.axWindowsMediaPlayer1.uiMode = "none";
                frm.axWindowsMediaPlayer1.Top = 0;
                frm.axWindowsMediaPlayer1.Left = 0;
                frm.axWindowsMediaPlayer1.Width = frm.Width;
                frm.axWindowsMediaPlayer1.Height = frm.Height;
                frm.axWindowsMediaPlayer1.stretchToFit = true;
                checkBox1.Checked = true;
                currentAction = 1;
                frm.pictureBox1.Visible = false;
                frm.axWindowsMediaPlayer1.Visible = true;
            }
            else if (netwrk == 1)
            {
                try
                {
                    //Fill the info for the message to be send
                    Data msgToSend = new Data();

                    msgToSend.strName = strName;
                    msgToSend.strMessage = "CANT,PLAY," + comboBox1.Text + "," + DateTime.Now.Ticks;
                    msgToSend.cmdCommand = Command.Message;

                    byte[] byteData = msgToSend.ToByte();

                    //Send it to the server
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to send message to the server.", "SGSclientUDP: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (netwrk == 0)
            {
                frm.axWindowsMediaPlayer1.Ctlcontrols.stop();
                frm.axWindowsMediaPlayer1.Visible = false;
                random = 0;
            }
            else if (netwrk == 1)
            {
                try
                {
                    //Fill the info for the message to be send
                    Data msgToSend = new Data();

                    msgToSend.strName = strName;
                    msgToSend.strMessage = "CANT,STOP," + comboBox1.Text + "," + DateTime.Now.Ticks;
                    msgToSend.cmdCommand = Command.Message;

                    byte[] byteData = msgToSend.ToByte();

                    //Send it to the server
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to send message to the server.", "SGSclientUDP: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if(netwrk == 0)
            {
                if (frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying) frm.axWindowsMediaPlayer1.Ctlcontrols.pause();
                else frm.axWindowsMediaPlayer1.Ctlcontrols.play();
                frm.axWindowsMediaPlayer1.uiMode = "none";
            }
            else if(netwrk == 1)
            {
                try
                {
                    //Fill the info for the message to be send
                    Data msgToSend = new Data();

                    msgToSend.strName = strName;
                    msgToSend.strMessage = "CANT,PAUSE," + comboBox1.Text + "," + DateTime.Now.Ticks;
                    msgToSend.cmdCommand = Command.Message;

                    byte[] byteData = msgToSend.ToByte();

                    //Send it to the server
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to send message to the server.", "SGSclientUDP: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            frm.axWindowsMediaPlayer1.settings.volume = trackBar1.Value * 10;
            currentVol = trackBar1.Value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName.ToString();
            var file = TagLib.File.Create(textBox1.Text);
            label10.Text = "";
            try
            {
                TagLib.IPicture pic = file.Tag.Pictures[0];
                MemoryStream stream = new MemoryStream(pic.Data.Data);
                var image = Image.FromStream(stream);
                pictureBox2.Image = image;
            }
            catch (Exception ev)
            {
                label10.Text = ">> No album art! " + ev.Message;
                pictureBox2.Image = Properties.Resources.video_generic;
            }
            button3.Visible = true;
            button14.Visible = true;
            button9.Visible = true;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(netwrk == 0)
            {
                label10.Text = "";
                frm.axWindowsMediaPlayer1.Visible = false;
                frm.pictureBox1.Visible = false;
                frm.pnlTitle.Visible = false;
                checkBox3.Checked = false;
                checkBox5.Checked = false;
                random = 0;
                if (iMonitorCount > 1) // If monitor Count 2 or more
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[1].WorkingArea;
                }
                else
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[0].WorkingArea;
                }
                frm.Show();
                frm.axWindowsMediaPlayer1.URL = textBox1.Text;
                var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);
                label10.Text = "";
                try
                {
                    TagLib.IPicture pic = file.Tag.Pictures[0];
                    MemoryStream stream = new MemoryStream(pic.Data.Data);
                    var image = Image.FromStream(stream);
                    pictureBox2.Image = image;
                }
                catch (Exception ev)
                {
                    label10.Text = ">> No album art! " + ev.Message;
                    pictureBox2.Image = Properties.Resources.video_generic;
                }
                //frm.axWindowsMediaPlayer1.Visible = true;
                frm.Location = new Point(rectMonitor.X, rectMonitor.Y);
                frm.WindowState = FormWindowState.Maximized;
                frm.axWindowsMediaPlayer1.uiMode = "none";
                frm.axWindowsMediaPlayer1.Top = 0;
                frm.axWindowsMediaPlayer1.Left = 0;
                frm.axWindowsMediaPlayer1.Width = frm.Width;
                frm.axWindowsMediaPlayer1.Height = frm.Height;
                frm.axWindowsMediaPlayer1.stretchToFit = true;
                checkBox1.Checked = true;
                currentAction = 2;
                frm.axWindowsMediaPlayer1.Visible = true;
            }
            else if (netwrk == 1)
            {
                try
                {
                    //Fill the info for the message to be send
                    Data msgToSend = new Data();

                    msgToSend.strName = strName;
                    msgToSend.strMessage = "VID,PLAY," + "play_video" + "," + DateTime.Now.Ticks;
                    msgToSend.cmdCommand = Command.Message;

                    byte[] byteData = msgToSend.ToByte();

                    //Send it to the server
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to send message to the server.", "SGSclientUDP: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (netwrk == 0)
            {
                if (frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying) frm.axWindowsMediaPlayer1.Ctlcontrols.pause();
                else frm.axWindowsMediaPlayer1.Ctlcontrols.play();
                frm.axWindowsMediaPlayer1.uiMode = "none";
            }
            else if (netwrk == 1)
            {
                try
                {
                    //Fill the info for the message to be send
                    Data msgToSend = new Data();

                    msgToSend.strName = strName;
                    msgToSend.strMessage = "VID,PAUSE," + "play_video" + "," + DateTime.Now.Ticks;
                    msgToSend.cmdCommand = Command.Message;

                    byte[] byteData = msgToSend.ToByte();

                    //Send it to the server
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to send message to the server.", "SGSclientUDP: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if(netwrk == 0)
            {
                frm.axWindowsMediaPlayer1.Ctlcontrols.stop();
                frm.axWindowsMediaPlayer1.Visible = false;
            }
            else if(netwrk == 1)
            {
                try
                {
                    //Fill the info for the message to be send
                    Data msgToSend = new Data();

                    msgToSend.strName = strName;
                    msgToSend.strMessage = "VID,STOP," + "play_video" + "," + DateTime.Now.Ticks;
                    msgToSend.cmdCommand = Command.Message;

                    byte[] byteData = msgToSend.ToByte();

                    //Send it to the server
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to send message to the server.", "SGSclientUDP: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox2.Checked) trackBar1.Value = currentVol;
            else trackBar1.Value = 0;
            frm.axWindowsMediaPlayer1.settings.volume = trackBar1.Value * 10;
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox3.Checked)
            {
                frm.axWindowsMediaPlayer1.Visible = false;
                frm.pictureBox1.Visible = false;
                frm.pnlTitle.Visible = false;
            }
            else
            {
                if(currentAction == 1 || currentAction == 2)frm.axWindowsMediaPlayer1.Visible = true;
                if (currentAction == 3) frm.pictureBox1.Visible = true;
                if(currentAction == 4) frm.pnlTitle.Visible = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = frm.axWindowsMediaPlayer1.status.ToString();
            if (frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                toolStripStatusLabel1.Text = frm.axWindowsMediaPlayer1.Ctlcontrols.currentItem.getItemInfo("Title");
                toolStripProgressBar1.Maximum = Convert.ToInt32(frm.axWindowsMediaPlayer1.currentMedia.duration);
                if (Convert.ToInt32(frm.axWindowsMediaPlayer1.Ctlcontrols.currentPosition) <= toolStripProgressBar1.Maximum) toolStripProgressBar1.Value = Convert.ToInt32(frm.axWindowsMediaPlayer1.Ctlcontrols.currentPosition);
                toolStripStatusLabel2.Text = Convert.ToInt32(frm.axWindowsMediaPlayer1.Ctlcontrols.currentPosition).ToString() + "/" + Convert.ToInt32(frm.axWindowsMediaPlayer1.currentMedia.duration).ToString();
                if (random == 1 && (Convert.ToInt32(frm.axWindowsMediaPlayer1.Ctlcontrols.currentPosition) > Convert.ToInt32(frm.axWindowsMediaPlayer1.currentMedia.duration) - 6))
                {
                    Random randomk = new Random();
                    int randomNumber = randomk.Next(1, 152);
                    string rn = "";
                    if (randomNumber < 10) rn = "00" + randomNumber;
                    else if (randomNumber < 100) rn = "0" + randomNumber;
                    else rn = "" + randomNumber;
                    label10.Text = "";
                    try
                    {
                        frm.axWindowsMediaPlayer1.URL = lines[0] + @"\sjjm_F_" + rn + "_r720P.mp4";
                        try
                        {
                            var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);
                            TagLib.IPicture pic = file.Tag.Pictures[0];
                            MemoryStream stream = new MemoryStream(pic.Data.Data);
                            var image = Image.FromStream(stream);
                            pictureBox1.Image = image;
                        }
                        catch (Exception ev)
                        {
                            label10.Text = ">> No album art! " + ev.Message;
                            pictureBox1.Image = Properties.Resources._default;
                        }
                    }
                    catch (Exception ev)
                    {
                        label10.Text = ">> " + ev.Message;
                    }
                }
                if (currentAction == 5 && (Convert.ToInt32(frm.axWindowsMediaPlayer1.Ctlcontrols.currentPosition) > Convert.ToInt32(frm.axWindowsMediaPlayer1.currentMedia.duration) - 6))
                {
                    currentSong++;
                    if (currentSong >= extraMedia.Count)
                    {
                        currentSong = 0;
                    }
                    label10.Text = "";
                    try
                    {
                        frm.axWindowsMediaPlayer1.URL = extraMedia[currentSong];
                        try
                        {
                            var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);
                            TagLib.IPicture pic = file.Tag.Pictures[0];
                            MemoryStream stream = new MemoryStream(pic.Data.Data);
                            var image = Image.FromStream(stream);
                            pictureBox1.Image = image;
                        }
                        catch (Exception ev)
                        {
                            label10.Text = ">> No album art! " + ev.Message;
                            pictureBox1.Image = Properties.Resources._default;
                        }
                    }
                    catch (Exception ev)
                    {
                        label10.Text = ">> " + ev.Message;
                    }
                }
            }
            if(netPause == 1 && frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                netPause = 0;
                frm.axWindowsMediaPlayer1.Ctlcontrols.pause();
                netState = 1;
            }
            if(netwrk == 1 && DateTime.Now >= new DateTime(netAlive).AddSeconds(15))
            {
                try
                {
                    //Fill the info for the message to be send
                    Data msgToSend = new Data();

                    msgToSend.strName = strName;
                    msgToSend.strMessage = "I am alive!";
                    msgToSend.cmdCommand = Command.Message;

                    byte[] byteData = msgToSend.ToByte();

                    //Send it to the server
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

                    textBox8.Text = null;
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to send message to the server.", "SGSclientUDP: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                netAlive = DateTime.Now.Ticks;
            }
            if (netState == 1 && DateTime.Now >= new DateTime(netcmdTime).AddMilliseconds(1380))
            {
                textBox7.Text += "\n" + netcmdTime;
                netState = 0;
                frm.axWindowsMediaPlayer1.Ctlcontrols.play();
            }
            if (netState == 2 && DateTime.Now >= new DateTime(netcmdTime).AddMilliseconds(1380))
            {
                textBox7.Text += "\n" + netcmdTime;
                netState = 0;
                frm.axWindowsMediaPlayer1.Ctlcontrols.pause();
            }
            if (netPlay == 1)
            {
                netPlay = 0;
                if(netMedia == "CANT" && netCMD == "PLAY")
                {
                    currentAction = 1;
                    label10.Text = "";
                    frm.axWindowsMediaPlayer1.Visible = false;
                    frm.pictureBox1.Visible = false;
                    frm.pnlTitle.Visible = false;
                    checkBox3.Checked = false;
                    checkBox5.Checked = false;
                    random = 0;
                    if (iMonitorCount > 1) // If monitor Count 2 or more
                    {
                        //Get the Dimension of the monitor
                        rectMonitor = Screen.AllScreens[1].WorkingArea;
                    }
                    else
                    {
                        //Get the Dimension of the monitor
                        rectMonitor = Screen.AllScreens[0].WorkingArea;
                    }
                    frm.Show();
                    frm.axWindowsMediaPlayer1.URL = lines[0] + @"\sjjm_F_" + netID + "_r720P.mp4";
                    var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);
                    TagLib.IPicture pic = file.Tag.Pictures[0];
                    MemoryStream stream = new MemoryStream(pic.Data.Data);
                    var image = Image.FromStream(stream);
                    pictureBox1.Image = image;
                    //pictureBox1.ImageLocation = @"C:\Users\Adminstrator-Tek\Documents\JW\Cantiques\sjjm_F_" + comboBox1.Text + "_r720P.mp4";
                    //frm.axWindowsMediaPlayer1.Visible = true;
                    frm.Location = new Point(rectMonitor.X, rectMonitor.Y);
                    frm.WindowState = FormWindowState.Maximized;
                    frm.axWindowsMediaPlayer1.uiMode = "none";
                    frm.axWindowsMediaPlayer1.Top = 0;
                    frm.axWindowsMediaPlayer1.Left = 0;
                    frm.axWindowsMediaPlayer1.Width = frm.Width;
                    frm.axWindowsMediaPlayer1.Height = frm.Height;
                    frm.axWindowsMediaPlayer1.stretchToFit = true;
                    checkBox1.Checked = true;
                    currentAction = 1;
                    frm.pictureBox1.Visible = false;
                    frm.axWindowsMediaPlayer1.Visible = true;
                    netPause = 1;
                }
                else if(netMedia == "CANT" && netCMD == "PAUSE")
                {
                    if (frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        netState = 2;
                    }
                    else
                    {
                        netState = 1;
                    }
                    frm.axWindowsMediaPlayer1.uiMode = "none";
                }
                else if(netMedia == "CANT" && netCMD == "STOP")
                {
                    frm.axWindowsMediaPlayer1.Ctlcontrols.stop();
                    frm.axWindowsMediaPlayer1.Visible = false;
                    random = 0;
                }
                else if(netMedia == "VID" && netCMD == "PLAY")
                {
                    label10.Text = "";
                    frm.axWindowsMediaPlayer1.Visible = false;
                    frm.pictureBox1.Visible = false;
                    frm.pnlTitle.Visible = false;
                    checkBox3.Checked = false;
                    checkBox5.Checked = false;
                    random = 0;
                    if (iMonitorCount > 1) // If monitor Count 2 or more
                    {
                        //Get the Dimension of the monitor
                        rectMonitor = Screen.AllScreens[1].WorkingArea;
                    }
                    else
                    {
                        //Get the Dimension of the monitor
                        rectMonitor = Screen.AllScreens[0].WorkingArea;
                    }
                    frm.Show();
                    frm.axWindowsMediaPlayer1.URL = textBox1.Text;
                    var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);
                    label10.Text = "";
                    try
                    {
                        TagLib.IPicture pic = file.Tag.Pictures[0];
                        MemoryStream stream = new MemoryStream(pic.Data.Data);
                        var image = Image.FromStream(stream);
                        pictureBox2.Image = image;
                    }
                    catch (Exception ev)
                    {
                        label10.Text = ">> No album art! " + ev.Message;
                        pictureBox2.Image = Properties.Resources.video_generic;
                    }
                    //frm.axWindowsMediaPlayer1.Visible = true;
                    frm.Location = new Point(rectMonitor.X, rectMonitor.Y);
                    frm.WindowState = FormWindowState.Maximized;
                    frm.axWindowsMediaPlayer1.uiMode = "none";
                    frm.axWindowsMediaPlayer1.Top = 0;
                    frm.axWindowsMediaPlayer1.Left = 0;
                    frm.axWindowsMediaPlayer1.Width = frm.Width;
                    frm.axWindowsMediaPlayer1.Height = frm.Height;
                    frm.axWindowsMediaPlayer1.stretchToFit = true;
                    checkBox1.Checked = true;
                    currentAction = 2;
                    frm.axWindowsMediaPlayer1.Visible = true;
                    frm.axWindowsMediaPlayer1.Ctlcontrols.pause();
                    netPause = 1;
                }
                else if(netMedia == "VID" && netCMD == "PAUSE")
                {
                    if (frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        netState = 2;
                    }
                    else
                    {
                        netState = 1;
                    }
                    frm.axWindowsMediaPlayer1.uiMode = "none";
                }
                else if(netMedia == "VID" && netCMD == "STOP")
                {
                    frm.axWindowsMediaPlayer1.Ctlcontrols.stop();
                    frm.axWindowsMediaPlayer1.Visible = false;
                }
            }
            //if (frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying) toolStripProgressBar1.Value = Convert.ToInt32(frm.axWindowsMediaPlayer1.Ctlcontrols.currentPosition * (frm.axWindowsMediaPlayer1.currentMedia.duration/100));
            statusStrip1.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            frm.axWindowsMediaPlayer1.Ctlcontrols.stop();
            frm.axWindowsMediaPlayer1.Visible = false;
            frm.pnlTitle.Visible = false;
            frm.pictureBox1.Dock = DockStyle.Fill;
            frm.pictureBox1.Visible = true;
            frm.pictureBox1.Image = Image.FromFile(textBox2.Text);
            frm.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            currentAction = 3;
            random = 0;
            frm.Show();
            checkBox1.Checked = true;
            checkBox3.Checked = false;
            checkBox5.Checked = false;
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            textBox2.Text = openFileDialog2.FileName.ToString();
            pictureBox3.ImageLocation = textBox2.Text;
            button4.Visible = true;
            button10.Visible = true;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            frm.pictureBox1.Visible = false;
            frm.pnlTitle.Visible = false;
            checkBox3.Checked = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label10.Text = "";
            try
            {
                var file = TagLib.File.Create(lines[0] + @"\sjjm_F_" + comboBox1.Text + "_r720P.mp4");
                TagLib.IPicture pic = file.Tag.Pictures[0];
                MemoryStream stream = new MemoryStream(pic.Data.Data);
                var image = Image.FromStream(stream);
                pictureBox1.Image = image;

                button1.Visible = true;
                button13.Visible = true;
                button8.Visible = true;
            }
            catch (Exception ev)
            {
                label10.Text = ">> No album art! " + ev.Message;
                pictureBox1.Image = Properties.Resources._default;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label10.Text = "";
            if (textBox3.Text.Length > 0 && richTextBox1.Text.Length > 0)
            {
                frm.axWindowsMediaPlayer1.Ctlcontrols.stop();
                frm.axWindowsMediaPlayer1.Visible = false;
                frm.pnlTitle.Visible = true;
                frm.pictureBox1.Visible = false;
                frm.lblDescription.Visible = true;
                frm.pnlTitle.Dock = DockStyle.Fill;
                random = 0;
                frm.lblDescription.Text = DateTime.Now.ToString(CultureInfo.CreateSpecificCulture("fr-FR"));
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                TextInfo textInfo = cultureInfo.TextInfo;
                frm.lblTitle.Text = textBox3.Text + ":\n" + textInfo.ToUpper(richTextBox1.Text);
                frm.lblTitle.Font = new System.Drawing.Font(frm.lblTitle.Font.Name, Convert.ToSingle(comboBox4.Text.ToString()));
                currentAction = 4;
                frm.Show();
                checkBox1.Checked = true;
                checkBox3.Checked = false;
                checkBox5.Checked = false;
            }
            else
            {
                label10.Text = ">> Empty Entries!";
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            frm.pictureBox1.Visible = false;
            frm.pnlTitle.Visible = false;
            checkBox3.Checked = true;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            random = 1;
            checkBox3.Checked = true;
            checkBox5.Checked = false;
            checkBox1.Checked = true;
            frm.axWindowsMediaPlayer1.Visible = false;
            frm.pnlTitle.Visible = false;
            if (iMonitorCount > 1) // If monitor Count 2 or more
            {
                //Get the Dimension of the monitor
                rectMonitor = Screen.AllScreens[1].WorkingArea;
            }
            else
            {
                //Get the Dimension of the monitor
                rectMonitor = Screen.AllScreens[0].WorkingArea;
            }
            frm.Show();
            Random randomk = new Random();
            int randomNumber = randomk.Next(1, 152);
            string rn = "";
            if (randomNumber < 10) rn = "00" + randomNumber;
            else if (randomNumber < 100) rn = "0" + randomNumber;
            else rn = "" + randomNumber;
            label10.Text = "";
            try
            {
                frm.axWindowsMediaPlayer1.URL = lines[0] + @"\sjjm_F_" + rn + "_r720P.mp4";
                try
                {
                    var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);
                    TagLib.IPicture pic = file.Tag.Pictures[0];
                    MemoryStream stream = new MemoryStream(pic.Data.Data);
                    var image = Image.FromStream(stream);
                    pictureBox1.Image = image;
                }
                catch (Exception ev)
                {
                    label10.Text = ">> No album art! " + ev.Message;
                    pictureBox1.Image = Properties.Resources._default;
                }
                //pictureBox1.ImageLocation = @"C:\Users\Adminstrator-Tek\Documents\JW\Cantiques\sjjm_F_" + comboBox1.Text + "_r720P.mp4";
                //frm.axWindowsMediaPlayer1.Visible = true;
                frm.Location = new Point(rectMonitor.X, rectMonitor.Y);
                frm.WindowState = FormWindowState.Maximized;
                frm.axWindowsMediaPlayer1.uiMode = "none";
                frm.axWindowsMediaPlayer1.Top = 0;
                frm.axWindowsMediaPlayer1.Left = 0;
                frm.axWindowsMediaPlayer1.Width = frm.Width;
                frm.axWindowsMediaPlayer1.Height = frm.Height;
                frm.axWindowsMediaPlayer1.stretchToFit = true;
                currentAction = 1;
            }
            catch (Exception ev)
            {
                label10.Text = ">> " + ev.Message;
            }
            button1.Visible = false;
            button13.Visible = false;
            button8.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label10.Text = "";
            lines = System.IO.File.ReadAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "config");
            textBox5.Text = lines[0];
            comboBox2.Text = lines[1];
            for (int i = 0; i < comboBox2.Items.Count; i++)
            {
                if (comboBox2.Text == comboBox2.Items[i].ToString())
                {
                    comboBox2.SelectedIndex = i;
                    break;
                }
            }

            string[] lang = System.IO.File.ReadAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "languages.csv");
            for (int i = 0; i < lang.Length; i++)
             {
                string[] temp = lang[i].Split(',');
                for (int y = 0; y < temp.Length; y++) langa[i, y] = temp[y];
             }

            // Language Customization

            int selLang = 0;
            for (int i = 0; i < 4; i++)
            {
                if (lines[1] == langa[0, i])
                {
                    selLang = i;
                    break;
                }
            }
            checkBox1.Text = langa[1, selLang];
            checkBox3.Text = langa[2, selLang];
            checkBox2.Text = langa[3, selLang];
            label1.Text = langa[4, selLang];
            button15.Text = langa[5, selLang];
            tabPage1.Text = langa[6, selLang];
            tabPage2.Text = langa[7, selLang];
            tabPage3.Text = langa[8, selLang];
            tabPage4.Text = langa[9, selLang];
            tabPage5.Text = langa[10, selLang];
            tabSettings.Text = langa[11, selLang];
            button2.Text = langa[12, selLang];
            button5.Text = langa[12, selLang];
            label3.Text = langa[13, selLang];
            button4.Text = langa[14, selLang];
            button6.Text = langa[14, selLang];
            button7.Text = langa[14, selLang];
            button10.Text = langa[15, selLang];
            button11.Text = langa[15, selLang];
            button12.Text = langa[15, selLang];
            label4.Text = langa[16, selLang];
            label5.Text = langa[17, selLang];
            label7.Text = langa[18, selLang];
            label6.Text = langa[19, selLang];
            label8.Text = langa[20, selLang];
            label9.Text = langa[21, selLang];
            button17.Text = langa[22, selLang];
            checkBox4.Text = langa[23, selLang];
            label2.Text = langa[24, selLang];
            button18.Text = langa[25, selLang];
            checkBox5.Text = langa[26, selLang];
            button19.Text = langa[12, selLang];

            // End of language customization

            //Network

             CheckForIllegalCrossThreadCalls = false;

            textBox5.Enabled = false;
            comboBox2.Enabled = false;
            button17.Visible = false;
            button16.Visible = false;
            button18.Visible = false;

            button1.Visible = false;
            button13.Visible = false;
            button8.Visible = false;

            button3.Visible = false;
            button14.Visible = false;
            button9.Visible = false;

            button4.Visible = false;
            button10.Visible = false;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox4.Checked == true)
            {
                textBox5.Enabled = true;
                comboBox2.Enabled = true;
                button17.Visible = true;
                button16.Visible = true;
                button18.Visible = true;
            }
            else
            {
                textBox5.Enabled = false;
                comboBox2.Enabled = false;
                button17.Visible = false;
                button16.Visible = false;
                button18.Visible = false;
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox5.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void textBox5_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox5.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(System.AppDomain.CurrentDomain.BaseDirectory + "config", false))
            {
                file.WriteLine(textBox5.Text);
                file.WriteLine(comboBox2.Text);
            }
            lines[0] = textBox5.Text;
            lines[1] = comboBox2.Text;
            checkBox4.Checked = false;
            textBox5.Enabled = false;
            comboBox2.Enabled = false;
            button17.Visible = false;
            button16.Visible = false;
            button18.Visible = false;

            string[] lang = System.IO.File.ReadAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "languages.csv");
            for (int i = 0; i < lang.Length; i++)
            {
                string[] temp = lang[i].Split(',');
                for (int y = 0; y < temp.Length; y++) langa[i, y] = temp[y];
            }

            // Language Customization

            int selLang = 0;
            for (int i = 0; i < 4; i++)
            {
                if (lines[1] == langa[0, i])
                {
                    selLang = i;
                    break;
                }
            }
            checkBox1.Text = langa[1, selLang];
            checkBox3.Text = langa[2, selLang];
            checkBox2.Text = langa[3, selLang];
            label1.Text = langa[4, selLang];
            button15.Text = langa[5, selLang];
            tabPage1.Text = langa[6, selLang];
            tabPage2.Text = langa[7, selLang];
            tabPage3.Text = langa[8, selLang];
            tabPage4.Text = langa[9, selLang];
            tabPage5.Text = langa[10, selLang];
            tabSettings.Text = langa[11, selLang];
            button2.Text = langa[12, selLang];
            button5.Text = langa[12, selLang];
            label3.Text = langa[13, selLang];
            button4.Text = langa[14, selLang];
            button6.Text = langa[14, selLang];
            button7.Text = langa[14, selLang];
            button10.Text = langa[15, selLang];
            button11.Text = langa[15, selLang];
            button12.Text = langa[15, selLang];
            label4.Text = langa[16, selLang];
            label5.Text = langa[17, selLang];
            label7.Text = langa[18, selLang];
            label6.Text = langa[19, selLang];
            label8.Text = langa[20, selLang];
            label9.Text = langa[21, selLang];
            button17.Text = langa[22, selLang];
            checkBox4.Text = langa[23, selLang];
            label2.Text = langa[24, selLang];
            button18.Text = langa[25, selLang];
            checkBox5.Text = langa[26, selLang];
            button19.Text = langa[12, selLang];

            // End of language customization
        }

        private void button18_Click(object sender, EventArgs e)
        {
            textBox5.Text = lines[0];
            comboBox2.Text = lines[1];

            checkBox4.Checked = false;
            textBox5.Enabled = false;
            comboBox2.Enabled = false;
            button17.Visible = false;
            button16.Visible = false;
            button18.Visible = false;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox5.Checked == true)
            {
                frm.pictureBox1.Image = Properties.Resources.texteAnee;
                frm.pictureBox1.Dock = DockStyle.Fill;
                frm.pictureBox1.Visible = true;
            }
            else
            {
                frm.pictureBox1.Visible = false;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            label10.Text = "";
            if (textBox3.Text.Length > 0 && richTextBox1.Text.Length > 0)
            {
                frm.axWindowsMediaPlayer1.Ctlcontrols.stop();
                frm.axWindowsMediaPlayer1.Visible = false;
                frm.pnlTitle.Visible = true;
                frm.pictureBox1.Visible = false;
                frm.pnlTitle.Dock = DockStyle.Fill;
                random = 0;
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                TextInfo textInfo = cultureInfo.TextInfo;
                frm.lblDescription.Visible = false;
                frm.lblTitle.Text = "(" + textBox4.Text.ToString(CultureInfo.CreateSpecificCulture("fr-FR")) + ")\n" + richTextBox2.Text;
                frm.lblTitle.Font = new System.Drawing.Font(frm.lblTitle.Font.Name, Convert.ToSingle(comboBox3.Text.ToString()));
                currentAction = 4;
                frm.Show();
                checkBox1.Checked = true;
                checkBox3.Checked = false;
                checkBox5.Checked = false;
            }
            else
            {
                label10.Text = ">> Empty Entries!";
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            frm.pictureBox1.Visible = false;
            frm.pnlTitle.Visible = false;
            checkBox3.Checked = true;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog2.ShowDialog();
            if (result == DialogResult.OK)
            {
                mediapath = folderBrowserDialog2.SelectedPath;
                extraMedia.Clear();
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            currentAction = 5;
            label10.Text = "";
            try
            {
                string[] filePaths = Directory.GetFiles(mediapath);
                for (int i = 0; i < filePaths.Length; i++)
                {
                    if (filePaths[i].Substring(filePaths[i].Length - 3) == "mp4" || filePaths[i].Substring(filePaths[i].Length - 3) == "mp3")
                    {
                        extraMedia.Add(filePaths[i]);
                    }
                }
                if (extraMedia.Count > 0)
                {
                    label10.Text = "";
                    frm.axWindowsMediaPlayer1.Visible = false;
                    frm.pictureBox1.Visible = false;
                    frm.pnlTitle.Visible = false;
                    checkBox3.Checked = false;
                    checkBox5.Checked = false;
                    random = 0;
                    if (iMonitorCount > 1) // If monitor Count 2 or more
                    {
                        //Get the Dimension of the monitor
                        rectMonitor = Screen.AllScreens[1].WorkingArea;
                    }
                    else
                    {
                        //Get the Dimension of the monitor
                        rectMonitor = Screen.AllScreens[0].WorkingArea;
                    }
                    frm.Show();
                    try
                    {
                        frm.axWindowsMediaPlayer1.URL = extraMedia[0];
                        var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);

                        try
                        {
                            TagLib.IPicture pic = file.Tag.Pictures[0];
                            MemoryStream stream = new MemoryStream(pic.Data.Data);
                            var image = Image.FromStream(stream);
                            pictureBox1.Image = image;
                            pictureBox2.Image = image;
                            pictureBox3.Image = image;
                        }
                        catch (Exception ev)
                        {
                            label10.Text = ">> No album art! " + ev.Message;
                            pictureBox1.Image = Properties.Resources._default;
                            pictureBox2.Image = Properties.Resources.video_generic;
                            pictureBox3.Image = Properties.Resources.imageart;
                        }

                        //pictureBox1.ImageLocation = @"C:\Users\Adminstrator-Tek\Documents\JW\Cantiques\sjjm_F_" + comboBox1.Text + "_r720P.mp4";
                        //frm.axWindowsMediaPlayer1.Visible = true;
                        frm.Location = new Point(rectMonitor.X, rectMonitor.Y);
                        frm.WindowState = FormWindowState.Maximized;
                        frm.axWindowsMediaPlayer1.uiMode = "none";
                        frm.axWindowsMediaPlayer1.Top = 0;
                        frm.axWindowsMediaPlayer1.Left = 0;
                        frm.axWindowsMediaPlayer1.Width = frm.Width;
                        frm.axWindowsMediaPlayer1.Height = frm.Height;
                        frm.axWindowsMediaPlayer1.stretchToFit = true;
                        checkBox1.Checked = true;
                        currentAction = 5;
                        frm.pictureBox1.Visible = false;
                        frm.axWindowsMediaPlayer1.Visible = true;
                    }
                    catch (Exception ev)
                    {
                        label10.Text = ">> " + ev.Message;
                    }
                }
                else
                {
                    label10.Text = ">> No suitable files to play";
                }
            }
            catch
            {
                label10.Text = ">> Select Folder";
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            if (currentAction == 5)
            {
                if (frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying) frm.axWindowsMediaPlayer1.Ctlcontrols.pause();
                else frm.axWindowsMediaPlayer1.Ctlcontrols.play();
                frm.axWindowsMediaPlayer1.uiMode = "none";
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (currentAction == 5)
            {
                frm.axWindowsMediaPlayer1.Ctlcontrols.stop();
                frm.axWindowsMediaPlayer1.Visible = false;
                currentSong = 0;
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            label10.Text = "";
            if (currentAction == 5 && frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                label10.Text = "";
                frm.axWindowsMediaPlayer1.Visible = false;
                frm.pictureBox1.Visible = false;
                frm.pnlTitle.Visible = false;
                checkBox3.Checked = false;
                checkBox5.Checked = false;
                random = 0;
                if (iMonitorCount > 1) // If monitor Count 2 or more
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[1].WorkingArea;
                }
                else
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[0].WorkingArea;
                }
                frm.Show();
                try
                {
                    currentSong--;
                    if (currentSong < 0) currentSong = extraMedia.Count - 1;
                    frm.axWindowsMediaPlayer1.URL = extraMedia[currentSong];
                    var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);

                    try
                    {
                        TagLib.IPicture pic = file.Tag.Pictures[0];
                        MemoryStream stream = new MemoryStream(pic.Data.Data);
                        var image = Image.FromStream(stream);
                        pictureBox1.Image = image;
                        pictureBox2.Image = image;
                        pictureBox3.Image = image;
                    }
                    catch (Exception ev)
                    {
                        label10.Text = ">> No album art! " + ev.Message;
                        pictureBox1.Image = Properties.Resources._default;
                        pictureBox2.Image = Properties.Resources.video_generic;
                        pictureBox3.Image = Properties.Resources.imageart;
                    }

                    //pictureBox1.ImageLocation = @"C:\Users\Adminstrator-Tek\Documents\JW\Cantiques\sjjm_F_" + comboBox1.Text + "_r720P.mp4";
                    //frm.axWindowsMediaPlayer1.Visible = true;
                    frm.Location = new Point(rectMonitor.X, rectMonitor.Y);
                    frm.WindowState = FormWindowState.Maximized;
                    frm.axWindowsMediaPlayer1.uiMode = "none";
                    frm.axWindowsMediaPlayer1.Top = 0;
                    frm.axWindowsMediaPlayer1.Left = 0;
                    frm.axWindowsMediaPlayer1.Width = frm.Width;
                    frm.axWindowsMediaPlayer1.Height = frm.Height;
                    frm.axWindowsMediaPlayer1.stretchToFit = true;
                    checkBox1.Checked = true;
                    currentAction = 5;
                    frm.pictureBox1.Visible = false;
                    frm.axWindowsMediaPlayer1.Visible = true;
                }
                catch (Exception ev)
                {
                    label10.Text = ">> " + ev.Message;
                }
            }

        }

        private void button24_Click(object sender, EventArgs e)
        {
            label10.Text = "";
            if (currentAction == 5 && frm.axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                label10.Text = "";
                frm.axWindowsMediaPlayer1.Visible = false;
                frm.pictureBox1.Visible = false;
                frm.pnlTitle.Visible = false;
                checkBox3.Checked = false;
                checkBox5.Checked = false;
                random = 0;
                if (iMonitorCount > 1) // If monitor Count 2 or more
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[1].WorkingArea;
                }
                else
                {
                    //Get the Dimension of the monitor
                    rectMonitor = Screen.AllScreens[0].WorkingArea;
                }
                frm.Show();
                try
                {
                    currentSong++;
                    if (currentSong >= extraMedia.Count) currentSong = 0;
                    frm.axWindowsMediaPlayer1.URL = extraMedia[currentSong];
                    var file = TagLib.File.Create(frm.axWindowsMediaPlayer1.URL);

                    try
                    {
                        TagLib.IPicture pic = file.Tag.Pictures[0];
                        MemoryStream stream = new MemoryStream(pic.Data.Data);
                        var image = Image.FromStream(stream);
                        pictureBox1.Image = image;
                        pictureBox2.Image = image;
                        pictureBox3.Image = image;
                    }
                    catch (Exception ev)
                    {
                        label10.Text = ">> No album art! " + ev.Message;
                        pictureBox1.Image = Properties.Resources._default;
                        pictureBox2.Image = Properties.Resources.video_generic;
                        pictureBox3.Image = Properties.Resources.imageart;
                    }

                    //pictureBox1.ImageLocation = @"C:\Users\Adminstrator-Tek\Documents\JW\Cantiques\sjjm_F_" + comboBox1.Text + "_r720P.mp4";
                    //frm.axWindowsMediaPlayer1.Visible = true;
                    frm.Location = new Point(rectMonitor.X, rectMonitor.Y);
                    frm.WindowState = FormWindowState.Maximized;
                    frm.axWindowsMediaPlayer1.uiMode = "none";
                    frm.axWindowsMediaPlayer1.Top = 0;
                    frm.axWindowsMediaPlayer1.Left = 0;
                    frm.axWindowsMediaPlayer1.Width = frm.Width;
                    frm.axWindowsMediaPlayer1.Height = frm.Height;
                    frm.axWindowsMediaPlayer1.stretchToFit = true;
                    checkBox1.Checked = true;
                    currentAction = 5;
                    frm.pictureBox1.Visible = false;
                    frm.axWindowsMediaPlayer1.Visible = true;
                }
                catch (Exception ev)
                {
                    label10.Text = ">> " + ev.Message;
                }
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
           
        }

        private void button25_Click(object sender, EventArgs e)
        {
            if (netwrk == 0 && textBox6.Text != "") {
                netwrk = 1;
                server = textBox6.Text;
                button25.Text = "Stop";
                textBox6.Visible = false;
                label11.Visible = false;

                //Initialize Network
                Random rnd = new Random();
                strName = "Client" + rnd.Next(1, 101);

                try
                {
                    //Using UDP sockets
                    clientSocket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Dgram, ProtocolType.Udp);

                    //IP address of the server machine
                    IPAddress ipAddress = IPAddress.Parse(textBox6.Text);
                    //Server is listening on port 1000
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                    epServer = (EndPoint)ipEndPoint;

                    
                    msgToSend.cmdCommand = Command.Login;
                    msgToSend.strMessage = null;
                    msgToSend.strName = strName;

                    byte[] byteData = msgToSend.ToByte();

                    //Login to the server
                    clientSocket.BeginSendTo(byteData, 0, byteData.Length,
                        SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SGSclient",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                CheckForIllegalCrossThreadCalls = false;

                //this.Text = "SGSclient: " + strName;

                //The user has logged into the system so we now request the server to send
                //the names of all users who are in the chat room
                
                msgToSend.cmdCommand = Command.List;
                msgToSend.strName = strName;
                msgToSend.strMessage = null;

                byteData = msgToSend.ToByte();

                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer,
                    new AsyncCallback(OnSend), null);

                byteData = new byte[1024];
                //Start listening to the data asynchronously
                clientSocket.BeginReceiveFrom(byteData,
                                           0, byteData.Length,
                                           SocketFlags.None,
                                           ref epServer,
                                           new AsyncCallback(OnReceive),
                                           null);

                netAlive = DateTime.Now.Ticks;
            }
            else if (netwrk == 1)
            {
                netwrk = 0;
                button25.Text = "Start";
                textBox6.Visible = true;
                label11.Visible = true;

                //Network Logout

                try
                {
                    //Send a message to logout of the server
                    Data msgToSend = new Data();
                    msgToSend.cmdCommand = Command.Logout;
                    msgToSend.strName = strName;
                    msgToSend.strMessage = null;

                    byte[] b = msgToSend.ToByte();
                    clientSocket.SendTo(b, 0, b.Length, SocketFlags.None, epServer);
                    clientSocket.Close();
                }
                catch (ObjectDisposedException)
                { }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SGSclient: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSclient: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);

                //Convert the bytes received into an object of type Data
                Data msgReceived = new Data(byteData);

                //Accordingly process the message received
                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        lstChatters.Add(msgReceived.strName);
                        break;

                    case Command.Logout:
                        lstChatters.Remove(msgReceived.strName);
                        break;

                    case Command.Message:
                        break;

                    case Command.List:
                        lstChatters.AddRange(msgReceived.strMessage.Split('*'));
                        lstChatters.RemoveAt(lstChatters.Count - 1);
                        textBox7.Text += "<<<" + strName + " has joined the room>>>\r\n";
                        break;
                }

                if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List)
                {
                    string[] temp = msgReceived.strMessage.Split(' ');
                    if(temp.Length > 1)
                    {
                        string[] parts = temp[1].Split(',');
                        if(parts.Length > 1 && (parts[0] == "CANT" || parts[0] == "VID"))
                        {
                            netPlay = 1;
                            netMedia = parts[0];
                            netCMD = parts[1];
                            netID = parts[2];
                            netcmdTime = Convert.ToInt64(parts[3]);
                        }
                    }
                    textBox7.Text += msgReceived.strMessage + "\r\n";
                }
                byteData = new byte[1024];

                //Start listening to receive more data from the user
                clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epServer,
                                           new AsyncCallback(OnReceive), null);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SGSclient: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button26_Click(object sender, EventArgs e)
        {
            try
            {
                //Fill the info for the message to be send
                Data msgToSend = new Data();

                msgToSend.strName = strName;
                msgToSend.strMessage = textBox8.Text;
                msgToSend.cmdCommand = Command.Message;

                byte[] byteData = msgToSend.ToByte();

                //Send it to the server
                clientSocket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

                textBox8.Text = null;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to send message to the server.", "SGSclientUDP: " + strName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    //The data structure by which the server and the client interact with 
    //each other
    class Data
    {
        //Default constructor
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.strName = null;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            //The first four bytes are for the Command
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            //The next four store the length of the name
            int nameLen = BitConverter.ToInt32(data, 4);

            //The next four store the length of the message
            int msgLen = BitConverter.ToInt32(data, 8);

            //This check makes sure that strName has been passed in the array of bytes
            if (nameLen > 0)
                this.strName = Encoding.UTF8.GetString(data, 12, nameLen);
            else
                this.strName = null;

            //This checks for a null message field
            if (msgLen > 0)
                this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
            else
                this.strMessage = null;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            //First four are for the Command
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            //Add the length of the name
            if (strName != null)
                result.AddRange(BitConverter.GetBytes(strName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the name
            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            //And, lastly we add the message text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            return result.ToArray();
        }

        public string strName;      //Name by which the client logs into the room
        public string strMessage;   //Message text
        public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
    }
}
