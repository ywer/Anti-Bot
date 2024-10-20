using Anti_Bot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anti_Bot.States
{
    internal class ProgrammState:IStateHandler
    {
        //todo: alle stats irgendwie überwachen generisch und bei änderungen melden?
        private IStateHandler.States ModulState = IStateHandler.States.Stop;
        private IStateHandler.States OldModulState = IStateHandler.States.Stop;
        private Exception LastException;

        public Exception ReturnLastException()
        {
            return LastException;
        }

        public IStateHandler.States ReturnState()
        {
            return ModulState;
        }

        public void SetError(Exception Ex)
        {
            if (Ex == null)
            {
                return;
            }
            LastException = Ex;
            return;
        }

        public bool SetStatus(IStateHandler.States SetState)
        {
            if (ModulState == SetState)
            {
                return false;
            }
            OldModulState = ModulState;
            ModulState = SetState;
            return true;

        }
    }
}
