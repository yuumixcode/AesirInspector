using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TypeFilter 特性案例。
    /// </summary>
    [AesirExample]
    internal class TypeFilterExampleSO : AttributeExampleSO<TypeFilterExampleSO>
    {
        public abstract class BaseClass
        {
            public int BaseField;
        }

        public class A1 : BaseClass
        {
            public int _A1;
        }

        public class A2 : A1
        {
            public int _A2;
        }

        public class A3 : A2
        {
            public int _A3;
        }

        public class B1 : BaseClass
        {
            public int _B1;
        }

        public class B2 : B1
        {
            public int _B2;
        }

        public class B3 : B2
        {
            public int _B3;
        }

        public class C1<T> : BaseClass
        {
            public T C;
        }

        [Title("Parameter: FilterMethod")]
        [TypeFilter("GetFilteredTypeList")]
        public IMyInterface FilteredInstance;

        [Title("Parameter: FilterMethod")]
        [TypeFilter("GetBaseClassTypeList")]
        public BaseClass A;

        [Title("Parameter: FilterMethod")]
        [TypeFilter("GetBaseClassTypeList")]
        public BaseClass B;

        [Title("Array Element TypeFilter")]
        [TypeFilter("GetBaseClassTypeList")]
        public BaseClass[] Array = new BaseClass[3];

        public IEnumerable<Type> GetFilteredTypeList()
        {
            var q = typeof(IMyInterface).Assembly.GetTypes().Where(x => !x.IsAbstract)
                .Where(x => !x.IsInterface).Where(x => typeof(IMyInterface).IsAssignableFrom(x));
            return q;
        }

        public IEnumerable<Type> GetBaseClassTypeList()
        {
            IEnumerable<Type> q = from x in typeof(BaseClass).Assembly.GetTypes()
                where !x.IsAbstract
                where !x.IsGenericTypeDefinition
                where typeof(BaseClass).IsAssignableFrom(x)
                select x;
            q = q.AppendWith<Type>(typeof(C1<>).MakeGenericType(typeof(GameObject)));
            q = q.AppendWith<Type>(typeof(C1<>).MakeGenericType(typeof(AnimationCurve)));
            return q.AppendWith<Type>(typeof(C1<>).MakeGenericType(typeof(List<float>)));
        }

        public override void AesirInspectorReset()
        {
            FilteredInstance = null;
            A = null;
            B = null;
            Array = new BaseClass[3];
        }
    }

    public interface IMyInterface
    {
        void DoSomething();
    }

    public class MyImplementationA : IMyInterface
    {
        public int A;
        public void DoSomething() => Debug.Log("A");
    }

    public class MyImplementationB : IMyInterface
    {
        public string B;
        public void DoSomething() => Debug.Log("B");
    }
}
