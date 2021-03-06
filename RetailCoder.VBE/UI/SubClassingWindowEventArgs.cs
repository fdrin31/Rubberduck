using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Rubberduck.UI
{
    public class SubClassingWindowEventArgs : EventArgs
    {
        private readonly Message _msg;

        public Message Message
        {
            get { return _msg; }
        }

        public SubClassingWindowEventArgs(Message msg)
        {
            _msg = msg;
        }
    }
}