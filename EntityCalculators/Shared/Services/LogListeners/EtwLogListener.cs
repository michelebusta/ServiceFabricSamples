using Shared.Helpers;
using Shared.Models;
using Shared.Services.LogSources;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;

namespace Shared.Services.LogListeners
{
    public class EtwLogListener : GenericLogListener
    {
        public EtwLogListener(ILogMessageEventSource source, ISettingService setting) : base(source, setting)
        {
        }

        public override async Task ProcessLogMessage(LogMessage message)
        {
            EtwEventSource.Current.Message(message);
        }
    }

    //https://github.com/jonwagner/EventSourceProxy/wiki/About-.NET-EventSource
    [EventSource(Name = "Yangles-EntityCalculatorsApp")]
    internal sealed class EtwEventSource : EventSource
    {
        public static readonly EtwEventSource Current = new EtwEventSource();

        static EtwEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { }).Wait();
        }

        // Instance constructor is private to enforce singleton semantics
        private EtwEventSource() : base() { }

        // Event keywords can be used to categorize events. 
        // Each keyword is a bit flag. A single event can be associated with multiple keywords (via EventAttribute.Keywords property).
        // Keywords must be defined as a public class named 'Keywords' inside EventSource that uses them.
        public static class Keywords
        {
            public const EventKeywords Requests = (EventKeywords)0x1L;
            public const EventKeywords ServiceInitialization = (EventKeywords)0x2L;
        }

        // Define an instance method for each event you want to record and apply an [Event] attribute to it.
        // The method name is the name of the event.
        // Pass any parameters you want to record with the event (only primitive integer types, DateTime, Guid & string are allowed).
        // Each event method implementation should check whether the event source is enabled, and if it is, call WriteEvent() method to raise the event.
        // The number and types of arguments passed to every event method must exactly match what is passed to WriteEvent().
        // Put [NonEvent] attribute on all methods that do not define an event.
        // For more information see https://msdn.microsoft.com/en-us/library/system.diagnostics.tracing.eventsource.aspx

        [NonEvent]
        public void Message(LogMessage message)
        {
            if (this.IsEnabled())
            {
                if (message.Properties != null && message.Properties.Count  > 0)
                {
                    string type = "";
                    string id = "";
                    string applicationType = "";
                    string applicationName = "";
                    string serviceType = "";
                    string serviceName = "";
                    string partitionId = "";
                    string replicationId = "";
                    string node = "";

                    message.Properties.TryGetValue(Constants.ServicePropType, out type);
                    message.Properties.TryGetValue(Constants.ServicePropId, out id);
                    message.Properties.TryGetValue(Constants.ServicePropApplicationType, out applicationType);
                    message.Properties.TryGetValue(Constants.ApplicationName, out applicationName);
                    message.Properties.TryGetValue(Constants.ServicePropServiceType, out serviceType);
                    message.Properties.TryGetValue(Constants.ServicePropServiceName, out serviceName);
                    message.Properties.TryGetValue(Constants.ServicePropPartitionId, out partitionId);
                    message.Properties.TryGetValue(Constants.ServicePropReplicationId, out replicationId);
                    message.Properties.TryGetValue(Constants.ServicePropNode, out node);

                    ServiceMessage(
                        message.Tag, 
                        message.CorrelationId, 
                        message.Message,
                        type,
                        id,
                        applicationType, 
                        applicationName,
                        serviceType,
                        serviceName,
                        partitionId,
                        replicationId,
                        node
                    );
                }
                else
                {
                    Message(message.Tag, message.CorrelationId, message.Message);
                }
            }
        }

        private const int MessageEventId = 1;
        [Event(MessageEventId, Level = EventLevel.Informational, Message = "{2}")]
        private void Message(string tag, string correlationId, string message)
        {
            if (this.IsEnabled())
            {
                WriteEvent(MessageEventId, tag, correlationId, message);
            }
        }

        private const int ServiceMessageEventId = 2;
        [Event(ServiceMessageEventId, Level = EventLevel.Informational, Message = "{11}")]
        private void ServiceMessage (
            string tag, 
            string correlationId, 
            string message,
            string type,
            string id,
            string applicationType,
            string applicationName,
            string serviceType,
            string serviceName,
            string partitionId,
            string replicationId,
            string node)
        {
            WriteEvent(ServiceMessageEventId, tag, correlationId, message, type, id, applicationType, applicationName, serviceType, serviceName, partitionId, replicationId, node);
        }
    }
}
