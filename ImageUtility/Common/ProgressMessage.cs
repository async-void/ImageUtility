using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Common
{
    public class ProgressMessage : ValueChangedMessage<int>
    {
        public ProgressMessage(int value) : base(value) { }
    }
}
