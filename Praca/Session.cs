using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praca
{
    class Session
    {
        private static int _UserID;
        private static int _EditID;
        private static Stack<object> PreviousWindow = new Stack<object>();


        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        public int EditID
        {
            get => _EditID;
            set => _EditID = value;
        }

        public object getPreviousWindow()
        {
            return PreviousWindow.Pop();
        }

        public void addPreviousWindow(object value)
        {
            PreviousWindow.Push(value);
        }
        
        public void clearPreviousWindow()
        {
            PreviousWindow.Clear();
        }
    }
}
