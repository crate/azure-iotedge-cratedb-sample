# Azure IoT Edge CrateDB sample

This is an [Azure IoT Edge](https://azure.microsoft.com/en-us/services/iot-edge/) sample showing how to store machine data from OPC-UA servers locally in a single node [CrateDB](https://crate.io/) database. All necessary components will be deployed by Azure IoT Edge.

![Sample architecture](assets/EdgeSampleArchitecture.png)

# Configuration

To be able to run this sample on your edge device, you'll have Azure IoT Edge installed on the device, including a container runtime like Docker. The sample was developed on a Windows 10 IoT machine with Docker Desktop and Linux container. 

Since this sample was developed with Visual Studio Code, the easiest way to run the sample is by using Visual Studio Code with the according Azure IoT Edge Plugin, as used for [custom module development of Azure IoT Edge](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-csharp-module).

# Set up the Edge Device as a vagrant VM
This approach make the set up experience more streamlined.

1. Checkout this repo: https://github.com/crate/azure-iotedge-cratedb-sample
2. Install Vagrant: https://www.vagrantup.com/downloads.html
3. Install Visual Studio Code: Download Visual Studio Code
4. Install extension to Visual Studio: (On the Visual Studio Code menu, preferences, extensions)
    1. Azure IoT Hub Toolkit
    2. Azure IoT Edge
    3. Vagrant
5. You need docker installed in your system
6. Follow the steps here to enable iotedgedev on your machineSee here: https://github.com/Azure/iotedgehubdev#installing
7. Open the project folder (the one you checked out from the repository before) with Visual Studio Code
8. Create a folder “crate” in the project directory. For me it is:
/gn/git/azure-iotedge-cratedb-sample/crate
9. Open Terminal
10. Restore the project dependencies
    1. cd /gn/git/azure-iotedge-cratedb-sample/modules/cratedbsaver/
    2. dotnet restore
11. Create an IoT Hub Instance on Azure
    1. Wait for it to be in an active state, it could take a while
12. After your IoT hub is started you can use visual studio integration to connect to it (you can also sign in and select the iot hub directly)
13. In VisualStudio you can create a new test Edge Device

    1. Then right click on  the device and select “Copy Device Connection String” from the Menu. We’ll need this later
    
14. browse the vagrant folder of the project, for me it is :
    1. cd /gn/git/azure-iotedge-cratedb-sample/vagrant
15. Modify the config.yaml to use your Device ID
    1. it is in this path of the repo: vagrant/config.yaml
16. Vagrant up
17. Open the shh by running
    1. vagrant ssh
18. Restart the service inside the VM
    1. sudo systemctl restart iotedge

17. continue with the next steps

## Connect to Grafana
The Vagrantfile binds the grafana port on the edge device (3000) to the port 3300 in your system.
Just browse `http://localhost:3300` and you should see the login page. use admin as username and password

## Configuring the local database

Before starting to deploy the sample to the edge device, the local CrateDB used in the sample has to be created first. An existing CrateDB in the local network could be used, too, but this sample uses CrateDB running in a container on the same device as the Azure IoT Edge runtime. 
This setup step should be done in your laptop, and will create the information in a data folder. This data folder is then shared with the Edge module, so the tables will be there. (that's why we start and stop the container after running the SQL)

1. Create a folder on the disk for the CrateDB database files, e.g. `C:\Dev\docker\mounts\crate\data`
2. Run CrateDB locally from the command line by using `docker run -p "4200:4200" -d --rm --name cratedb -v C:\Dev\docker\mounts\crate\data:/data crate` and replace it with the folder you have created before in step 1
3. Open a web browser with `http://localhost:4200` and go to the CrateDB console
4. Create the tables and the user by executing all SQL Statements in the file `./scripts/createTable.sql`
5. Stop the container with `docker container stop cratedb`
6. Edit the files `deployment.template.json` and `deployment.debug.template.json` by changing the database folder location from `C:/Dev/docker/mounts/crate/data` to your database files location (as done in step 1)



## Configuring the OPC-UA Publisher edge module

1. Per default this sample uses all nodes from the [OPC-UA Server simulation](https://github.com/Azure-Samples/iot-edge-opc-plc). If you want to change the setup, just edit the `publishednodes.json` file in the folder `appdata`
2. Edit the files `deployment.template.json` and `deployment.debug.template.json` by changing the publisher's data folder from `C:/Dev/iotedge/crate/localstorageCrateDBEdgeSolution` to the directory you've downloaded the source repository on your local disk.


# Running the sample

Easiest way to run the sample is by using Visual Studio Code.