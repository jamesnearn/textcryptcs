﻿using System;
using System.IO;
using System.Windows.Forms;

namespace TextCrypt
{
    public partial class frmYourKeys : Form
    {
        Crypto crypto;
        frmGetNewKey getNewKey;
        frmGetPassword2 password2;

        // Shared variable
        public string keyStorePath { get; set; }

        // Initialize form
        public frmYourKeys()
        {
            InitializeComponent();

            crypto = new Crypto();
            getNewKey = new frmGetNewKey();
            password2 = new frmGetPassword2();

            // Event for Combo Box Select
            this.cmbKeyPairName.SelectedIndexChanged += new System.EventHandler(cmbKeyPairName_SelectedIndexChanged);

            // Event to refresh form on activate
            this.Activated += frmYourKeys_Activated;
        }

        // Copy public key to clipboard
        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (txtPublicKey.Text != String.Empty) Clipboard.SetText(txtPublicKey.Text);
        }

        // Update key name list
        private void updateKeyPairList()
        {
            cmbKeyPairName.Items.Clear();
            cmbKeyPairName.Text = String.Empty;
            txtPublicKey.Text = String.Empty;

            try
            {

                string[] list = Directory.GetFiles(keyStorePath, "*.prvkey");
                foreach (string item in list)
                {
                    string nameOnly = Path.GetFileNameWithoutExtension(item);
                    cmbKeyPairName.Items.Add(nameOnly);
                }

                if (cmbKeyPairName.Items.Count > 0) cmbKeyPairName.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("Failed to locate the Key Store directory!", "Key Store Error");
                this.Close();
                return;
            }
        }

        // Update Public Key Window
        private void cmbKeyPairName_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            try
            {
                string[] pKey = File.ReadAllLines(keyStorePath + cmbKeyPairName.Text + ".pubkey");

                txtPublicKey.Text = String.Empty;
                foreach (string line in pKey)
                {
                    txtPublicKey.AppendText(line);
                    txtPublicKey.AppendText(Environment.NewLine);
                }
            }
            catch { return; }
        }

        // Add new Key Pair
        private void btnAddKeyPair_Click(object sender, EventArgs e)
        {
            Tuple<string, string> kp;
            string keyName;

            // Show the GetNewKey dialog
            getNewKey.ShowDialog();
            keyName = getNewKey.newKey;
            if (keyName == String.Empty) return;

            // Show the Get Password Dialog
            password2.ShowDialog();
            string password = password2.Password;
            if (password == String.Empty) return;

            // Generate RSA Key Pair
            kp = crypto.RSACreateKeyPair();

            // Store the key pairs
            if (crypto.RSAStorePublicKey(keyStorePath + keyName + ".pubkey", kp.Item2)==1)
            {
                MessageBox.Show("Failed to store RSA Public Key", "RSA Key Error");
                return;
            }
            if (crypto.RSAStorePrivateKey(keyStorePath + keyName + ".prvkey", kp.Item1, password)==1)
            {
                MessageBox.Show("Failed to store RSA Private Key", "RSA Key Error");
                return;
            }

            MessageBox.Show("Key Pair Added!", "Key Pair Status");
            updateKeyPairList();
        }

        // Update Key Pair
        private void UpdateKeyPair_Click(object sender, EventArgs e)
        {
            Tuple<string, string> kp;
            string keyName;

            //Get name from dropdown
            keyName = cmbKeyPairName.Text;

            if (keyName == String.Empty) return;

            // Show Warning
            DialogResult yesno = MessageBox.Show("Are you sure? This will forever alter the existing Key Pair and any text Encrypted using this Key Pair will be forever lost!", "Warning", MessageBoxButtons.YesNo);

            // Bail out if no
            if (yesno == DialogResult.No) return;

            // Show the Get Password Dialog
            password2.ShowDialog();
            string password = password2.Password;

            // Generate RSA Key Pair
            kp = crypto.RSACreateKeyPair();

            // Store the key pairs
            if (crypto.RSAStorePublicKey(keyStorePath + keyName + ".pubkey", kp.Item2) == 1)
            {
                MessageBox.Show("Failed to store RSA Public Key", "RSA Key Error");
                return;
            }
            if (crypto.RSAStorePrivateKey(keyStorePath + keyName + ".prvkey", kp.Item1, password) == 1)
            {
                MessageBox.Show("Failed to store RSA Private Key", "RSA Key Error");
                return;
            }

            MessageBox.Show("Key Pair Updated!","Key Pair Status");
            updateKeyPairList();
        }

        // Delete Key Pair
        private void btnDeleteKeyPair_Click(object sender, EventArgs e)
        {
            string keyName;

            //Get name from dropdown
            keyName = cmbKeyPairName.Text;
            if (keyName == String.Empty) return;

            // Show Warning
            DialogResult yesno = MessageBox.Show("Are you sure? This will delete the existing Key Pair and any text Encrypted using this Key Pair will be forever lost!", "Warning", MessageBoxButtons.YesNo);

            // Bail out if no
            if (yesno == DialogResult.No) return;

            // Delete the files
            try
            {
                File.Delete(keyStorePath + keyName + ".pubkey");
                File.Delete(keyStorePath + keyName + ".prvkey");
            }
            catch
            {
                MessageBox.Show("Key Pair delete failed!", "Key Pair Status");
                return;
            }

            MessageBox.Show("Key Pair Deleted!", "Key Pair Status");

            updateKeyPairList();
        }

        // Update the drop down when form activated
        private void frmYourKeys_Activated(object sender, EventArgs e)
        {
            updateKeyPairList();
        }
    }
}