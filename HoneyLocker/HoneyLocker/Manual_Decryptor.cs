using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace HoneyLocker
{
    public partial class Manual_Decryptor : Form
    {
        public Manual_Decryptor()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bin Files (*.bin)|*.bin";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                label3.Text = selectedFilePath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bin Files (*.bin)|*.bin";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                label5.Text = selectedFilePath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Honey Files (*.Honey)|*.Honey";
                openFileDialog.Title = "Select encrypted file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string encryptedFilePath = openFileDialog.FileName;
                    string directoryPath = Path.GetDirectoryName(encryptedFilePath);

                    string keyFilePath = label3.Text; // Replace with the label3 control that holds the key file path
                    string ivFilePath = label5.Text; // Replace with the label5 control that holds the IV file path

                    byte[] key = File.ReadAllBytes(keyFilePath);
                    byte[] iv = File.ReadAllBytes(ivFilePath);

                    string decryptedFileName = Path.GetFileNameWithoutExtension(encryptedFilePath).Replace(".Honey", "");
                    string decryptedFilePath = Path.Combine(directoryPath, decryptedFileName);

                    try
                    {
                        using (Aes aes = Aes.Create())
                        {
                            aes.Key = key;
                            aes.IV = iv;

                            using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (FileStream decryptedFileStream = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                                {
                                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                                    {
                                        using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, decryptor, CryptoStreamMode.Read))
                                        {
                                            cryptoStream.CopyTo(decryptedFileStream);
                                        }
                                    }
                                }
                            }

                            File.Delete(encryptedFilePath);

                            MessageBox.Show("File decrypted successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error occurred while decrypting the file: " + ex.Message);
                    }
                }
            }
        }
        private void label8_Click(object sender, EventArgs e)
        {

        }
    }
}
