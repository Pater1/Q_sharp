using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Q_sharp.Core{
    public static class SubStack {
        private static volatile ConcurrentQueue<IEnumerator> frames = new ConcurrentQueue<IEnumerator>();

        public static void Start(IEnumerator frame) => Enqueue(frame);
        internal static void Enqueue(IEnumerator frame){
            frames.Enqueue(frame);
            if(frames.Count == 1){//was empty before enqueue; needs kickstart
                ThreadPool.QueueUserWorkItem(Process);
            }
        }
        
        private static void Process(object o = null /*Never used; just makes ThreadPool happy*/){
            IEnumerator next;
            while(frames.TryDequeue(out next)) { //keep the thread processing until the queue is empty; will spin/yeild/sleep threads as needed automatically; only returns false when queue is empty
                if(next.MoveNext()) { //runs next segment of code; only returns false if end of method is reached
                    Enqueue(next);  
                }
            }
        }

        private static volatile ConcurrentDictionary<long, Dictionary<string, object>> returnSet = new ConcurrentDictionary<long, Dictionary<string, object>>();
        public static long Allocate(out Dictionary<string, object> value){
            long key;
            value = new Dictionary<string, object>();
            do {
                key = Extentions.RandLong();
            } while(returnSet.TryAdd(key, value));//returns false if key is in use

            return key;
        }
        public static void Push(long key, Dictionary<string, object> value) => returnSet[key] = value;
        public static Dictionary<string, object> Pull(long key) {
            Dictionary<string, object> ret;
            if(returnSet.TryRemove(key, out ret)) {//returns false if nothing is mapped to key
                return ret.DeepClone();//ensure no errors get thrown if this dictionary happened to have been created by a different thread?
            }else{
                return null;
            }
        }
    }
}
