using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SKYPE4COMLib;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections;

//This application is a simple windows form application that runs in the background of your windows box 
//which is running the Skype client.   This app also either 
//  a) hosts the Http Rest service (if using the local area network option
//  b) posts to the Http Rest service when the status changes (if using the Azure deployed option)
namespace SkypeMonitor
{
    public partial class Form1 : Form
    {
        //Configuration values
        //Port to run local network Rest service on
        String localServerPort = "8081";
        //Location of SkypeAzureRestService deployed on Azure (if using Azure option)
        String AzureURL = "http://192.168.200.40:8080/SkypeStatus.svc/currentstatus";

        //Global Variables
        HttpListener listener;
        Skype skype;
        NotifyIcon notifyIcon;
        Thread listenThread1;
        String currentStatus;
        Boolean continueThread;
        private WebRequest wrPost;

        public Form1()
        {
            InitializeComponent();
        }

        //This method runs when the program starts up
        private void Form1_Load(object sender, EventArgs e)
        {
            //Initial Skype Desktop API
            skype = new Skype();
            skype.Attach(7, true);
            //Hookup local event method to run when Skype status changes
            skype.OnlineStatus += Skype_OnlineStatus;
            //Retrieve the current status of the skype client
            TOnlineStatus myStatus = skype.CurrentUser.OnlineStatus;

            //Turn Skype status into a String
            label1.Text = getStatusString(myStatus);
            
            //Setup system tray icon (making the app able to run in the background) and not on the task bar
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.BalloonTipText = "located in system tray";
            notifyIcon.BalloonTipTitle = "Skype Status Monitor for Raspberry Pi";
            notifyIcon.Text = "Skype Status Monitor for Raspberry Pi";
            notifyIcon.Icon = SkypeMonitor.Properties.Resources.MyIcon;
            notifyIcon.Click += new EventHandler(HandlerToMaximiseOnClick);
            if (notifyIcon != null)
            {
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(2000);
            }
            //Minimize the app to the system tray (running in the background to be less obtrusive to user)
            this.WindowState = FormWindowState.Minimized;

            //setup local area network Rest Service
            //retrieve the local IP addresses to bind the HttpService to
            ArrayList myIps = new ArrayList();
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    myIps.Add( ip.ToString());
                }
            }
            //Setup Http Server
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + localServerPort + "/");
            listener.Prefixes.Add("http://127.0.0.1:" + localServerPort + "/");
            foreach (String IP in myIps)
            {
                listener.Prefixes.Add("http://" + IP + ":" + localServerPort + "/");
            }
            //No Authentication needed
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            //Spawn additional thread and run HttpService in that background thread
            continueThread = true;
            listener.Start();
            this.listenThread1 = new Thread(new ParameterizedThreadStart(startlistener));
            listenThread1.Start();
            //setup closing method to dispose of thread properly
            this.Disposed += Form1_Disposed;
        }

        //Method called when app is shut down.   
        private void Form1_Disposed(object sender, EventArgs e)
        {
            //Stop Http Service
            listener.Stop();
            continueThread = false;
        }

        
        //This method is called when the app is told to disappear (Minimize)
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        //Method to turn the Skype status to a readable string
        private String getStatusString(TOnlineStatus status)
        {
            String retValue = String.Empty;
            switch(status)
            {
                case TOnlineStatus.olsNotAvailable:
                    retValue = "Not Available";
                    break;
                case TOnlineStatus.olsOnline:
                    retValue = "Online";
                    break;
                case TOnlineStatus.olsDoNotDisturb:
                    retValue = "Do Not Disturb";
                    break;
                case TOnlineStatus.olsAway:
                    retValue =  "Away";
                    break;
                default:
                    retValue = "Unknown";
                    break;
            }
            currentStatus = retValue;
            return retValue;
        }

        //Event handler that fires when the Skype status changes.   
        private void Skype_OnlineStatus(User pUser, TOnlineStatus Status)
        {
            //get the current status
            TOnlineStatus myStatus = skype.CurrentUser.OnlineStatus;
            //turn status into a string
            label1.Text = getStatusString(myStatus);
            //if the user is using the Azure deployed service, post updated status to Azure service
            if(checkBox1.Checked==true)
            {
                postStatusToAzure();
            }
        }

        //Method to fire when application is clicked on in the system tray (Change the state from visuable UI
        // to mimized and invisible)
        private void HandlerToMaximiseOnClick(Object obj, EventArgs args)
        {
            if (this.WindowState == FormWindowState.Normal || this.WindowState==FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Minimized;
                this.Show();

            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.Hide();

            }
        }

        //Method to start up the local area network Http Rest Service
        private void startlistener(object s)
        {
            while (continueThread)
            {
                ////blocks until a client has connected to the server
                ProcessRequest();
            }
        }

        //process and incoming request from the Raspberry Pi to get the current status of Skype
        private void ProcessRequest()
        {
            var result = listener.BeginGetContext(ListenerCallback, listener);
            result.AsyncWaitHandle.WaitOne();
        }

        // Event handler for Http Service used to send the current status to the Raspberry Pi
        private void ListenerCallback(IAsyncResult result)
        {
            if (listener != null && listener.IsListening)
            {
                //Respond with JSON response of the string representation of the Skype status
                var context = listener.EndGetContext(result);
                Thread.Sleep(1000);
                var data_text = new StreamReader(context.Request.InputStream,
                context.Request.ContentEncoding).ReadToEnd();

                context.Response.StatusCode = 200;
                context.Response.StatusDescription = "OK";

                byte[] buffer = Encoding.UTF8.GetBytes("{\"Status\":" + "\"" + currentStatus + "\""+ "}");
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.Close();
            }
        }

        //Method to send a REST request POST to the Azure service to update the Skype status
        private void postStatusToAzure()
        {
            try {
                //setup http request
                wrPost = WebRequest.Create(AzureURL);
                wrPost.Method = "POST";
                wrPost.ContentType = "application/json";
                //setup body of request to be the JSON payload of the Skype Status
                using (var streamWriter = new StreamWriter(wrPost.GetRequestStream()))
                {
                    string json = "{\"Status\":\"" + currentStatus + "\",}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                //send the request
                var httpResponse = (HttpWebResponse)wrPost.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
                //update UI with response from the Azure service
                label2.Text = "Last Response: " + httpResponse.StatusCode.ToString();
            }
            catch(Exception ex)
            {
                //Error posting to Azure service, update UI with error message
                label2.Text = "Last Response: " + ex.Message;
            }
        }
    }
}
