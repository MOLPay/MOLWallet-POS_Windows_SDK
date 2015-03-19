using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MOLWallet_SDK;
using Newtonsoft.Json.Linq;

namespace MOLWalletDemo
{
    public partial class Form1 : Form
    {
        string TxnId { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Payment paymentForm = new Payment(tbAmount.Text, tbDescription.Text);
            paymentForm.StartPosition = FormStartPosition.CenterParent;
            paymentForm.ShowDialog();
        }
    }
}
