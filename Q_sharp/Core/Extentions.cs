using System;
using System.Security.Cryptography;
using System.Linq;
using System.Reflection;

namespace Q_sharp.Core {
    public static class Extentions {
        private static Type[] edgeCatch = new Type[] {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            
            typeof(float),
            typeof(double),
            typeof(decimal),

            typeof(char),
            typeof(string),

            typeof(bool),
        };

        internal static object DeepClone(this object toClone){
            Type t = toClone.GetType();
            if(edgeCatch.Contains(t)){
                return toClone;
            }

            object ret = Activator.CreateInstance(t);
            FieldInfo[] fields = t.GetFields();
            
            foreach(FieldInfo f in fields){
                object v = f.GetValue(toClone);
                if(!edgeCatch.Contains(f.FieldType)){
                    v = v.DeepClone();
                }
                f.SetValue(ret, v);
            }

            return ret;
        }
        
        public static T DeepClone<T>(this T toClone) {
            return (T)DeepClone((object)toClone);
        }

        private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        private static byte[] bytes = new byte[8];
        internal static long RandLong(){
            rng.GetBytes(bytes);
            long l = 0;
            for(int i = 0; i < bytes.Length; i++){
                l |= ((long)bytes[i]) << (8 * i);
            }
            return l;
        }
    }
}
