using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anti_Bot.Interfaces
{
    internal interface IStateHandler
    {

        public enum States
        {
            Stop,
            Running,
            Error,
            Quit,
            Deactivated,
            Restarting,
            Starting,
            Debug,
            Playing,
            Streaming,
            Connected,
            Disconnected,
            Skip,
            Null
        }

        
        public bool SetStatus(States SetState);

        public States ReturnState();

        public void SetError(Exception Ex);

        public Exception ReturnLastException();



    }
}
