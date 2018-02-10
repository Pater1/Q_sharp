using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Q_sharp.Core{
    public static class SubStack{
        private static volatile Queue<IEnumerator<object>> frames = new Queue<IEnumerator<object>>();

        public static void Start(IEnumerator<object> frame) => Enqueue(frame);
        internal static void Enqueue(IEnumerator<object> frame){
            frames.Enqueue(frame);
            if(frames.Count == 1){//was empty before enqueue; needs kickstart
                ThreadPool.QueueUserWorkItem(Process);
            }
        }
        
        private static void Process(object o = null /*Never used; just makes ThreadPool happy*/){
            while(frames.Count > 0) {
                IEnumerator<object> next = frames.Dequeue();
                if(next.MoveNext()){
                    Enqueue(next);
                }
            }
        }

        private static volatile Dictionary<long, Dictionary<string, object>> returnSet = new Dictionary<long, Dictionary<string, object>>();
        public static long Allocate(out Dictionary<string, object> value){
            long key;
            do {
                key = Extentions.RandLong();
            } while(key == 0 || returnSet.ContainsKey(key));

            value = new Dictionary<string, object>();
            returnSet.Add(key, value);

            return key;
        }
        //public Dictionary<string, object> this[long index] {
        //    get {
        //        return Pull(indexer);
        //    }
        //    set {
        //        Push[index, value];
        //    }
        //}
        public static void Push(long key, Dictionary<string, object> value) {
            if(returnSet.ContainsKey(key)) {
                returnSet[key] = value;
            }
        }
        public static Dictionary<string, object> Pull(long key) {
            if(returnSet.ContainsKey(key)) {
                Dictionary<string, object> ret = returnSet[key];
                returnSet.Remove(key);
                return ret;
            }else{
                return null;
            }
        }
    }
}
