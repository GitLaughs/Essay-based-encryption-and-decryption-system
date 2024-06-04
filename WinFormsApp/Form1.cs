using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        private bool _isDrawing;
        private Bitmap _canvas;
        private Point _lastPoint;
        private Pen _pen;
        private Color _lineColor;
        private DashStyle _lineStyle;
        private MenuStrip menuStrip666;
        private ToolStripMenuItem encryptMenuItem;
        private ToolStripMenuItem decryptMenuItem;
        private ToolStripMenuItem mouseMenuItem;
        private ToolStripMenuItem penMenuItem;
        private Bitmap _bitmap;
        private Aes _aes;
        private RSA _rsa;
        private bool _isPenMode;
        private bool _isEncrypted;

        public Form1()
        {
            menuStrip666 = new MenuStrip();
            InitializeComponent();
            _bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            _aes = Aes.Create();
            _rsa = RSA.Create();
            _canvas = new Bitmap(800, 600);
            _pen = new Pen(Color.Black, 2);
            _isDrawing = false;
            _isPenMode = false;
            _isEncrypted = false;
            this.Paint += new PaintEventHandler(Form1_Paint);
            this.MouseDown += new MouseEventHandler(Form1_MouseDown);
            this.MouseMove += new MouseEventHandler(Form1_MouseMove);
            this.MouseUp += new MouseEventHandler(Form1_MouseUp);
            this.Resize += new EventHandler(Form1_Resize);
            _lineColor = Color.Black;
            _lineStyle = DashStyle.Solid;
            InitializeMenuStrip();
        }

        private void InitializeMenuStrip()
        {
            menuStrip666 = new MenuStrip();
            encryptMenuItem = new ToolStripMenuItem("加密");
            decryptMenuItem = new ToolStripMenuItem("解密");
            encryptMenuItem.Click += EncryptMenuItem_Click;
            decryptMenuItem.Click += DecryptMenuItem_Click;
            menuStrip666.Items.Add(encryptMenuItem);
            menuStrip666.Items.Add(decryptMenuItem);

            mouseMenuItem = new ToolStripMenuItem("鼠标");
            penMenuItem = new ToolStripMenuItem("画笔");
            mouseMenuItem.Click += MouseMenuItem_Click;
            penMenuItem.Click += PenMenuItem_Click;
            menuStrip666.Items.Add(mouseMenuItem);
            menuStrip666.Items.Add(penMenuItem);
            this.Controls.Add(menuStrip666);
            ToolStripMenuItem drawMenuItem = new ToolStripMenuItem("绘图");
            ToolStripMenuItem lineColorMenuItem = new ToolStripMenuItem("线条颜色");
            ToolStripMenuItem lineStyleMenuItem = new ToolStripMenuItem("线条类型");

            foreach (KnownColor color in Enum.GetValues(typeof(KnownColor)))
            {
                ToolStripMenuItem colorItem = new ToolStripMenuItem(color.ToString());
                colorItem.Tag = Color.FromKnownColor(color);
                colorItem.Click += LineColorMenuItem_Click;
                lineColorMenuItem.DropDownItems.Add(colorItem);
            }

            foreach (DashStyle style in Enum.GetValues(typeof(DashStyle)))
            {
                ToolStripMenuItem styleItem = new ToolStripMenuItem(style.ToString());
                styleItem.Tag = style;
                styleItem.Click += LineStyleMenuItem_Click;
                lineStyleMenuItem.DropDownItems.Add(styleItem);
            }

            drawMenuItem.DropDownItems.Add(lineColorMenuItem);
            drawMenuItem.DropDownItems.Add(lineStyleMenuItem);
            menuStrip666.Items.Add(drawMenuItem);
        }

        private void MouseMenuItem_Click(object sender, EventArgs e)
        {
            _isPenMode = false;
        }

        private void PenMenuItem_Click(object sender, EventArgs e)
        {
            _isPenMode = true;
        }

        private void EncryptMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                EncryptAndSave();
                _isEncrypted = true;
                MessageBox.Show("加密成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("加密失败: " + ex.Message);
            }
        }

        private void DecryptMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (DecryptAndLoad())
                {
                    Invalidate();  // 使整个窗体无效并触发重绘事件
                    MessageBox.Show("解密成功！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("解密失败: " + ex.Message);
            }
        }

        private void LineColorMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem colorItem = (ToolStripMenuItem)sender;
            _lineColor = (Color)colorItem.Tag;
            _pen.Color = _lineColor;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                _canvas = new Bitmap(ClientSize.Width, ClientSize.Height);
            }
        }

        private void LineStyleMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem styleItem = (ToolStripMenuItem)sender;
            _lineStyle = (DashStyle)styleItem.Tag;
            _pen.DashStyle = _lineStyle;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (_isPenMode && e.Button == MouseButtons.Left)
            {
                _isDrawing = true;
                _lastPoint = e.Location;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPenMode && _isDrawing)
            {
                using (Graphics g = Graphics.FromImage(_canvas))
                {
                    g.DrawLine(_pen, _lastPoint, e.Location);
                }
                _lastPoint = e.Location;
                this.Invalidate();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_canvas, 0, 0);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isPenMode && _isDrawing)
            {
                _isDrawing = false;
            }
        }

        private void EncryptAndSave()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    _canvas.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] drawingData = ms.ToArray();
                    ICryptoTransform encryptor = _aes.CreateEncryptor();
                    byte[] encryptedData = encryptor.TransformFinalBlock(drawingData, 0, drawingData.Length);
                    byte[] encryptedAesKey = _rsa.Encrypt(_aes.Key, RSAEncryptionPadding.Pkcs1);
                    byte[] encryptedAesIV = _rsa.Encrypt(_aes.IV, RSAEncryptionPadding.Pkcs1);
                    using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        fileStream.Write(encryptedAesKey, 0, encryptedAesKey.Length);
                        fileStream.Write(encryptedAesIV, 0, encryptedAesIV.Length);
                        fileStream.Write(encryptedData, 0, encryptedData.Length);
                    }
                }
            }
        }

        private bool DecryptAndLoad()
        {
            if (!_isEncrypted)
            {
                MessageBox.Show("未进行加密操作，无法解密！");
                return false;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                byte[] encryptedAesKey, encryptedAesIV, encryptedData;
                using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    encryptedAesKey = new byte[_rsa.KeySize / 8];
                    encryptedAesIV = new byte[_rsa.KeySize / 8];
                    encryptedData = new byte[fileStream.Length - encryptedAesKey.Length - encryptedAesIV.Length];
                    fileStream.Read(encryptedAesKey, 0, encryptedAesKey.Length);
                    fileStream.Read(encryptedAesIV, 0, encryptedAesIV.Length);
                    fileStream.Read(encryptedData, 0, encryptedData.Length);
                }

                byte[] aesKey;
                byte[] aesIV;
                byte[] decryptedData;
                try
                {
                    aesKey = _rsa.Decrypt(encryptedAesKey, RSAEncryptionPadding.Pkcs1);
                    aesIV = _rsa.Decrypt(encryptedAesIV, RSAEncryptionPadding.Pkcs1);

                    using (MemoryStream ms = new MemoryStream(encryptedData))
                    {
                        ICryptoTransform decryptor = _aes.CreateDecryptor(aesKey, aesIV);
                        decryptedData = decryptor.TransformFinalBlock(ms.ToArray(), 0, (int)ms.Length);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("解密过程中出错: " + ex.Message);
                    return false;
                }

                using (MemoryStream msBitmap = new MemoryStream(decryptedData))
                {
                    _canvas = new Bitmap(msBitmap);
                }
                return true;
            }
            return false;
        }
    }
}
