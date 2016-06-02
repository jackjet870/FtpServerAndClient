using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Collections;
using FtpDotNet;
using System.Diagnostics;

namespace FtpClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //SharpFtpServer.ClientConnection ftp;
        //TcpClient client;
        //FtpConnection ftp;
        System.Net.FtpClient.FtpClient ftp;
        string localPath = "C:\\FtpPath";
        string serverPath = "C:\\FtpServer";

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //client = new TcpClient(Dns.GetHostEntry(textBox1.Text.Trim()).HostName, 21);
                //ftp = new SharpFtpServer.ClientConnection(client);
                //ftp = new FtpConnection(textBox1.Text.Trim(), 21, "harve", "kell654321");
                //ftp.LocalDirectory = localPath;
                ftp = new System.Net.FtpClient.FtpClient("kell", "123", textBox1.Text.Trim(), 21);
                ftp.SetWorkingDirectory(localPath);
                ftp.Connect();
                RefreshRemoteList();
                listBox1.Enabled = listBox2.Enabled = true;
                MessageBox.Show("连接成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接失败：" + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(localPath))
                Directory.CreateDirectory(localPath);
            RefreshLocalList();
        }

        private void RefreshLocalList()
        {
            IEnumerable<string> files = Directory.GetFiles(localPath);
            listBox1.Items.Clear();
            foreach (string f in files)
            {
                int a = f.LastIndexOf("\\");
                string file = f.Substring(a + 1);
                listBox1.Items.Add(file);
            }
        }

        private void RefreshRemoteList()
        {
            //List<string> files = ftp.ListDirectory();
            System.Net.FtpClient.FtpListItem[] files = ftp.GetListing(serverPath, System.Net.FtpClient.FtpListType.LIST);
            listBox2.Items.Clear();
            //foreach (string f in files)
            foreach (System.Net.FtpClient.FtpListItem item in files)
            {
                string f = item.LinkPath;
                int a = f.LastIndexOf("\\");
                string file = f.Substring(a + 1);
                listBox2.Items.Add(file);
            }
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effect = DragDropEffects.Link;
            }
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            //string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            //if (files != null)
            //{
            //    foreach (string f in files)
            //    {
            string f = e.Data.GetData(DataFormats.StringFormat) as string;
            int a = f.LastIndexOf("\\");
            string fileName = f.Substring(a + 1);
            ftp.Download(fileName);
            //}
            //}
            /*
            using (NetworkStream ns = client.GetStream())
            {
                object o = e.Data.GetData(DataFormats.FileDrop);
                string f = o.ToString();
                string cmd = "RETR " + f;
                byte[] cmdData = Encoding.ASCII.GetBytes(cmd);
                ns.Write(cmdData, 0, cmdData.Length);
                if (ns.CanRead)
                {
                    List<byte> ds = new List<byte>();
                    byte[] data = new byte[1024];
                    int len = ns.Read(data, 0, data.Length);
                    for (int i = 0; i < len; i++)
                    {
                        ds.Add(data[i]);
                    }
                    while (len > 0)
                    {
                        len = ns.Read(data, 0, data.Length);
                        for (int i = 0; i < len; i++)
                        {
                            ds.Add(data[i]);
                        }
                    }
                    File.WriteAllBytes(localPath + "\\" + f, ds.ToArray());
                }
                RefreshLocalList();
            }*/
        }

        private void listBox2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                e.Effect = DragDropEffects.Link;
            }
        }

        private void listBox2_DragDrop(object sender, DragEventArgs e)
        {
            //string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            //if (files != null)
            //{
            //    foreach (string f in files)
            //    {
            string f = e.Data.GetData(DataFormats.StringFormat) as string;
            int a = f.LastIndexOf("\\");
            string fileName = f.Substring(a + 1);
            ftp.Upload(fileName);
            //}
            //}
            /*
            using (NetworkStream ns = client.GetStream())
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null)
                {
                    foreach (string f in files)
                    {
                        string cmd = "PASV " + f;
                        byte[] cmdData = Encoding.ASCII.GetBytes(cmd);
                        ns.Write(cmdData, 0, cmdData.Length);
                        List<byte> data = new List<byte>(cmdData);
                        data.AddRange(null);
                        ns.Write(data.ToArray(), 0, data.Count);

                        byte[] LIST = Encoding.ASCII.GetBytes("LIST");
                        ns.Write(LIST, 0, LIST.Length);

                        if (ns.CanRead)
                        {
                            List<byte> ds = new List<byte>();
                            byte[] buffer = new byte[1024];
                            int len = ns.Read(buffer, 0, buffer.Length);
                            for (int i = 0; i < len; i++)
                            {
                                ds.Add(buffer[i]);
                            }
                            while (len > 0)
                            {
                                len = ns.Read(buffer, 0, buffer.Length);
                                for (int i = 0; i < len; i++)
                                {
                                    ds.Add(buffer[i]);
                                }
                            }
                            string fileList = Encoding.ASCII.GetString(ds.ToArray());
                            RefreshRemoteList(fileList.Split(' '));
                        }
                    }
                }
            }*/
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            listBox1.DoDragDrop(listBox1.SelectedItem, DragDropEffects.Link);
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            listBox2.DoDragDrop(listBox2.SelectedItem, DragDropEffects.Link);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Process.Start(localPath);
        }

        private void label5_Click(object sender, EventArgs e)
        {
            Process.Start(serverPath);
        }
    }
}