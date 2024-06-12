# 基于随笔画的加解密系统
基于随笔画的加解密系统


实现一个随笔绘图的加解密系统 
A. C#语言中图形绘制的基本原理和实现方法。
B. 对称加密和非对称加密的基本概念及实现。
C. 文件读写操作的基本方法及在加解密中的应用。
D. Windows Forms应用程序的基本设计和事件处理。

完成以下主要功能：
A.菜单设计
绘图部分要求用户能够通过鼠标在窗体中进行随笔绘画。窗体菜单的功能选择应至少包括：绘图，加密，解密。同时还需设置子菜单供用户选择调整线条类型，线条颜色等。
加密部分要求提供文件保存对话框让用户选择文件名和保存位置。
解密部分要求提供文件打开对话框让用户选择加密文件。
B.加解密实现
a.生成对称密钥：可以选择使用Aes.Create()方法生成对称密钥和初始化向量（IV）。并使用AES算法对绘图数据进行加密，加密后的数据存储在文件中。
b.生成非对称密钥对：生成RSA密钥对，并使用RSA算法加密对称密钥和IV。将加密的对称密钥、IV和绘图数据写入文件。
c. 读取加密数据：读取分离出加密的对称密钥、IV和加密的绘图数据，使用RSA私钥解密对称密钥和IV。
d. 解密绘图数据：使用解密得到的对称密钥和IV解密绘图数据。进而从解密后的数据中恢复绘图点，并重新绘制图像。
