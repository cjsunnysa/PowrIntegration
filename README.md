About
=====
PowrIntegration is a suite of containerized [Docker](https://www.docker.com) applications which provide integration functionality between Powertill point-of-sale software and the Zambian Revenue Authority to track VAT collection.

It is comprised of the follwing services:
- backoffice - stores and transforms data from Powrtill and between itself and the zra service.
- zra - consumes messages from the backoffice service, communicates with the ZRA VSDC API and publishes messages to the backoffice service.
- rabbitmq - provides queues for asynchronous communication:
  - BackOffice - queue for messages intended for processing by the backoffice service.
  - BackOfficeDead - a dead-letter queue where messages that cannot be processed are placed.
  - Zra - queue for messages intended for processing by the zra service.
  - ZraDead - a dead-letter queue where messages that cannot be processed are placed.
- prometheus - provides a store for application and resource metrics.
- loki - provides a store for metadata indexed logs.
- grafana - provides a portal for creating dashboards, querying log and metric data, configuring and executing alert notifications.

Windows Installation
====================

ZRA VSDC
--------
* Install the latest Java Runtime Environment (JRE) - https://www.java.com/en/download/manual.jsp
* Install Apache Tomcat (web server) - https://tomcat.apache.org/download-11.cgi
* Copy the VSDC WAR file to `C:\Program Files (x86)\Apache Software Foundation\Tomcat 9.0\webapps`
* Right click the Apache Tomcat icon in the application tray and select restart Tomcat.

PowrIntegration
---------------
* Download the PowrIntegration.zip archive from the Releases section of this repository.
* Extract to a folder eg. `C:\Containers\PowrIntegration`
* Open Powershell as adminstrator.
* Change directory to the where you extracted the zip file.
`cd C:\Containers\PowrIntegration`
* Enable execution of unsigned scripts
`Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Unrestricted`
* Run the install.ps1 script
`.\install.ps1`

The installation will execute the following in order:
* Are you running the script with Administrator priveleges. Will fail if not.
* Is the Windwos Subsystem for Linux feature enabled. Will enable if not. This is required for running Linux based containers on Windows.
* Is Docker Desktop Installed. Will download and install if not. This is required to host and orchestrate Docker containers.
* A restart will happen after Docker Desktop is installed. When the machine is restarted open a terminal as Administrator and run the install.ps1 script again.
* Is Docker Desktop running. Will start Docker Desktop if not.
* Is Docker Desktop Swarm mode enabled. Will enable if not. This is required for automatic restarting of containers if they fail and horizontal scaling of containers to meet processing demand.
* Is the PowrIntegration stack already deployed. Will remove if so.
* Are any ports required by the PowrIntegration stack of applications already in use. Will report on ports in use and fail if so.
* Is the PowrIntegration stack running. Will deploy and run if not.

Grafana
-------
Detailed documentation can be found here: https://grafana.com/docs/grafana/latest/

All logs can be viewed using the Grafana application which is opened at http://localhost:3000
* The default credentials are admin:admin
* You can explore the logs from the menu on the left side under the Explore item:
![image](https://github.com/user-attachments/assets/5b463e20-f049-496a-99bd-88c789340c4c)
* You can drill into logs for each service by clicking the 'show logs' button for each service.
* Errors are show with a red margin. Other logs are shown with a green margin.
* You can click on the individual log line to expand the details.

RabbitMQ
--------
The message queues can be viewed at http://localhost:15672/#/queues
* The default credentials are guest:guest
* The PowrIntegration application uses 4 queues: BackOffice, BackOfficeDead, Zra, ZraDead
![image](https://github.com/user-attachments/assets/b8211da7-2435-4c48-b487-d615484c1832)
* The Dead queues are used for storing messages which the application fails to process.

Back-Office and Zra Services
----------------------------
* Configuration for these service can be updated in the docker-compose.yml file found in the release PowrIntegration.zip archive.
![image](https://github.com/user-attachments/assets/6e8a9595-85c4-4c47-ad8d-8bd104fac619)
* After you edit these config settings run the restart-services.ps1 script to restart all of the deployed containers.

Create Item Sequence Diagram
----------------------------
![CreateItemSequenceDiagramMermaid](https://github.com/user-attachments/assets/b257c0ec-899d-4c9c-9295-253627d0a9a8)

