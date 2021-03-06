﻿/*
Copyright(c) Microsoft.All rights reserved.

The MIT License(MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
 
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using IotGateway.Common;
using IotGateway.Core;
using IotGateway.Host;
using IotGateway.PlugIn.IotHub;
using IotGateway.PlugIn.OpcUa;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeC;

namespace IotGateway.Typed.ConsoleRunner
{
    class Program
    {
        public static readonly string TELEMETRY_NAMESPACE = "TELEMETRY";
        public static readonly string COMMAND_NAMESPACE = "COMMAND";
        public static readonly string ALARM_NAMESPACE = "ALARM";
        static void Main(string[] args)
        {
            TypeContainer tc = GetTypeContainerFromCode();
            Run(tc);
        }
        private static TypeContainer GetTypeContainerFromCode()
        {
            TypeContainer tc = TypeContainer.Instance;

            ////register PlugIn libraries

            //register OPC receivers, converters
            tc.Register<ILogger, ConsoleLogger>();
            tc.Register<IReceiver, OpcReceiver<IMessage>>(TELEMETRY_NAMESPACE);
            tc.Register<IConverter<object, Tag>, OpcTagToTagStateConverter>(TELEMETRY_NAMESPACE);

            //register IotHub sender
            tc.Register<IConverter<Tag, object>, ToIotHubMessageConverter>(TELEMETRY_NAMESPACE);
            tc.Register<ISender, IoTHubTelemetrySender<Tag>>(TELEMETRY_NAMESPACE);

            //register additional libraries from Azure Connector; we don't have to Dictionary<string, object>ally load them
            //  as there are not expected to be changed by the PlugIn writers
            tc.Register<IServiceHost, TelemetryHost>(TELEMETRY_NAMESPACE);

            //Register IotHub command receiver
            tc.Register<IReceiver, IotHubCommandReceiver<Command>>(COMMAND_NAMESPACE);
            tc.Register<IConverter<string, Command>, FromIotHubCommandConverter>(COMMAND_NAMESPACE);
            tc.Register<ICommandExecutor<Command>,OpcUaCommandExecutor> (COMMAND_NAMESPACE);
            tc.Register<IServiceHost, CommandHost>(COMMAND_NAMESPACE);
            //the following generates XML to be used in reflection based ConsoleRunner. 
            string xml = tc.GetRegistryAsXml();
            return tc;
        }

        private static void Run(TypeContainer tc)
        {
            Dictionary<string, object> settings = new Dictionary<string, object>();

            settings["IotHubConfigFile"] = "IotHubConfig.json";
            settings["OpcServerConfigFile"] = "OpcServerConfig.json";
            settings["OpcTagConfigFile"] = "TagConfig.json";

            //Configure TelemetryHost
            IServiceHost telemetryHost = tc.GetInstance<IServiceHost>(TELEMETRY_NAMESPACE);
            //until we have constructor support in TypeContainer, we will need to initialize the namespace separately
            telemetryHost.SetContainerNamespace(TELEMETRY_NAMESPACE);
            IProcessor telemetryHostProcessor = (IProcessor)telemetryHost;
            telemetryHostProcessor.Initialize(settings);

            //Configure ControlHost
            IServiceHost commandHost = tc.GetInstance<IServiceHost>(COMMAND_NAMESPACE);
            commandHost.SetContainerNamespace(COMMAND_NAMESPACE);
            IProcessor commandHostProcessor = (IProcessor)commandHost;
            commandHostProcessor.Initialize(settings);


            System.Console.WriteLine("Press ENTER to Start IoT Gateway");
            System.Console.ReadLine();

            telemetryHost.Start();
            commandHost.Start();

            //wait for the user to exit

            System.Console.WriteLine("Press ENTER to exit");
            System.Console.ReadLine();
        }
    }
}
