using MOLWallet_SDK;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace MOLWalletDemo
{
    public partial class MobileQRPayment : Form
    {
        static string TxnId;
        MOLWallet molWallet;
        string amount, desc;
        DispatcherTimer dispatcherTimer;
        int currentTime;
        int delay;
        public delegate void RequestPaymentDelegate(string phoneNumber);
        private RequestPaymentDelegate RequestPayment;


        public MobileQRPayment(string amount, string desc)
        {
            InitializeComponent();
            RequestPayment = InitPayment;
            this.amount = amount;
            this.desc = desc;
        }

        public async void InitPayment(string phoneNumber)
        {
            try
            {
                Dictionary<MOLWallet_SDK.Keys, string> extraDic = new Dictionary<MOLWallet_SDK.Keys, string>();
                extraDic.Add(MOLWallet_SDK.Keys.MerchantId, "merchantId");
                extraDic.Add(MOLWallet_SDK.Keys.AppName, "AppName");
                extraDic.Add(MOLWallet_SDK.Keys.TerminalId, "100000000");
                extraDic.Add(MOLWallet_SDK.Keys.VerifyKey, "verifyKey");
                extraDic.Add(MOLWallet_SDK.Keys.BillName, "Wallet user");
                extraDic.Add(MOLWallet_SDK.Keys.BillMobile, phoneNumber);
                extraDic.Add(MOLWallet_SDK.Keys.BillDescription, desc.Trim());
                extraDic.Add(MOLWallet_SDK.Keys.Currency, "MYR");
                extraDic.Add(MOLWallet_SDK.Keys.Country, "MY");
                extraDic.Add(MOLWallet_SDK.Keys.Amount, amount.Trim());
                extraDic.Add(MOLWallet_SDK.Keys.UserName, "username");
                extraDic.Add(MOLWallet_SDK.Keys.Password, "password");

                molWallet = new MOLWallet();
                string result = await molWallet.GetPayment(extraDic, PaymentMode.Mobile);
                JObject jResult = JObject.Parse(result);

                var status = jResult["status"].ToString();
                if (status.Equals("true"))
                {
                    TxnId = jResult["txn_ID"].ToString();
                    extraDic.Add(MOLWallet_SDK.Keys.TransactionId, TxnId);
                    //result = await molWallet.CheckPaymentStatus(extraDic);
                    //jResult = JObject.Parse(result);
                    lbStatus.Text = "Waiting for payment!";
                    //  DispatcherTimer setup
                    dispatcherTimer = new DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                    currentTime = 600;
                    delay = 4;
                    dispatcherTimer.Start();
                    lbExpiresIn.Visible = true;
                    lbTimer.Visible = true;
                    //lbStatus.Text = jResult["status_code"].ToString();
                }
                else {
                    lbStatus.Text = "Request payment faild!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            currentTime--;
            delay--;
            lbTimer.Text = string.Format("{0:00}:{1:00}", currentTime / 60, currentTime % 60);
            if (delay == 0)
            {
                CheckPayment();
                delay = 4;
            }
            
            if (currentTime == 0)
            {
                dispatcherTimer.Stop();
                dispatcherTimer = null;
                lbTimer.Text = "Timer for payment is expired. Please rescan!";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CheckPayment();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InitPayment(textBox1.Text.Trim());
            lbStatus.Text = "Payment request is sent";
        }

        private async void CheckPayment()
        {
            try
            {
                Dictionary<MOLWallet_SDK.Keys, string> extraDic = new Dictionary<MOLWallet_SDK.Keys, string>();
                extraDic.Add(MOLWallet_SDK.Keys.MerchantId, "merchantid");
                extraDic.Add(MOLWallet_SDK.Keys.VerifyKey, "verifyKey");
                extraDic.Add(MOLWallet_SDK.Keys.TransactionId, TxnId);
                extraDic.Add(MOLWallet_SDK.Keys.Amount, amount.Trim());
                var result = await molWallet.CheckPaymentStatus(extraDic);
                var jResult = JObject.Parse(result);
                var statusCode = jResult["status_code"].ToString();
                switch (statusCode)
                {
                    case "00":
                        //Successfull
                        lbStatus.Text = "Payment successfuly completed";

                        await Task.Delay(3000);
                        lbExpiresIn.Visible = false;
                        lbTimer.Visible = false;
                        break;
                    case "11":
                        //Faild
                        //lbStatus.Text = "Payment faild";
                        break;
                    case "22":
                        //Pending
                        //lbStatus.Text = "Payment is pending";
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}
