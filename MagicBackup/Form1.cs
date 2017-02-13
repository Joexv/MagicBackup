using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Timers;
using CodeIsle.LibIpsNet;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Reflection;
using IniParser.Model;
using IniParser;
using System.Threading.Tasks;
using DesktopToast;
using Nintenlord.UPSpatcher;

namespace MagicBackup
{
    public partial class Form1 : Form
    {
        private const String APP_ID = "MagicBackup";

        string WindowsVersion = "";
        bool OldNoti = false;
        bool NewNoti = false;

        int BackupNum = 0;

        bool modSelected = false;
        bool origSelected = false;
        bool folderSelected = false;

        private IniFile Settings = new IniFile(@"C:\ProgramData\MagicBackup\Settings.ini");
        private FileIniDataParser fileIniData = new FileIniDataParser();

        System.Timers.Timer timer = new System.Timers.Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            #region Prepare Files
            ExtractFile("image.png");
            ExtractFile("Settings.ini");
            if (!Settings.KeyExists("Minutes", "Settings"))
            {
                Settings.Write("Minutes", "1", "Settings");
            }
            if (!Settings.KeyExists("BackupType", "Settings"))
            {
                Settings.Write("BackupType", "IPS", "Settings");
            }
            if (!Settings.KeyExists("BackupNumber", "Settings"))
            {
                Settings.Write("BackupNumber", "0", "Settings");
            }
            #endregion

            #region Get Settings
            fileIniData.Parser.Configuration.CommentString = "#";

            IniData parsedData = fileIniData.ReadFile(@"C:\ProgramData\MagicBackup\Settings.ini");

            numericUpDown1.Value = Int32.Parse(parsedData["Settings"]["Minutes"]);
            BackupNum = Int32.Parse(parsedData["Settings"]["Minutes"]);
            if (parsedData["Settings"]["BackupType"].ToUpper() == "IPS")
            {
                radioButton1.Checked = true;
            }
            else if (parsedData["Settings"]["BackupType"].ToUpper() == "BAK")
            {
                radioButton3.Checked = true;
            }
            else if (parsedData["Settings"]["BackupType"].ToUpper() == "UPS")
            {
                radioButton2.Checked = true;
            }
            #endregion

            #region Windows Version
            switch (WindowsVersion = getOSInfo())
            {
                case "Windows XP":
                    OldNoti = true;
                    NewNoti = false;
                    break;
                case "Windows Vista":
                    OldNoti = true;
                    NewNoti = false;
                    break;
                case "Windows 7":
                    OldNoti = true;
                    NewNoti = false;
                    break;
                case "Windows 8":
                    OldNoti = false;
                    NewNoti = true;
                    break;
                case "Windows 10":
                    OldNoti = false;
                    NewNoti = true;
                    break;
                default:
                    OldNoti = true;
                    NewNoti = false;
                    break;
            }
            #endregion

        }

        private void handleNotification()
        {
            if(OldNoti == true)
            {
                BalloonNotification();
            }
            else if (NewNoti == true)
            {
                ToastNotification();
            }
        }

        private void ErrorNotification(string original, string modified, string output, string BackupType)
        {
            if (OldNoti == true)
            {
                notifyIcon1.BalloonTipTitle = "Magic Backup";
                notifyIcon1.BalloonTipText = "Backup Failed " + original + " " + modified + " " + output + " " + BackupType;
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(180000);
                Thread.Sleep(2000);
                notifyIcon1.Visible = false;
            }
            else if (NewNoti == true)
            {
                // Get a toast XML template
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

                // Fill in the text elements
                XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
                stringElements[0].AppendChild(toastXml.CreateTextNode("Magic Backup"));
                //Path.GetFileNameWithoutExtension(label1.Text)
                stringElements[1].AppendChild(toastXml.CreateTextNode("Backup Failed"));
                stringElements[2].AppendChild(toastXml.CreateTextNode(original + " " + modified + " " + output + " " + BackupType));

                // Specify the absolute path to an image
                String imagePath = @"C:\ProgramData\MagicBackup\image.png";
                XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
                imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                // Create the toast and attach event listeners
                ToastNotification toast = new ToastNotification(toastXml);

                // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
                ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
            }
        }

        private void ToastNotification()
        {
            // Get a toast XML template
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            // Fill in the text elements
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode("Magic Backup"));
            //Path.GetFileNameWithoutExtension(label1.Text)
            stringElements[1].AppendChild(toastXml.CreateTextNode("Backup Created!"));
            stringElements[2].AppendChild(toastXml.CreateTextNode(""));

            // Specify the absolute path to an image
            String imagePath = @"C:\ProgramData\MagicBackup\image.png";
            XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            // Create the toast and attach event listeners
            ToastNotification toast = new ToastNotification(toastXml);

            // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
        }

        private async Task<string> ShowToastAsync()
        {
            var request = new ToastRequest
            {
                ToastTitle = "DesktopToast WinForms Sample",
                ToastBodyList = new[] { "This is a toast test.", "Looping sound will be played." },
                AppId = "DesktopToast.WinForms",
            };

            var result = await ToastManager.ShowAsync(request);

            return result.ToString();
        }

        private void BalloonNotification()
        {
            notifyIcon1.BalloonTipTitle = "Magic Backup";
            notifyIcon1.BalloonTipText = "Backup created";
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(180000);
            Thread.Sleep(2000);
            notifyIcon1.Visible = false;
        }

        private static void Extract(string nameSpace, string outDirectory, string internalFilePath, string resourceName)
        {
            Assembly assembly = Assembly.GetCallingAssembly();

            using (Stream s = assembly.GetManifestResourceStream(nameSpace + "." + (internalFilePath == "" ? "" : internalFilePath + ".") + resourceName))
            using (BinaryReader r = new BinaryReader(s))
            using (FileStream fs = new FileStream(outDirectory + "\\" + resourceName, FileMode.OpenOrCreate))
            using (BinaryWriter w = new BinaryWriter(fs))
                w.Write(r.ReadBytes((int)s.Length));
        }

        private static void ExtractFile(string FileName)
        {
            if (!File.Exists(@"C:\ProgramData\MagicBackup\" + FileName))
            {
                DirectoryInfo di = Directory.CreateDirectory(@"C:\ProgramData\MagicBackup\");
                Extract("MagicBackup", @"C:\ProgramData\MagicBackup\", "Settings", FileName);
            }
        }

        string getOSInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        else
                            operatingSystem = "7";
                        break;
                    default:
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;
                //See if there's a service pack installed.
                if (os.ServicePack != "")
                {
                    //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                    operatingSystem += " " + os.ServicePack;
                }
                //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                //operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        }

        private void WaitNSeconds(int segundos)
        {
            if (segundos < 1) return;
            DateTime _desired = DateTime.Now.AddSeconds(segundos);
            while (DateTime.Now < _desired)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "GBA File (*.gba)|*.gba";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                label1.Text = ofd.FileName;
                modSelected = true;
                Settings.Write("BackupFile", label1.Text, "Settings");
            }
            ofd.Dispose();

            button4.Enabled = CheckButtons();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
        }

        public void StartTimer(int Minutes)
        {
            timer = new System.Timers.Timer(60000 * Minutes);
            //timer = new System.Timers.Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(OnElapsed);
            timer.AutoReset = false;
            timer.Start();
        }

        // Holds the command line option.
        string option = string.Empty;
        // Holds the path to the patch file.
        string patch = string.Empty;
        // Holds the path to the temporary patch file (used when patch is an HTTP URL).
        string tempPatch = string.Empty;
        // Holds the path to the target file to be patched.
        string target = string.Empty;
        // Holds the path to the original, unaltered file.
        string original = string.Empty;
        // Holds the path to the modified file (with the differences compared to 'original').
        string modified = string.Empty;
        // Holds the path to the output file (te file to write to).
        string output = string.Empty;

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            Backup();
            BackupNum++;
            handleNotification();
            timer.Start(); // Restart timer
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button3.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button5.Enabled = false;

            radioButton1.Enabled = false;
            //radioButton2.Enabled = false;
            radioButton3.Enabled = false;
            StartTimer(Convert.ToInt32(numericUpDown1.Value));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button4.Enabled = true;
            button3.Enabled = false;

            button1.Enabled = true;
            button2.Enabled = true;

            radioButton1.Enabled = true;
            //radioButton2.Enabled = true;
            radioButton3.Enabled = true;

            if (radioButton1.Checked == false)
            {
                button5.Enabled = false;
            }
            else
            {
                button5.Enabled = true;
            }

            timer.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                label2.Text = fbd.SelectedPath;
                folderSelected = true;
                Settings.Write("FolderLocation", label2.Text, "Settings");
            }
            
            button4.Enabled = CheckButtons();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "GBA File (*.gba)|*.gba";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                label3.Text = ofd.FileName;
                origSelected = true;
                Settings.Write("CleanFile", label3.Text, "Settings");
            }
            ofd.Dispose();

            button4.Enabled = CheckButtons();
        }

        public bool CheckButtons()
        {
            if(origSelected == true)
            {
                if (modSelected == true)
                {
                    if (folderSelected == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == false)
            {
                button5.Enabled = false;
            }
            else if (radioButton1.Checked == true)
            {
                Settings.Write("BackupType", "IPS", "Settings");
                button5.Enabled = true;
                if (label3.Text == "No Orig File")
                {
                    origSelected = false;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            origSelected = true;
            modSelected = true;
            folderSelected = true;

            button4.Enabled = CheckButtons();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Settings.Write("Minutes", numericUpDown1.Value.ToString(), "Settings");
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == false)
            {
                button5.Enabled = false;
            }
            else if (radioButton2.Checked == true)
            {
                Settings.Write("BackupType", "UPS", "Settings");
                button5.Enabled = true;
                if (label3.Text == "No Orig File")
                {
                    origSelected = false;
                }
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                Settings.Write("BackupType", "BAK", "Settings");
                origSelected = true;
            }
            else if (radioButton3.Checked == false)
            {
                if (label3.Text == "No Orig File")
                {
                    origSelected = false;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Dispose();
            Settings.Write("BackupNumber", BackupNum.ToString(), "Settings");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            BalloonNotification();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ToastNotification();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Backup();
            //handleNotification();
        }

        private void Backup()
        {
            string BackupTypeError = "IPS";
            original = label3.Text;
            modified = label1.Text;
            output = label2.Text;
            try
            {
                if (radioButton1.Checked == true)
                {
                    //IPS
                    BackupTypeError = "IPS";
                    Creator creator = new Creator();
                    creator.Create(original, modified, output + @"/" + Path.GetFileNameWithoutExtension(label1.Text) + BackupNum + ".ips");

                }
                else if (radioButton2.Checked == true)
                {
                    //UPS
                    byte[] original2 = null, modified2 = null;

                    try
                    {
                        BinaryReader br = new BinaryReader(File.OpenRead(original));
                        original2 = br.ReadBytes((int)br.BaseStream.Length);
                        br.Close();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error opening file\n" + original);
                        return;
                    }

                    try
                    {
                        BinaryReader br = new BinaryReader(File.Open(modified, FileMode.Open));
                        modified2 = br.ReadBytes((int)br.BaseStream.Length);
                        br.Close();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error opening file\n" + modified);
                        return;
                    }
                    BackupTypeError = "UPS";
                    UPSfile upsFile = new UPSfile(original2, modified2);
                    upsFile.writeToFile(output);
                }
                else if (radioButton3.Checked == true)
                {
                    //BAK
                    BackupTypeError = "BAK";
                    string fileName = Path.GetFileNameWithoutExtension(label1.Text);
                    File.Copy(label1.Text, label2.Text + "//" + fileName + BackupNum + ".bak");
                }
            }
            catch
            {
                ErrorNotification(original, modified, output, BackupTypeError);
            }   
        }

        private void button10_Click(object sender, EventArgs e)
        {
            original = label3.Text;
            modified = label1.Text;
            output = label2.Text;
            Creator creator = new Creator();
            creator.Create(original, modified, output + @"\" + Path.GetFileNameWithoutExtension(label1.Text) + BackupNum + ".ips");

            handleNotification();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetFileNameWithoutExtension(label1.Text);
            File.Copy(label1.Text, label2.Text + @"\" + fileName + BackupNum + ".bak");

            handleNotification();
        }

        private void button4_EnabledChanged(object sender, EventArgs e)
        {
            if (button4.Enabled == true)
            {
                button9.Enabled = true;
            }
            else
            {
                button9.Enabled = false;
            }
        }
    }
}
