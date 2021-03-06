namespace cratedbsaver
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Npgsql;
    using Npgsql.CrateDb;
    using System.Data.Common;

    class Program
    {
        static int counter;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            NpgsqlDatabaseInfo.RegisterFactory(new CrateDbDatabaseInfoFactory());

            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub.
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
               return await WriteToCreateDBWithNpgSQL(message, messageString, userContext);
            } else
                return MessageResponse.Completed;
        }

        static async Task<MessageResponse> WriteToCreateDBWithNpgSQL(Message message, string messageString, object userContext) {

            string crateDBConnectionString = System.Environment.GetEnvironmentVariable("CrateDBConnString");

            using (var conn = new NpgsqlConnection(crateDBConnectionString))
            {
                conn.Open();

                // Insert some data
                using (var cmd = new NpgsqlCommand())
                {
                    // Use the enqueued timestamp from IoT Hub as event timestamp
                    DateTime ts = message.EnqueuedTimeUtc;

                    // The device ID is taken from message
                    string deviceID = message.ConnectionDeviceId != null ? message.ConnectionDeviceId : "Unknown";

                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO doc.raw (iothub_enqueuedtime,iothub_connection_device_id,payload) VALUES (@iothubtime,@deviceid,@payload)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("iothubtime", ts);
                    cmd.Parameters.AddWithValue("deviceid", deviceID);
                    string payloadString = messageString.StartsWith('{') ? messageString : ("{\"data\" : " + messageString + "}");
                    cmd.Parameters.AddWithValue("payload", payloadString);

                    try {
                        bool wasSuccessfull = await cmd.ExecuteNonQueryAsync() == 1;
                        if (!wasSuccessfull)
                        {
                            Console.WriteLine($"Error writing message to Crate DB: {payloadString}");
                        } else
                        {
                            Console.WriteLine("Successfully written message to Crate DB");
                        }
                    } catch (Exception dbEx) {
                        Console.WriteLine("Exception writing message to Crate DB. " + dbEx.Message);
                    }
                }
            }

            return MessageResponse.Completed;
        }
    }
}
