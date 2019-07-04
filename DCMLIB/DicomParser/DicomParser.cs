using DCMLIB;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static DCMLIB.TransferSyntax;
namespace DicomParser
{
    public partial class DicomParser : Form
    {
        public DicomParser()
        {
            InitializeComponent();
        }
        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }


        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void DicomParser_Load(object sender, EventArgs e)
        {
            cbTransferSyntax.Items.Clear();
            foreach (KeyValuePair<string, TransferSyntax> syntax in TransferSyntaxs.All)
            {
                cbTransferSyntax.Items.Add(syntax.Value);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] data = HexStringToByteArray(txtInput.Text);
            //解码到数据集对象
            DCMDataSet ds = new DCMDataSet((TransferSyntax)cbTransferSyntax.SelectedItem);
            uint idx = 0;
            ds.Decode(data, ref idx);
            //数据集转换为字符串显示
            string str = ds.ToString("");
            string[] lines = str.Split('\n');
            lvOutput.Items.Clear();
            for (int i = 0; i < lines.Length; i++)
            {
                ListViewItem item = new ListViewItem(lines[i].Split('\t'));
                lvOutput.Items.Add(item);
            }
        }

        private void cbTransferSyntax_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();//打开OpenFileDlg选择文件
            DCMFile file = new DCMFile(); //实例化DCMFile对象
            dialog.ShowDialog();
            file.Decode(dialog.FileName);

            string[] lines = file.ToString("").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                ListViewItem item = new ListViewItem(lines[i].Split('\t'));
                lvOutput.Items.Add(item); //显示到ListView中;
            }

            frmImage form = new frmImage(file.Data);
            form.Show();
        }

        private void lvOutput_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

   
    }
}
