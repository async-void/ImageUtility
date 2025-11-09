using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Messaging
{
    public class UserDataActivityMessage(UserDataPayload value) : ValueChangedMessage<UserDataPayload>(value)
    {
    }
}
