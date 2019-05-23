using CodeIsle.LibIpsNet;
using IniParser;
using IniParser.Model;
using Nintenlord.UPSpatcher;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;


namespace MagicBackup
{
    public partial class Form1 : Form
    {

        private const String APP_ID = "MagicBackup";

        private int BackupNum = 0;
        private FileIniDataParser fileIniData = new FileIniDataParser();
        private bool folderSelected = false;

        // Holds the path to the modified file (with the differences compared to 'original').
        private string modified = string.Empty;

        private bool modSelected = false;
        private bool OldNoti = false;

        // Holds the command line option.
        private string option = string.Empty;

        // Holds the path to the original, unaltered file.
        private string original = string.Empty;

        private bool origSelected = false;

        // Holds the path to the output file (te file to write to).
        private string output = string.Empty;

        // Holds the path to the patch file.
        private string patch = string.Empty;

        private IniFile Settings = new IniFile(@"C:\ProgramData\MagicBackup\Settings.ini");

        // Holds the path to the target file to be patched.
        private string target = string.Empty;

        // Holds the path to the temporary patch file (used when patch is an HTTP URL).
        private string tempPatch = string.Empty;

        private System.Timers.Timer timer = new System.Timers.Timer();



        public Form1()
        {
            InitializeComponent();
        }


        public bool CheckButtons
            => (origSelected && modSelected && folderSelected);

        public void StartTimer(int Minutes)
        {
            timer = new System.Timers.Timer(60000 * Minutes);
            timer.Elapsed += new ElapsedEventHandler(OnElapsed);
            timer.AutoReset = false;
            timer.Start();
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
        private void Backup()
        {
            string BackupTypeError = "IPS";
            original = label3.Text;
            modified = label1.Text;
            output = label2.Text;
            string Time = string.Format("{0:yyyy-MM-dd_hh.mm.ss.tt}", DateTime.Now);
            try
            {
                if (radioButton1.Checked == true)
                {
                    //IPS
                    BackupTypeError = "IPS";
                    Creator creator = new Creator();
                    creator.Create(original, modified, output + @"/" + Path.GetFileNameWithoutExtension(label1.Text) + "_" + Time + "_" + ".ips");
                }
                else if (radioButton2.Checked == true)
                {
                    //UPS
                    byte[] original2 = null, modified2 = null;

                    original2 = getBytes(original);
                    modified2 = getBytes(modified);

                    BackupTypeError = "UPS";
                    UPSfile upsFile = new UPSfile(original2, modified2);
                    upsFile.writeToFile(output + "//" + Path.GetFileNameWithoutExtension(label1.Text) + "_" + Time + "_" + ".ups");
                }
                else if (radioButton3.Checked == true)
                {
                    //BAK
                    BackupTypeError = "BAK";
                    string fileName = Path.GetFileNameWithoutExtension(label1.Text);
                    File.Copy(label1.Text, label2.Text + "//" + fileName + "_" + Time + "_" + ".bak");
                }
            }
            catch
            {
                BalloonNotification("Magic Backup Failed!", 
                    String.Format("Backup Failed\nOriginal File:{0}\nModded File:{1}\nOutput:{2}\nBackup Type:{3}", original, modified, output, BackupTypeError));
            }
        }

        private byte[] getBytes(string file)
        {
            byte[] Results = { };
            try
            {
                BinaryReader br = new BinaryReader(File.OpenRead(file));
                Results = br.ReadBytes((int)br.BaseStream.Length);
                br.Close();
            }
            catch (Exception ex) {
                BalloonNotification("Fatal Error!", ex.ToString());
            }
            return Results;
        }

        private void BalloonNotification(string Title = "Magic Backup", string Text = "Backup created")
        {
            notifyIcon1.BalloonTipTitle = Title;
            notifyIcon1.BalloonTipText = Text;
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(180000);
            Thread.Sleep(2000);
            notifyIcon1.Visible = false;
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

            BinaryReader br = new BinaryReader(File.Open(label1.Text, FileMode.Open));
            long sourcelen = br.BaseStream.Length;
            if (sourcelen > 16777216)
            {
                radioButton1.Enabled = false;
                radioButton1.Checked = false;
            }
            else
                radioButton1.Enabled = true;
            
            br.Close();

            button4.Enabled = CheckButtons;
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

        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                label2.Text = fbd.SelectedPath;
                folderSelected = true;
                Settings.Write("FolderLocation", label2.Text, "Settings");
            }

            button4.Enabled = CheckButtons;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button4.Enabled = true;
            button3.Enabled = false;

            button1.Enabled = true;
            button2.Enabled = true;

            radioButton1.Enabled = true;
            radioButton3.Enabled = true;

            button5.Enabled = radioButton1.Checked;

            timer.Dispose();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button3.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button5.Enabled = false;

            radioButton1.Enabled = false;

            radioButton3.Enabled = false;
            StartTimer(Convert.ToInt32(numericUpDown1.Value));
        }

        private void button4_EnabledChanged(object sender, EventArgs e)
        {
                button9.Enabled = button4.Enabled;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "GBA File (*.gba)|*.gba";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                origSelected = true;
                Settings.Write("CleanFile", ofd.FileName, "Settings");
            }
            ofd.Dispose();

            button4.Enabled = CheckButtons;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            origSelected = true;
            modSelected = true;
            folderSelected = true;

            button4.Enabled = CheckButtons;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Backup();
            handleNotification();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Dispose();
            Settings.Write("BackupNumber", BackupNum.ToString(), "Settings");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ExtractFile("image.png");
            ExtractFile("Settings.ini");
            if (!Settings.KeyExists("Minutes", "Settings"))
                Settings.Write("Minutes", "1", "Settings");
            if (!Settings.KeyExists("BackupType", "Settings"))
                Settings.Write("BackupType", "IPS", "Settings");
            if (!Settings.KeyExists("BackupNumber", "Settings"))
                Settings.Write("BackupNumber", "0", "Settings");

            fileIniData.Parser.Configuration.CommentString = "#";
            IniData parsedData = fileIniData.ReadFile(@"C:\ProgramData\MagicBackup\Settings.ini");

            numericUpDown1.Value = Int32.Parse(parsedData["Settings"]["Minutes"]);
            BackupNum = Int32.Parse(parsedData["Settings"]["Minutes"]);
            if (parsedData["Settings"]["BackupType"].ToUpper() == "IPS")
                radioButton1.Checked = true;
            else if (parsedData["Settings"]["BackupType"].ToUpper() == "BAK")
                radioButton3.Checked = true;
            else if (parsedData["Settings"]["BackupType"].ToUpper() == "UPS")
                radioButton2.Checked = true;
        }


        private void handleNotification()
        {
            if (checkBox1.Checked == true)
                BalloonNotification();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Settings.Write("Minutes", numericUpDown1.Value.ToString(), "Settings");
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            Backup();
            handleNotification();
            timer.Start();
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
                    origSelected = false; 
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == false)
                button5.Enabled = false;
            else if (radioButton2.Checked == true)
            {
                Settings.Write("BackupType", "UPS", "Settings");
                button5.Enabled = true;

                if (label3.Text == "No Orig File")
                    origSelected = false;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                Settings.Write("BackupType", "BAK", "Settings");
                origSelected = true;
            }
            else if (radioButton3.Checked == false && label3.Text == "No Orig File")
                origSelected = false;
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }
    }
}