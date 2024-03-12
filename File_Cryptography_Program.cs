using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace fileCryptography
{
    public partial class FileCryptography : Form
    {
        public FileCryptography()
        {
            InitializeComponent();
            btnBrowse.Click += new EventHandler(btnBrowse_Click);
            btnEncrypt.Click += new EventHandler(btnEncrypt_Click);
            btnDecrypt.Click += new EventHandler(btnDecrypt_Click);

            // Prevent the form from being resizable
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            // Optional: Disable the maximize box
            this.MaximizeBox = false;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            string inputFile = txtFilePath.Text;
            string outputFile = inputFile + ".encrypted";
            string password = txtPassword.Text;

            // Validate password
            if (!IsValidPassword(password))
            {
                MessageBox.Show("Invalid password. Please ensure it meets the minimum length requirement.");
                return;
            }
            try
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] salt = new byte[16]; // 128 bits
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                using (Aes aes = Aes.Create())
                {
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, salt, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);

                    using (FileStream fsCrypt = new FileStream(outputFile, FileMode.Create))
                    {
                        fsCrypt.Write(salt, 0, salt.Length); // Prepend the salt to the file

                        using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                            {
                                int data;
                                while ((data = fsIn.ReadByte()) != -1)
                                {
                                    cs.WriteByte((byte)data);
                                }
                            }
                        }
                    }
                }
                MessageBox.Show("Encryption Complete");

                // Clear the fields after successful encryption
                txtFilePath.Clear();
                txtPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Encryption failed: " + ex.Message);
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            string inputFile = txtFilePath.Text;
            string outputFile = inputFile.EndsWith(".encrypted") ? inputFile.Substring(0, inputFile.Length - 10) : inputFile + ".decrypted";
            string password = txtPassword.Text;

            // Validate password
            if (!IsValidPassword(password))
            {
                MessageBox.Show("Invalid password. Please ensure it meets the minimum length requirement.");
                return;
            }
            try
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] salt = new byte[16]; // Match the size used during encryption

                using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    fsCrypt.Read(salt, 0, salt.Length); // Read the salt from the file

                    using (Aes aes = Aes.Create())
                    {
                        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(passwordBytes, salt, 1000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);

                        using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                            {
                                int data;
                                while ((data = cs.ReadByte()) != -1)
                                {
                                    fsOut.WriteByte((byte)data);
                                }
                            }
                        }
                    }
                }
                MessageBox.Show("Decryption Complete");

                // Clear the fields after successful decryption
                txtFilePath.Clear();
                txtPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Decryption failed: " + ex.Message);
            }
        }

        // Password validation method
        private bool IsValidPassword(string password)
        {
            // Define a minimum password length
            const int MinPasswordLength = 8;

            // Check if the password meets the length requirement
            return !string.IsNullOrWhiteSpace(password) && password.Length >= MinPasswordLength;
        }

       
    }
}
