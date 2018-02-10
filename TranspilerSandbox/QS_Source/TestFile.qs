using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q_sharp {
    class Test {
        static void NotMain(string[] args) {
            Console.WriteLine(1);
            Console.WriteLine(1);

            fibbonaci(1, 1, 15);

            Console.ReadLine();
        }

        static int fibbonaci(int a, int b, uint count) {
            int ret = 
                a + 
                    b
                    
                    ;
            Console.WriteLine(ret);

            if(count > 0) {
                return fibbonaci(b,
                
                
                ret, --count);
            }

            return ret;
        }
    }
}
