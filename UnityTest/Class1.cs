using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace UnityTest
{
    public class Class1
    {
        public Class1()
        {
            var container = new UnityContainer();
            container.Resolve<int>();
            var container2 = new TinyIoC.TinyIoCContainer();
        }
    }
}