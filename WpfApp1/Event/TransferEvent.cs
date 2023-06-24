using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Event
{
    public class RequestEvent : PubSubEvent<string>
    {
    }

    public class TransferEvent : PubSubEvent<string>
    {

    }
}
