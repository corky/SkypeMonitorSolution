using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Devices.Gpio;
using System.Net;
using System.Runtime.Serialization.Json;

// This application runs on the Raspberry PI and is the client to the rest service which hosts the current
// Skype status.
namespace RaspPiSkypeClient2
{
    public sealed partial class MainPage : Page
    {
        //REST Service Configuration
        //Below this is the string to be used to hit the local network deployed (ie form app) REST service.
        //Option A Deployment
        //Uncomment out this line and replace IP address of your windows 10 box running skype monitor 
        private String statusService = "http://<IP ADDRESS OF DESKTOP RUNNING SKYPE/SKYPEMONITOR>:8081/currentstatus";
        //Below this is the string to be used to hit the Azure deployed REST service.
        //Uncomment out this line (and make sure the above line is commented out)
        //  and replace IP address of your Azure server to which you deployed
        //  the SkypeAzureRestService only if using Option B deployment
        //private String statusService = "http://<IP ADDRESS OF AZURE SERVER>:8080/SkypeStatus.svc/currentstatus";

        //PI LED Pin Configuration
        private const int GREEN_LED_PIN = 27;
        private const int RED_LED_PIN = 22;
        private const int YELLOW_LED_PIN = 18;

        //Global Variables
        private GpioPin REDpin;
        private GpioPin YELLOWpin;
        private GpioPin GREENpin;
        private DispatcherTimer timer;
        private DispatcherTimer timer2;
        private WebRequest wrGETURL;
        private static String responseString = String.Empty;

        //UI Features
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
        private SolidColorBrush greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);
        private SolidColorBrush yellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        
        //GPIO Constants
        private GpioPinValue LO = GpioPinValue.Low;
        private GpioPinValue HI = GpioPinValue.High;
        
        //Initial Startup Method
        public MainPage()
        {
            this.InitializeComponent();
            //setup Timer 1 (Poller for Rest Service)
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Tick += Timer_Tick;
            //Setup Timer 2 (UI Refresher)
            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(500);
            timer2.Tick += Timer2_Tick;
            
            //setup LED Breakout Board
            InitGPIO();

            //Initiate Timers if LED Breakboard initialized successfully
            if (REDpin != null && YELLOWpin != null && GREENpin != null)
            {
                timer.Start();
                timer2.Start();
            }
        }

        private void InitGPIO()
        {
            GpioPinValue pinValue;

            //setup the Windows 10 GPIO COntroller
            var gpio = GpioController.GetDefault();
            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                REDpin = null;
                GREENpin = null;
                YELLOWpin = null;
                LastResponse.Text = "There is no GPIO controller on this device.";
                return;
            }

            //Setup the Pins to control the LED Breakout Board, initialize to Off
            REDpin = gpio.OpenPin(RED_LED_PIN);
            YELLOWpin = gpio.OpenPin(YELLOW_LED_PIN);
            GREENpin = gpio.OpenPin(GREEN_LED_PIN);
            pinValue = GpioPinValue.Low;
            REDpin.Write(pinValue);
            YELLOWpin.Write(pinValue);
            GREENpin.Write(pinValue);
            REDpin.SetDriveMode(GpioPinDriveMode.Output);
            YELLOWpin.SetDriveMode(GpioPinDriveMode.Output);
            GREENpin.SetDriveMode(GpioPinDriveMode.Output);
        }

        //Fires in a loop every so often (see interval above) to poll the REST service to get the
        //latest Skype status
        private void Timer_Tick(object sender, object e)
        {
            //setup an Asynchronous web call to the REST server to retrieve the Skype Status
            //  THis method will initiate the call, and setup the Callback when the call complets
            //  so we can process the result 
            string sURL;
            sURL = statusService;
            wrGETURL = WebRequest.Create(sURL);
            wrGETURL.BeginGetResponse(new AsyncCallback(FinishWebRequest), null);
        }

        //Fires in a loop every so often (see interval above) to update the UI and control the LED
        //  breakout board and change the color to the current Skype status from the REST service
        private void Timer2_Tick(object sender, object e)
        {
            //update UI
            LastResponse.Text = "result:" + responseString;
            switch (responseString)
            {
                case "Online":
                    //turn on Green LED light
                    enableDisableLights(HI, LO, LO, greenBrush);
                    break;
                case "Away":
                    //turn on yellow LED light (which is a combo of RED and GREEN)
                    enableDisableLights(HI, HI, LO, yellowBrush);
                    break;
                case "Do Not Disturb":
                    //Turn on Red LED light
                    enableDisableLights(LO, HI, LO, redBrush);
                    break;
                default:
                    //default behavior, if we dont understand the status value, turn all lights off
                    enableDisableLights(LO, LO, LO, grayBrush);
                    break;
            }
        }

        //Callback method when REST call completes.   This will process the JSON response from the REST 
        //service and set the appropriate Skype Status for the UI timer to read and process.
        void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                //get the Response
                WebResponse resp = wrGETURL.EndGetResponse(result);
                //Attempt to take string returned from REST call and turn into an object
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(StatusData));
                object objResp = jsonSerializer.ReadObject(resp.GetResponseStream());
                StatusData myStatus = objResp as StatusData;
                //set the global variable of the skype status for the UI poller (timer 2) to update the UI
                //and set the LED lights.
                responseString = myStatus.Status;
            }
            catch (Exception ex)
            {
                //there was an error processing the REST service request.   Turn all lights off.
                responseString = "";
            }
        }

        //simple method to centralize all handling of of enabling/disabling pins for the LED lights
        // and updating the UI.
        public void enableDisableLights(GpioPinValue GreenLED, GpioPinValue RedLED, GpioPinValue YellowLED, SolidColorBrush currentColor)
        {
            GREENpin.Write(GreenLED);
            REDpin.Write(RedLED);
            YELLOWpin.Write(YellowLED);
            LED.Fill = currentColor;
        }
    }
}
