using System;
using System.Collections.Generic;
using System.Reflection;

namespace YKFramwork.ResMgr.Utils
{
    public class ReflectionUtils
    {

        public static List<T2> GetTypesList<T, T2>() where T : System.Attribute where T2 : class
        {
            List<T2> list = new List<T2>();
            var assemblies = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type t in assemblies)
            {
                var fields = t.GetFields(BindingFlags.Static
                                         | BindingFlags.Public
                                         | BindingFlags.NonPublic
                                         | BindingFlags.DeclaredOnly);
                if (typeof(T2) is Type)
                {
                    if (t.IsDefined(typeof(T), false))
                    {
                        if (!list.Contains(t as T2))
                            list.Add(t as T2);
                    }
                }

                foreach (var f in fields)
                {
                    if (f.IsDefined(typeof(T), false))
                    {
                        object o = f.GetValue(null);
                        if (o is T2)
                        {
                            if (!list.Contains(o as T2))
                                list.Add(o as T2);
                        }
                        else if (o is IEnumerable<T2>)
                        {
                            IEnumerable<T2> types = o as IEnumerable<T2>;
                            foreach (T2 tt in types)
                            {
                                if (!list.Contains(tt))
                                    list.Add(tt);
                            }
                        }
                    }
                }

            }

            //Debug.LogError(list.Count + "," + typeof(T) + "," + typeof(T2));
            return list;
        }
    }
}