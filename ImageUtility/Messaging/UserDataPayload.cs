using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUtility.Messaging
{
    public class UserDataPayload
    {
        public int ResizeCount { get; set; }
        public int RenameCount { get; set; }
        public int ConversionCount { get; set; }
    }
}
