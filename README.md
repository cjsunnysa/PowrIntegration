Windows
=======

ZRA VSDC
--------
* Install the latest Java Runtime Environment (JRE) - https://www.java.com/en/download/manual.jsp
* Install Apache Tomcat (web server) - https://tomcat.apache.org/download-11.cgi
* Copy the VSDC WAR file to `C:\Program Files (x86)\Apache Software Foundation\Tomcat 9.0\webapps`
* Right click the Apache Tomcat icon in the application tray and select restart Tomcat.

PowrIntegration
---------------
* Download the folowing:
[PowrIntegration.zip](https://github.com/user-attachments/files/19622508/PowrIntegration.zip)
* Extract to a folder eg. `C:\Containers\PowrIntegration`
* Open Powershell as adminstrator.
* Change directory to the where you extracted the zip file.
`cd C:\Containers\PowrIntegration`
* Enable execution of unsigned scripts
`Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Unrestricted`
* Run the install.ps1 script
`.\install.ps1`

The installation will check for a few pre-requisites and install them if necessary:
* The Windows Sub-System for Linux (WSL2) - The containers run on a Linux operating system
* Docker Desktop - The container engine
* Enabling Docker Desktop to run in Swarm mode - A production mode for managing multiple container instances.

During the installation process a restart may be performed. When the computer has restarted: open Powershell as an Adminstrator and run the install.ps1 script again.

The installation will start a stack of the containers required for PowrIntegration. Your system is now installed and running.

RabbitMQ
--------
