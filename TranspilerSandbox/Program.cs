using Q_sharp.Transpiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranspilerSandbox {
    class Program {
        static void Main(string[] args) {
            TranspileQS.StartSolutionTranspile("E:\\Personal\\Projects\\Q_sharp\\TranspilerSandbox");
            Console.WriteLine("Press any button to continue...");
            Console.ReadLine();
        }
    }
}
