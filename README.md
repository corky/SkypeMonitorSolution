# SkypeMonitor

This is a set of projects for windows 10, written in CSharp to monitor your Skype status and update a Raspberry Pi connected to 
some multi-colored LEDs to give a "phsyical" representation of your current status.   

I used this solution when I work from home to keep my family updated on my status and when I am unavailable to talk to because I am
in a meeting.


![alt tag](https://raw.github.com/corky/SkypeMonitorSolution/master/HowItWorks.png)

There are 3 projects in this repository

####SkypeMonitor 
This project runs on your windows 10 desktop which has Skype running.  It retrieves your skype "presence" status and will update the locally hosted Http Rest Server with it and optionally update a Azure hosted rest service with the latest status.

####RaspPiSkypeClient2 
This project runs on the Raspberry Pi with a windows 10 IOT Core image on it.   It will periodically poll a rest service (locally hosted on the SkypeMonitor app, or the Azure deployment of the third project), and update its set of LED lights depending on the value of your skype status (Online - Green, Do Not Disturb - Red, Away - Yellow)

####SkypeAzureRestService
(Optional) This project is meant to run in IIS on an Azure deployed server.   It allows you to offload the rest server piece of this application to the cloud to a) improve performance and b) eliminate the possiblity of IP address changing on the client devices (Desktop computer, Raspberry PI) requiring a code change and re-deployment

###How to Deploy
####Option A (Local Network)
#####Bill Of Materials: 
* Local windows desktop running skype
* Raspberry Pi 2
* SparkFun Tri-Color LED Breakout Kit

[Connections from breakout board to Pi](https://raw.github.com/corky/SkypeMonitorSolution/master/PiWithLEDs.png)

1. Compile/Run the SkypeMonitor project on a desktop running Windows and Skype.
  * Using Visual Studio Community Edition, load the SkypeMonitor.sln file
  * In the Solution Explorer, right click the Solution "SkypeMonitor" and choose build solution
  * If you have problems locating the reference Skype4Com it is typically located in C:\Program Files\Common Files\Skype
  * Once successful on building, execute the SkypeMonitor.exe you just built.
  * The first time you run the SkypeMonitor.exe successfully it wont start until you confirm a security message in skype allowing the SkypeMonitor.exe to communicate with Skype.  Click 'Allow'.
  * Make sure to run the SkypeMonitor.exe as "administrator" to allow access to create the http server and bind to port 8081.
  * If you are running windows firewall, you will need to create a new "inbound rule" on port 8081 and allow all traffic.
2. Configure/Compile/Deploy the RaspPiSkypeClient application 
  * Update the MainPage.xaml.cs with the IP address of your computer running the SkypeMonitor.exe from step 1.
  * In the Solution Explorer, right click the Solution "SkypeMonitor" and choose build solution.
  * Follow the directions [here](http://ms-iot.github.io/content/en-US/GetStarted.htm) for deploying to your Raspberry Pi.
     * Highlights:  (From the Project Properties, Debug window)
     * Choose Debug Configuration, ARM Platform, Target Device: Raspberry PI running Windows 10, Uncheck "Use Authentication"
     * Right Click project and choose "Deploy"
3. Run the Raspberry Pi client
  * Start the Windows 10 IOT Core Watcher
  * Right click your Raspberry PI and choose "Web Browser Here"
  * Click Apps
  * Under Installed Apps find "RaspPiSkypeClient2_1.0.0.0_arm_<blah>" highlight it and click "start"
4. LED lights should now light up based on your skype status
5. Install PI and LEDs in a location (In wall near office door) to be viewed

####Option B (Azure Deployment)
#####Bill of Materials
* Local windows desktop running skype
* Raspberry Pi 2
* Azure deployed IIS server
* SparkFun Tri-Color LED Breakout Kit

[Connections from breakout board to Pi](https://raw.github.com/corky/SkypeMonitorSolution/master/PiWithLEDs.png)

1. Build/Deploy SkypeAzureRestService to a Azure deployed server
  * Documentation for this step can be found [here](https://azure.microsoft.com/en-us/documentation/articles/web-sites-deploy/)
  * Make note of the IP/Port address of your Azure server deployment to use in step 2 and 3
  * Test with rest client application like Fiddler or Postman.
2. Compile/Run the SkypeMonitor project on a desktop running Windows and Skype.
  * Using Visual Studio Community Edition, load the SkypeMonitor.sln file
  * Find the SkypeMonitor/Form1.cs and update the configuration values with the IP address and port of your Azure server deployment from step 1
  * In the Solution Explorer, right click the Solution "SkypeMonitor" and choose build solution
  * If you have problems locating the reference Skype4Com it is typically located in C:\Program Files\Common Files\Skype
  * Once successful on building, execute the SkypeMonitor.exe you just built.
  * The first time you run the SkypeMonitor.exe successfully it wont start until you confirm a security message in skype allowing the SkypeMonitor.exe to communicate with Skype.  Click 'Allow'.
  * The application will launch minimized in the system tray.   Click on the application in the system tray to reshow it and checkbox the "Post status to Azure service".
  * Minimize application back to the system tray.
3. Configure/Compile/Deploy the RaspPiSkypeClient application 
  * Uncomment out the Option B line in the MainPage.xaml.cs (under configuration) with the IP address and port of your Azure server running SkypeAzureRestService from step 1.
  * In the Solution Explorer, right click the Solution "SkypeMonitor" and choose build solution.
  * Follow the directions [here](http://ms-iot.github.io/content/en-US/GetStarted.htm) for deploying to your Raspberry Pi.
     * Highlights:  (From the Project Properties, Debug window)
     * Choose Debug Configuration, ARM Platform, Target Device: Raspberry PI running Windows 10, Uncheck "Use Authentication"
     * Right Click project and choose "Deploy"
4. Run the Raspberry Pi client
  * Start the Windows 10 IOT Core Watcher
  * Right click your Raspberry PI and choose "Web Browser Here"
  * Click Apps
  * Under Installed Apps find "RaspPiSkypeClient2_1.0.0.0_arm_<blah>" highlight it and click "start"
5. LED lights should now light up based on your skype status
6. Install PI and LEDs in a location (In wall near office door) to be viewed

###Troubleshooting
* Make sure to run the SkypeMonitor.exe as "administrator" to allow access to create the http server and bind to port 8081.
* If you are running windows firewall, you will need to create a new "inbound rule" on port 8081 and allow all traffic.
* If you launch SkypeMonitor.exe and nothing happens, make sure to check Skype to see if there is a permission dialog allowing the SkypeMonitor to communicate with Skype.  Click Allow.
* Use a tool like Fiddler or Postman to test the endpoints of each application (desktop computer running SkypeMonitor on port 8081 and Azure deployment on port 8080)

#####SkypeMonitor Test
GET /currentstatus
Host: IP-ADDRESS-OF-LOCAL-DESKTOP:8081

#####Azure Deployment Test
POST /SkypeStatus.svc/currentstatus
Host: <IP OF AZURE SERVER>:8080
{
    "Status": "test"
}

GET /SkypeStatus.svc/currentstatus
Host: IP-OF-AZURE-SERVER:8080
