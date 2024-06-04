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
        private ToolStripMenuItem clearScreenMenuItem;
        private Bitmap _bitmap;
        private Aes _aes;
        private RSA _rsa;
        private bool _isPenMode;
        private bool _isEncrypted;
        private int _dragCount = 0;

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
            ShowMouseModePrompt();
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

            clearScreenMenuItem = new ToolStripMenuItem("清屏");
            clearScreenMenuItem.Click += ClearScreenMenuItem_Click;
            menuStrip666.Items.Add(clearScreenMenuItem);

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

        private void ShowMouseModePrompt()
        {
            var result = MessageBox.Show("是否切换到画笔工具？", "提示", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                _isPenMode = true;
            }
            else
            {
                MessageBox.Show("如需要绘画，请使用画笔工具");
            }
        }

        private void ClearScreenMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确定清屏吗？", "确认清屏", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                ClearCanvas();
            }
        }

        private void ClearCanvas()
        {
            using (Graphics g = Graphics.FromImage(_canvas))
            {
                g.Clear(Color.White);
            }
            this.Invalidate();
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
            if (!_isEncrypted)
            {
                MessageBox.Show("未进行加密操作，无法解密！");
                return;
            }

            DialogResult result = MessageBox.Show("解密操作将清除页面上的所有图像。是否继续？", "警告", MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Yes)
            {
                EncryptAndSave();
                MessageBox.Show("加密保存成功！请继续选择要解密的文件");
            }
            else if (result == DialogResult.No)
            {
                MessageBox.Show("解密操作已取消。");
                return;
            }
            else
            {
                MessageBox.Show("解密操作已取消。");
                return;
            }

            try
            {
                if (DecryptAndLoad())
                {
                    Invalidate();
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    byte[] encryptedAesKey = new byte[_rsa.KeySize / 8];
                    byte[] encryptedAesIV = new byte[_rsa.KeySize / 8];
                    fileStream.Read(encryptedAesKey, 0, encryptedAesKey.Length);
                    fileStream.Read(encryptedAesIV, 0, encryptedAesIV.Length);

                    byte[] aesKey = _rsa.Decrypt(encryptedAesKey, RSAEncryptionPadding.Pkcs1);
                    byte[] aesIV = _rsa.Decrypt(encryptedAesIV, RSAEncryptionPadding.Pkcs1);

                    _aes.Key = aesKey;
                    _aes.IV = aesIV;

                    byte[] encryptedData = new byte[fileStream.Length - encryptedAesKey.Length - encryptedAesIV.Length];
                    fileStream.Read(encryptedData, 0, encryptedData.Length);

                    ICryptoTransform decryptor = _aes.CreateDecryptor();
                    byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                    using (MemoryStream ms = new MemoryStream(decryptedData))
                    {
                        Bitmap decryptedBitmap = new Bitmap(ms);
                        _canvas = new Bitmap(decryptedBitmap);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
