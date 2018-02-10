using System.Linq;

namespace System.Collections.Generic {
    public class Tree<T>: IEnumerable<T> {
        public T data;
        public List<Tree<T>> children;
        public IEnumerable<Tree<T>> Children {
            get {
                //yield return this;
                foreach(Tree<T> child in children) {
                    yield return child;
                    foreach(Tree<T> subChild in child.Children) {
                        yield return subChild;
                    }
                }
            }
        }

        public Tree() {
            children = new List<Tree<T>>();
        }
        public Tree(T data): this() {
            this.data = data;
        }

        public void Add(T data) {
            children.Add(new Tree<T>(data));
        }
        public void Add(Tree<T> data) {
            children.Add(data);
        }
        public void Add(IEnumerable<T> data) {
            Tree<T> t = new Tree<T>();
            foreach(T d in data){
                t.Add(d);
            }
            children.Add(t);
        }

        public void Remove(Tree<T> data) {
            children.Remove(data);
        }
        public void Remove(int index) {
            children.RemoveAt(index);
        }

        public Tree<T> Collapse(){
            Tree<T> newData = new Tree<T>();
            foreach(T line in this) {
                newData.Add(line);
            }

            this.children = newData.children;

            return this;
        }
        public List<int> IndexOf(T data){
            if(this.Contains(data)) {
                List<int> ret = new List<int>();

                if(this.data != null && !this.data.Equals(data)) {
                    for(int i = 0; i < children.Count; i++) {
                        List<int> ch = children[i].IndexOf(data);
                        if(ch != null){
                            ret.Add(i);
                            ret.AddRange(ch);
                            break;
                        }
                    }
                }

                return ret;
            } else {
                return null;
            }
        }

        public Tree<T> this[IEnumerable<int> indexes] {
            get {
                Tree<T> cur = this;
                foreach(int i in indexes){
                    cur = cur[i];
                    if(cur == null) break;
                }
                return cur;
            }
            set{
                Tree<T> cur = this;
                int[] inds = indexes.ToArray();
                for(int i = 0; i < inds.Length; i++){
                    if(i == inds.Length -1){
                        cur[i] = value;
                    }else{
                        cur = cur[i];
                    }
                }
            }
        }
        public Tree<T> this[int index] { 
            get {
                if(index < 0 || index >= children.Count) {
                    return null;
                }else{
                    return children[index];
                }
            }
            set{
                if(!(index < 0 || index >= children.Count)) {
                    children[index] = value;
                } 
            }
        }
        
        public IEnumerator<T> GetEnumerator() {
            foreach(Tree<T> child in Children) {
                yield return child.data;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            yield return data;
            foreach(Tree<T> child in children) {
                IEnumerator<T> nexts = child.GetEnumerator();
                while(nexts.MoveNext()) {
                    yield return nexts.Current;
                }
            }
        }

        public bool Replace(T from, Tree<T> to) {
            for(int i = 0; i < children.Count; i++) {
                if(children[i].data != null && children[i].data.Equals(from)) {
                    children[i] = to;
                    return true;
                }
            }
            foreach(Tree<T> child in children) {
                if(child.Replace(from, to)) {
                    return true;
                }
            }
            return false;
        }
        public bool Replace(T from, T to) {
            if(data != null && data.Equals(from)) {
                data = to;
                return true;
            } else {
                foreach(Tree<T> child in children) {
                    if(child.Replace(from, to)) {
                        return true;
                    }
                }
                return false;
            }
        }
        public void ReplaceAll(T from, T to) {
            if(data != null && data.Equals(from)) {
                data = to;
            }
            foreach(Tree<T> child in children) {
                child.Replace(from, to);
            }
        }

        //public override bool Equals(object obj) {
        //    if(obj is Tree<T>) {
        //        if(ReferenceEquals(this, obj)) {
        //            return true;
        //        }
        //        IEnumerator<T> t = GetEnumerator(), o = (obj as Tree<T>).GetEnumerator();
        //        bool canT = true, canO = true;
        //        while(canT && canO){
        //            canT = t.MoveNext();
        //            canO = o.MoveNext();

        //            if(t.Current != null)
        //            if(canO != canT || !t.Current.Equals(o.Current)){
        //                return false;
        //            }
        //        }
        //        return true;
        //    }
        //    return false;
        //}
    }
}
