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
        // 定义一些私有变量
        private bool _isDrawing;  // 是否正在绘图
        private Bitmap _canvas;  // 画布
        private Point _lastPoint;  // 上一个点的位置
        private Pen _pen;  // 画笔
        private Color _lineColor;  // 线条颜色
        private DashStyle _lineStyle;  // 线条样式
        private MenuStrip menuStrip666;  // 菜单栏
        private ToolStripMenuItem encryptMenuItem;  // 加密菜单项
        private ToolStripMenuItem decryptMenuItem;  // 解密菜单项
        private ToolStripMenuItem mouseMenuItem;  // 鼠标菜单项
        private ToolStripMenuItem penMenuItem;  // 画笔菜单项
        private ToolStripMenuItem clearScreenMenuItem;  // 清屏菜单项
        private Bitmap _bitmap;  // 位图
        private Aes _aes;  // AES加密对象
        private RSA _rsa;  // RSA加密对象
        private bool _isPenMode;  // 是否为画笔模式
        private bool _isEncrypted;  // 是否已加密
        private int _dragCount = 0;  // 拖动计数

        // 构造函数
        public Form1()
        {
            menuStrip666 = new MenuStrip();
            InitializeComponent();
            _bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            _aes = Aes.Create();  // 创建AES加密对象
            _rsa = RSA.Create();  // 创建RSA加密对象
            _canvas = new Bitmap(800, 600);  // 创建画布
            _pen = new Pen(Color.Black, 2);  // 创建画笔
            _isDrawing = false;  // 初始化绘图状态
            _isPenMode = true;  // 初始化画笔模式
            _isEncrypted = false;  // 初始化加密状态
            this.Paint += new PaintEventHandler(Form1_Paint);  // 添加绘图事件处理器
            this.MouseDown += new MouseEventHandler(Form1_MouseDown);  // 添加鼠标按下事件处理器
            this.MouseMove += new MouseEventHandler(Form1_MouseMove);  // 添加鼠标移动事件处理器
            this.MouseUp += new MouseEventHandler(Form1_MouseUp);  // 添加鼠标释放事件处理器
            this.Resize += new EventHandler(Form1_Resize);  // 添加窗口大小改变事件处理器
            _lineColor = Color.Black;  // 初始化线条颜色
            _lineStyle = DashStyle.Solid;  // 初始化线条样式
            InitializeMenuStrip();  // 初始化菜单栏
            //ShowMouseModePrompt();
        }

        // 初始化菜单栏
        private void InitializeMenuStrip()
        {
            menuStrip666 = new MenuStrip();
            encryptMenuItem = new ToolStripMenuItem("加密");
            decryptMenuItem = new ToolStripMenuItem("解密");
            encryptMenuItem.Click += EncryptMenuItem_Click;  // 添加加密菜单项点击事件处理器
            decryptMenuItem.Click += DecryptMenuItem_Click;  // 添加解密菜单项点击事件处理器
            menuStrip666.Items.Add(encryptMenuItem);
            menuStrip666.Items.Add(decryptMenuItem);

            mouseMenuItem = new ToolStripMenuItem("鼠标");
            penMenuItem = new ToolStripMenuItem("画笔");
            mouseMenuItem.Click += MouseMenuItem_Click;  // 添加鼠标菜单项点击事件处理器
            penMenuItem.Click += PenMenuItem_Click;  // 添加画笔菜单项点击事件处理器
            menuStrip666.Items.Add(mouseMenuItem);
            menuStrip666.Items.Add(penMenuItem);

            clearScreenMenuItem = new ToolStripMenuItem("清屏");
            clearScreenMenuItem.Click += ClearScreenMenuItem_Click;  // 添加清屏菜单项点击事件处理器
            menuStrip666.Items.Add(clearScreenMenuItem);

            this.Controls.Add(menuStrip666);

            ToolStripMenuItem drawMenuItem = new ToolStripMenuItem("绘图");
            ToolStripMenuItem lineColorMenuItem = new ToolStripMenuItem("线条颜色");
            ToolStripMenuItem lineStyleMenuItem = new ToolStripMenuItem("线条类型");

            foreach (KnownColor color in Enum.GetValues(typeof(KnownColor)))
            {
                ToolStripMenuItem colorItem = new ToolStripMenuItem(color.ToString());
                colorItem.Tag = Color.FromKnownColor(color);
                colorItem.Click += LineColorMenuItem_Click;  // 添加线条颜色菜单项点击事件处理器
                lineColorMenuItem.DropDownItems.Add(colorItem);
            }

            foreach (DashStyle style in Enum.GetValues(typeof(DashStyle)))
            {
                ToolStripMenuItem styleItem = new ToolStripMenuItem(style.ToString());
                styleItem.Tag = style;
                styleItem.Click += LineStyleMenuItem_Click;  // 添加线条样式菜单项点击事件处理器
                lineStyleMenuItem.DropDownItems.Add(styleItem);
            }

            drawMenuItem.DropDownItems.Add(lineColorMenuItem);
            drawMenuItem.DropDownItems.Add(lineStyleMenuItem);
            menuStrip666.Items.Add(drawMenuItem);
        }

        // 清屏菜单项点击事件处理器
        private void ClearScreenMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确定清屏吗？", "确认清屏", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                ClearCanvas();  // 清除画布
            }
        }

        // 清除画布
        private void ClearCanvas()
        {
            using (Graphics g = Graphics.FromImage(_canvas))
            {
                g.Clear(Color.White);  // 将画布清空为白色
            }
            this.Invalidate();  // 使窗口无效，触发重绘事件
        }

        // 鼠标菜单项点击事件处理器
        private void MouseMenuItem_Click(object sender, EventArgs e)
        {
            _isPenMode = false;  // 切换到鼠标模式
        }

        // 画笔菜单项点击事件处理器
        private void PenMenuItem_Click(object sender, EventArgs e)
        {
            _isPenMode = true;  // 切换到画笔模式
        }

        // 加密菜单项点击事件处理器
        private void EncryptMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                EncryptAndSave();  // 尝试加密并保存绘图
                _isEncrypted = true;  // 如果成功，将_isEncrypted标志设置为true
                MessageBox.Show("加密成功！");  // 显示消息框通知用户加密成功
            }
            catch (Exception ex)
            {
                MessageBox.Show("加密失败: " + ex.Message);  // 如果捕获到异常，显示消息框通知用户加密失败，并显示异常信息
            }
        }

        private void DecryptMenuItem_Click(object sender, EventArgs e)
        {
            if (!_isEncrypted)  // 如果_isEncrypted标志为false，表示当前没有加密的绘图
            {
                MessageBox.Show("未进行加密操作，无法解密！");  // 显示消息框通知用户没有加密的绘图，无法进行解密
                return;  // 直接返回，不执行后续代码
            }

            DialogResult result = MessageBox.Show("解密操作将清除页面上的所有图像。是否保存文件", "警告", MessageBoxButtons.YesNoCancel);  // 显示消息框询问用户是否保存当前的绘图

            if (result == DialogResult.Yes)  // 如果用户选择"Yes"，则尝试加密并保存当前的绘图
            {
                try
                {
                    EncryptAndSave();  // 尝试加密并保存当前的绘图
                    MessageBox.Show("加密保存成功！请继续选择要解密的文件");  // 如果成功，显示消息框通知用户加密并保存成功，可以选择要解密的文件
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败: " + ex.Message);  // 如果捕获到异常，显示消息框通知用户保存失败，并显示异常信息
                    return;  // 直接返回，不执行后续代码
                }
            }
            else if (result == DialogResult.No)  // 如果用户选择"No"，则不保存当前的绘图，直接进行解密操作
            {
                // 继续进行解密
            }
            else if (result == DialogResult.Cancel)  // 如果用户选择"Cancel"，则取消解密操作，直接返回，不执行后续代码
            {
                // 取消操作，直接返回
                return;
            }

            try
            {
                if (DecryptAndLoad())  // 尝试解密并加载绘图，如果成功，将解密后的绘图显示在窗体上
                {
                    Invalidate();  // 使窗体无效，窗体在下一次重绘操作时会自动重绘解密后的绘图
                    MessageBox.Show("解密成功！");  // 显示消息框通知用户解密成功
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("解密失败: " + ex.Message);  // 如果捕获到异常，显示消息框通知用户解密失败，并显示异常信息
            }
        }

        private void LineColorMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem colorItem = (ToolStripMenuItem)sender;  // 获取触发事件的菜单项
            _lineColor = (Color)colorItem.Tag;  // 从菜单项的Tag属性中获取颜色值
            _pen.Color = _lineColor;  // 将画笔的颜色设置为获取的颜色值
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (ClientSize.Width > 0 && ClientSize.Height > 0)  // 如果窗体的客户区宽度和高度都大于0
            {
                _canvas = new Bitmap(ClientSize.Width, ClientSize.Height);  // 创建一个新的位图，其大小与窗体的客户区大小相同，用于作为绘图的画布
            }
        }

        private void LineStyleMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem styleItem = (ToolStripMenuItem)sender;  // 获取触发事件的菜单项
            _lineStyle = (DashStyle)styleItem.Tag;  // 从菜单项的Tag属性中获取线条样式
            _pen.DashStyle = _lineStyle;  // 将画笔的线条样式设置为获取的线条样式
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (_isPenMode && e.Button == MouseButtons.Left)  // 如果当前处于画笔模式，并且鼠标左键被按下
            {
                _isDrawing = true;  // 将_isDrawing标志设置为true，表示开始绘图
                _lastPoint = e.Location;  // 记录鼠标的位置，作为绘图的起始点
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPenMode && _isDrawing)  // 如果当前处于画笔模式，并且正在绘图
            {
                using (Graphics g = Graphics.FromImage(_canvas))  // 创建一个Graphics对象，用于在画布上绘图
                {
                    g.DrawLine(_pen, _lastPoint, e.Location);  // 使用画笔在画布上绘制一条线，从上一个点到当前鼠标的位置
                }
                _lastPoint = e.Location;  // 更新上一个点的位置为当前鼠标的位置
                this.Invalidate();  // 使窗体无效，窗体在下一次重绘操作时会自动重绘画布上的图像
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(_canvas, 0, 0);  // 在窗体上绘制画布上的图像
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_isPenMode && _isDrawing)  // 如果当前处于画笔模式，并且正在绘图
            {
                _isDrawing = false;  // 将_isDrawing标志设置为false，表示结束绘图
            }
        }

        private void EncryptAndSave()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();  // 创建一个保存文件对话框
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";  // 设置保存文件对话框的过滤器，使其只显示.txt文件
            bool success = false;  // 创建一个布尔变量，用于标识操作是否成功

            if (saveFileDialog.ShowDialog() == DialogResult.OK)  // 如果用户在保存文件对话框中点击了"OK"按钮
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())  // 创建一个内存流，用于存储绘图数据
                    {
                        _canvas.Save(ms, System.Drawing.Imaging.ImageFormat.Png);  // 将画布上的图像保存到内存流中，格式为PNG
                        byte[] drawingData = ms.ToArray();  // 将内存流中的数据转换为字节数组
                        ICryptoTransform encryptor = _aes.CreateEncryptor();  // 创建一个AES加密器
                        byte[] encryptedData = encryptor.TransformFinalBlock(drawingData, 0, drawingData.Length);  // 使用AES加密器对绘图数据进行加密
                        byte[] encryptedAesKey = _rsa.Encrypt(_aes.Key, RSAEncryptionPadding.Pkcs1);  // 使用RSA算法加密AES的密钥
                        byte[] encryptedAesIV = _rsa.Encrypt(_aes.IV, RSAEncryptionPadding.Pkcs1);  // 使用RSA算法加密AES的初始化向量

                        using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))  // 创建一个文件流，用于将数据写入文件
                        {
                            fileStream.Write(encryptedAesKey, 0, encryptedAesKey.Length);  // 将加密后的AES密钥写入文件
                            fileStream.Write(encryptedAesIV, 0, encryptedAesIV.Length);  // 将加密后的AES初始化向量写入文件
                            fileStream.Write(encryptedData, 0, encryptedData.Length);  // 将加密后的绘图数据写入文件
                            success = true; // 如果成功写入文件，设置 success 为 true
                        }
                    }
                }
                catch
                {
                    success = false; // 如果发生异常，设置 success 为 false
                    throw;
                }
            }

            if (!success)
            {
                throw new Exception("加密过程中出现错误或用户取消了操作。");
            }
        }


        private bool DecryptAndLoad()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();  // 创建一个打开文件对话框
            if (openFileDialog.ShowDialog() == DialogResult.OK)  // 如果用户在打开文件对话框中点击了"OK"按钮
            {
                using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open))  // 创建一个文件流，用于读取用户选择的文件
                {
                    byte[] encryptedAesKey = new byte[_rsa.KeySize / 8];  // 创建一个字节数组，用于存储加密的AES密钥
                    byte[] encryptedAesIV = new byte[_rsa.KeySize / 8];  // 创建一个字节数组，用于存储加密的AES初始化向量
                    fileStream.Read(encryptedAesKey, 0, encryptedAesKey.Length);  // 从文件中读取加密的AES密钥
                    fileStream.Read(encryptedAesIV, 0, encryptedAesIV.Length);  // 从文件中读取加密的AES初始化向量

                    byte[] aesKey = _rsa.Decrypt(encryptedAesKey, RSAEncryptionPadding.Pkcs1);  // 使用RSA私钥解密AES密钥
                    byte[] aesIV = _rsa.Decrypt(encryptedAesIV, RSAEncryptionPadding.Pkcs1);  // 使用RSA私钥解密AES初始化向量

                    _aes.Key = aesKey;  // 将解密得到的AES密钥设置为_aes的密钥
                    _aes.IV = aesIV;  // 将解密得到的AES初始化向量设置为_aes的初始化向量

                    byte[] encryptedData = new byte[fileStream.Length - fileStream.Position];  // 创建一个字节数组，用于存储加密的绘图数据
                    fileStream.Read(encryptedData, 0, encryptedData.Length);  // 从文件中读取加密的绘图数据

                    ICryptoTransform decryptor = _aes.CreateDecryptor();  // 创建一个AES解密器
                    byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);  // 使用AES解密器对绘图数据进行解密

                    using (MemoryStream ms = new MemoryStream(decryptedData))  // 创建一个内存流，用于存储解密后的绘图数据
                    {
                        _canvas = new Bitmap(ms);  // 从内存流中创建一个位图，用于存储解密后的绘图
                    }

                    this.Invalidate();  // 使窗体无效，窗体在下一次重绘操作时会自动重绘解密后的绘图
                    _isEncrypted = false;  // 将_isEncrypted标志设置为false，表示当前没有加密的绘图
                    return true;  // 返回true，表示解密操作成功
                }
            }

            return false;  // 如果用户在打开文件对话框中点击了"Cancel"按钮，返回false，表示解密操作失败
        }
    }
}
