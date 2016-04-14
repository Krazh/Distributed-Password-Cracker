using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackingDistributedServer
{
    class Chunk
    {
        public List<string> WordList { get; set; }
        public int UserId { get; set; }
        public TimeSpan ElapsedTime { get; set; }

        public Chunk()
        {
            WordList = new List<string>();
            UserId = new int();
            ElapsedTime = new TimeSpan();
        }
    }
}
