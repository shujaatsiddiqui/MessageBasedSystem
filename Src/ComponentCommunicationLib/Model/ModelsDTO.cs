using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBasedService.Model
{
    // Models/MessagePayload.cs
    public class MessagePayload
    {
        public Header Header { get; set; }
        public string Payload { get; set; }
        public Error Error { get; set; }
        public State State { get; set; }
        public Priority Priority { get; set; }
        public MessageCommandType CommandType { get; set; }
    }

    public class Header
    {
        public Header()
        {

        }

        public Header(string correlationId)
        {
            CorrelationId = correlationId;
        }
        public string MessageId { get; set; }
        public DateTime Timestamp { get; private set; } = DateTime.Now;
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public Priority Priority { get; set; }
        public string CorrelationId { get; set; }
    }

    //public class Payload
    //{
    //    public string CommandType { get; set; }
    //    public string Data { get; set; } // can be any json object
    //}

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public enum State
    {
        Started = 0,
        Proceed = 1,
        Completed = 2,
        Ok = 3,
        Error = 4
    }

    public enum Priority
    {
        Low = 1,
        High = 0
    }

    public enum MessageCommandType
    {
        AllComponentsStarted = 0,
    }
}
