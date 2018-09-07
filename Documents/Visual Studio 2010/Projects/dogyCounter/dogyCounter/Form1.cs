/* dogy Counter 1.0
 * This small utility is designed for group administrators in the social network vk.com.
 * If the number of blocked participants (further dogys) in the group exceeds half the group is blocked.
 * This program in real time shows the number of users, the number of dogys and calculates the current percentage of dogys
 * url public and page with search result are in config.ini
 */

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace dogyCounter
{
    public partial class Form1 : Form
    {        
        static string path1 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static string GetPrivateString(string aSection, string aKey)
        {
            StringBuilder buffer = new StringBuilder(SIZE);
            GetPrivateString(aSection, aKey, null, buffer, SIZE, path1 + "/config.ini");
            return buffer.ToString();
        }

        public void WritePrivateString(string aSection, string aKey, string aValue)
        {
            WritePrivateString(aSection, aKey, aValue, path1 + "/config.ini");
        }

        public string Path { get { return path1 + "/config.ini"; } set { path = value; } }

        private const int SIZE = 1024;
        private string path = null;

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetPrivateString(string section, string key, string def, StringBuilder buffer, int size, string path);

        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern int WritePrivateString(string section, string key, string str, string path);

        static string publicName = GetPrivateString("url_set", "public_name");
        static string legalUserPage = GetPrivateString("url_set", "legal_user_page");

        public Form1()
        {
            TopMost = true;
            
            InitializeComponent();
            this.Location = new Point(1141, 609);
            timer1.Interval = 3000;
            timer1.Start();
            label1.BringToFront();
        }

        static String getHttpPage(string url)
        {
            try
            {
                WebRequest req = WebRequest.Create(url);
                WebResponse res = req.GetResponse();
                using (StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex) { return ex.Message; }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //total user
            string html = getHttpPage(publicName);
            string pattern = @"<em class=""pm_counter"">(?<val>.*?)<\/em>";
            RegexOptions options = RegexOptions.Compiled | RegexOptions.Singleline;
            Regex regex = new Regex(pattern, options);
            Match match = regex.Match(html.ToString());
            string totalUser = "";
            totalUser += match.Groups["val"].Value;
            match = match.NextMatch();

            //legal user
            string html1 = getHttpPage(legalUserPage);
            string pattern1 = @"<span>Найден (?<val>.*?) участник";
            RegexOptions options1 = RegexOptions.Compiled | RegexOptions.Singleline;
            Regex regex1 = new Regex(pattern1, options1);
            Match match1 = regex1.Match(html1.ToString());
            string legalUser = "";
            legalUser += match1.Groups["val"].Value;
            match1 = match1.NextMatch();

            string substr = legalUser.Substring(0);
            if (substr == "о" || substr == "а" || substr == "ы" || substr == " ")
            {
                legalUser = legalUser.Remove(0, 1);
            }
            else
            {
                try
                {
                    int totalLegalUser = (Convert.ToInt32(legalUser) * 100) / Convert.ToInt32(totalUser);
                    int percentBot = (100 - totalLegalUser);
                    label1.Text = "Всего подписчиков: " + totalUser + "\nИз них активных: " + legalUser + "\nПроцент 'собачек': " + percentBot + "%";

                    panel2.Width = percentBot * 2;
                    if (percentBot < 20)
                    {
                        panel2.BackColor = System.Drawing.Color.Lime;
                        stateLabel.Text = "Все в порядке";
                    }
                    else if (percentBot >= 20 && percentBot < 50)
                    {
                        panel2.BackColor = System.Drawing.Color.Yellow;
                        stateLabel.Text = "Сократите 'собачек'";
                    }
                    else
                    {
                        panel2.BackColor = System.Drawing.Color.Red;
                        stateLabel.Text = "Группа близка к блокировке";
                    }
                }
                catch
                {
                    
                }

            }
        }
    }
}
