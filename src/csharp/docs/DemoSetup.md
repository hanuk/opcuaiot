#IotGateway Demo#
###Initial Setup###
[Note: The test harness is IotGateway.Typed.Consolerunner; you can also use IotGateway.Reflection.ConsoleRunner if you plan to load types from XML file]
1. Create a pool of IoT Hub devices for telemetry, command and events/alarms
2. Edit IotHubConfig.json with the device and IotHub host information
3. Edit OpcServerConfig.json to reflect OPC server URL
4. Edit TagConfig.json to reflect the variables being monitored. You need to use https://opcfoundation.org/ tools to find out the TagName, TagNameType, and TagNamespace. 
5. TagConfig_BA.json corresponds to the tags emitted by the simulation OPC UA server.
6. TagConfig_Omron.json corresponds to tags emitted by the NJ Sysmac Omron PLC with embedded OPC UA server

###Compilation###
1. Download .NET Eval SDK from https://www.unified-automation.com/. 
2. If you run into any refernece errors, replace the assembly references (UnifiedAutomation.UaBase.dll and UnifiedAutomation.UaClient.dll) in IotGateway.PlugIn.OpcUa project
3. Compile the solution

###Running with Building Automation Simulator###
1. Download DeviceExplorer from https://github.com/Azure/azure-iot-sdks/tree/master/tools/DeviceExplorer/DeviceExplorer. Start monitoring the device traffic.1. 
2. Run our OPC UA server
2. Run IotGateway.Typed.ConsoleRunner 

if everything goes well you should see the telemetry coming through in DeviceExplorer. If you run into any issues first check if the name of opc server host name. 
  