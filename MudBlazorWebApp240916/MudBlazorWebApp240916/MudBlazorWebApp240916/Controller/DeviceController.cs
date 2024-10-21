using Microsoft.AspNetCore.Mvc;
using MudBlazorWebApp240916.Shared.DataModel;
using System.Collections.Generic;

namespace MudBlazorWebApp240916.Server.Controllers
{
    [Route("api/device")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        // Example: This would come from a database or service
        private static List<Device> AvailableDevices = new List<Device>
        {
            new Device { Name = "Raspberry Pi", Address = "192.168.1.2", Description = "Main Raspberry Pi", MqttServer = "localhost", MqttPort = "1883", IsExpanded = false },
            new Device { Name = "Arduino", Address = "192.168.1.3", Description = "Arduino Device", MqttServer = "localhost", MqttPort = "1883", IsExpanded = false }
        };

        private static List<Device> ConnectedDevices = new List<Device>();

        [HttpGet("available")]
        public ActionResult<List<Device>> GetAvailableDevices ()
        {
            return Ok(AvailableDevices);
        }

        [HttpGet("connected")]
        public ActionResult<List<Device>> GetConnectedDevices ()
        {
            return Ok(ConnectedDevices);
        }

        [HttpPost("connect")]
        public ActionResult ConnectDevice ([FromBody] Device device)
        {
            AvailableDevices.Remove(device);
            ConnectedDevices.Add(device);
            return Ok();
        }
    }
}
