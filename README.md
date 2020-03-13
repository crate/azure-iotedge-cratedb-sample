# Azure IoT Edge CrateDB sample

This is an [Azure IoT Edge](https://azure.microsoft.com/en-us/services/iot-edge/) sample showing how to store machine data from OPC-UA servers locally in a single node [CrateDB](https://crate.io/) database. All necessary components will be deployed by Azure IoT Edge.

![Sample architecture](assets/EdgeSampleArchitecture.png)

To be able to run this sample on your edge device, you'll need to have [Azure IoT Edge](https://github.com/Azure/iotedgehubdev#installing) installed on the device, including a container runtime like Docker. The sample was developed on a Windows 10 IoT machine with Docker Desktop and Linux container and tested on macOS.

Since this sample was developed with Visual Studio Code, the easiest way to run the sample is by using Visual Studio Code with the according Azure IoT Edge Plugin, as used for [custom module development of Azure IoT Edge](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-csharp-module).

# Running the sample

## Requirements

* Checkout this repo `git clone https://github.com/crate/azure-iotedge-cratedb-sample.git`
* Install [Vagrant](https://www.vagrantup.com/downloads.html)
* Install [VirtualBox](https://www.virtualbox.org)
* Install [Visual Studio Code](https://code.visualstudio.com/download).
* Install extensions for Visual Studio Code: (On the Visual Studio Code: Menu -> Preferences -> Extensions)
    1. Azure IoT Tools
    2. Vagrant
* Install [Docker](https://www.docker.com/get-started).

## Create the IoTHub and the IoT Edge device

* Create an IoT Hub (S1) on Azure. You'll need to wait for it to be in an active state, it could take a while. This [guide](https://devblogs.microsoft.com/visualstudio/azure-iot-tools-help-you-connect-to-azure-iot-hub-in-1-minute-in-visual-studio-code/) helps you with the necessary steps.
* After your IoT Hub has started, you can use Visual Studio Code integration to connect to it.
* In Visual Studio Code (or on the Azure Portal), you can create a new IoT Edge device.
* Copy the contents of `.env.template` into a new file `.env`. The default settings work out of the box for the vagrant box. If you want to configure a different IoT Edge device please consult the 'Use a different device as IoT Edge Device' instructions.
* Right click on `deployment.template.json` and select 'Build IoT Edge Solution'.
* Go to the config folder, right click on `deployment.amd64.json` and select on 'Create Deployment for Single Device'.

## Prefill CrateDB with required tables and users

This setup step can be done on your laptop and will create the information in a data folder. This data folder is then shared with the IoT Edge device, so the tables will be there. (that's why we start and stop the container after running the SQL)

* Create a sub-folder in the checked out repository for the CrateDB database files: `mkdir -p crate/data`
* Run CrateDB locally from the command line by using `docker run -p "4200:4200" -d --rm --name cratedb -v /absolute/path/to/folder/crate/data:/data crate:4.1.2`. Use the absolute path to the folder you have created in the step above.
* Open a web browser with `http://localhost:4200` and go to the CrateDB Admin UI console.
* Create the tables and the user by executing all SQL Statements in the file `./scripts/database_setup/createTable.sql`. Every SQL query must be executed on it's own.
* Stop the container with `docker container stop cratedb`.


## Set up Vagrant VM as IoT Edge Device

* Right click on the device and select “Copy Device Connection String” from the Menu.
* Modify the `vagrant/config.template.yaml` to use your Device Connection String and rename it to `config.yaml`.
* In your terminal change directory `cd vagrant` and start the Vagrant box by `vagrant up`.
* Open the shh by running `vagrant ssh`.
* Restart the service inside the VM by `sudo systemctl restart iotedge`.

### Connecting to CrateDB running on the Vagrant IoT Edge Device
The Vagrantfile binds CrateDB on the edge device to the port 4200 on your system. Open `http://localhost:4200` in your browser and you will find the CrateDB Admin UI.

### Connecting to Grafana running on the Vagrant IoT Edge Device
The Vagrantfile binds Grafana on the edge device to the port 3000 in your system. Open `http://localhost:3000` in your browser and you will find the login page. Use `admin` as username and password.


## Use a different device as IoT Edge Device

You can configure different devices as IoT Edge Device. Follow these instructions to set up the device:

* Create the local database and copy the folder to the external device
* Copy the appdata folder to the external device
* Configure the .env file with the relevant paths on the external device
  * `PATH_PREFIX_EDGE` should point to the parent folder of the appdata folder (e.g. use `/path/to` when the path is `/path/to/appdata/`)
  * `PATH_PREFIX_STORAGE` should point to the partent folder of the crate folder (e.g. use `/path/to` when the path is `/path/to/crate/data`)
* Right click on `deployment.template.json` and click on 'Build IoT Edge Solution'.
* Go to the config folder, right click on `deployment.amd64.json` and click on 'Create Deployment for Single Device'.
* Consult the `vagrant/Vagrantfile` for hints on which packages you need to install on the external device. If you use Ubuntu as operating system on your device the commands can be used as they are. If you use a different operating system please install the required dependencies according to the underlying OS.
* Copy the connection string and add the Device Connection String to the config.yaml from iotedge package.
* Now this external device should behave as a IoT Edge Device.


## Configuring the OPC-UA Publisher edge module

Per default this sample uses all nodes from the [OPC-UA Server simulation](https://github.com/Azure-Samples/iot-edge-opc-plc). If you want to change the setup, just edit the `publishednodes.json` file in the folder `appdata`.
