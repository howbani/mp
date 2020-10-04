using MP.Dataplane.NOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MP.MergedPath.computing
{
    class LoopMechanizimAvoidance
    {
        private Packet _pck;
        public LoopMechanizimAvoidance(Packet pck)
        {
            _pck = pck;
        }


        /// <summary>
        /// return true if loop is discovred.
        /// </summary>
        public bool isLoop
        {
            get
            {

                
                string[] spliter = _pck.Path.Split('>');
                if (spliter.Length >= 4)
                {
                    string last1 = spliter[spliter.Length - 1];
                    string last2 = spliter[spliter.Length - 2];
                    string last3 = spliter[spliter.Length - 3];
                    string last4 = spliter[spliter.Length - 4];

                    if (last1 == last3 && last4 == last2)
                    {
                        Console.WriteLine("%%%>Packet:" + _pck.PID + " [ " + _pck.Path + " ] entered a loop.");
                        return true;
                    }
                        
                }
                
                return false;
            }
        }


    }
}
