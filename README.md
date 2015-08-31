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
(Optional) This project is meant to run in IIS on an Azure deployed server.   It allows you to offload the rest server piece of this application to the cloud to a) improve performance and b) eliminate the possiblity of IP address changing on the client devices (Desktop computer, Raspberry PI) requiring a code change and deployment
