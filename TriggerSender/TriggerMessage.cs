using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggerSender
{
    public class TriggerMessage
    {
        public byte[] getMessageObjCubeReady(string sp)
        {
            var crmsg = new CubeReadyMessage()
            {
                StartPointCode = sp,
                Path = $"SystemScheduledJobs-{DateTime.Now.ToString("yy-MM-dd")}/{sp}",
                UserId = "0"
            };
            string bmStr = JsonConvert.SerializeObject(crmsg);
            QueueMessage qm = new QueueMessage()
            {
                Message = bmStr,
                QueueName = "cm/domain/workflow/cubereadyprocessingmessage"
            };
            string qmStr = JsonConvert.SerializeObject(qm);
            return Encoding.UTF8.GetBytes(qmStr);
        }
    }
}
