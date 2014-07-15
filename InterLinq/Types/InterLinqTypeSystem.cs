using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using InterLinq.Types.Anonymous;

namespace InterLinq.Types
{
    /// <summary>
    /// Singleton class for the <see cref="InterLinqTypeSystem"/>.
    /// </summary>
    internal class InterLinqTypeSystem
    {

        #region Singleton

        private static InterLinqTypeSystem _instance;
        /// <summary>
        /// Gets an instance of the <see cref="InterLinqTypeSystem"/>.
        /// </summary>
        public static InterLinqTypeSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(InterLinqTypeSystem))
                    {
                        if (_instance == null)
                        {
                            _instance = new InterLinqTypeSystem();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Private constructor to avoid external instantiation.
        /// </summary>
        private InterLinqTypeSystem() { }

        #endregion

        #region From CLR To InterLinqMemberInfo

        private readonly Dictionary<MemberInfo, InterLinqMemberInfo> _typeMap = new Dictionary<MemberInfo, InterLinqMemberInfo>();

        /// <summary>
        /// Returns the <see cref="InterLinqMemberInfo"/> for a C# <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="memberInfo"><see cref="MemberInfo"/>.</param>
        /// <returns>Returns the <see cref="InterLinqMemberInfo"/> for a C# <see cref="MemberInfo"/>.</returns>
        public InterLinqMemberInfo GetInterLinqMemberInfo(MemberInfo memberInfo)
        {
            lock (this)
            {
                if (memberInfo == null)
                {
                    return null;
                }
                if (_typeMap.ContainsKey(memberInfo))
                {
                    return _typeMap[memberInfo];
                }

                InterLinqMemberInfo createdMemberInfo;
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Constructor:
                        createdMemberInfo = new InterLinqConstructorInfo();
                        break;
                    case MemberTypes.Field:
                        createdMemberInfo = new InterLinqFieldInfo();
                        break;
                    case MemberTypes.Method:
                        createdMemberInfo = new InterLinqMethodInfo();
                        break;
                    case MemberTypes.Property:
                        createdMemberInfo = new InterLinqPropertyInfo();
                        break;
                    case MemberTypes.TypeInfo:
                        createdMemberInfo = ((Type)memberInfo).IsAnonymous() ? new AnonymousMetaType() : new InterLinqType();
                        break;
                    default:
                        throw new Exception(string.Format("MemberInfo \"{0}\" could not be handled.", memberInfo));
                }
                _typeMap.Add(memberInfo, createdMemberInfo);
                createdMemberInfo.Initialize(memberInfo);
                return createdMemberInfo;
            }
        }

        /// <summary>
        /// Returns a <see cref="ReadOnlyCollection{T}"/> with each element in <paramref name="memberInfos"/>.
        /// The elements in <paramref name="memberInfos"/> will be converted to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Element <see cref="Type">Types</see> in the returned ReadOnlyColelction.</typeparam>
        /// <param name="memberInfos"><see cref="IEnumerable"/> of <see cref="MemberInfo">MemberInfo's</see>.</param>
        /// <returns>Returns a <see cref="ReadOnlyCollection{T}"/> with each element in <paramref name="memberInfos"/>.</returns>
        public ReadOnlyCollection<T> GetCollectionOf<T>(IEnumerable memberInfos) where T : InterLinqMemberInfo
        {
            if (memberInfos == null)
            {
                return null;
            }
            List<T> returnValue = (from MemberInfo memberInfo in memberInfos select GetInterLinqVersionOf<T>(memberInfo)).ToList();
            return new ReadOnlyCollection<T>(returnValue);
        }

        /// <summary>
        /// Returns the <see cref="InterLinqMemberInfo"/> for a C# <see cref="MemberInfo"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the requested <see cref="InterLinqMemberInfo"/></typeparam>
        /// <param name="memberInfo"><see cref="MemberInfo"/>.</param>
        /// <returns>Returns the <see cref="InterLinqMemberInfo"/> for a C# <see cref="MemberInfo"/>.</returns>
        public T GetInterLinqVersionOf<T>(MemberInfo memberInfo) where T : InterLinqMemberInfo
        {
            return (T)GetInterLinqMemberInfo(memberInfo);
        }

        #endregion

        #region From InterLinqMemberInfo to CLR

        private readonly Dictionary<InterLinqMemberInfo, MemberInfo> _interLinqTypeMap = new Dictionary<InterLinqMemberInfo, MemberInfo>();

        /// <summary>
        /// Returns true if the <see cref="InterLinqMemberInfo"/> was already constructed.
        /// </summary>
        /// <param name="memberInfo"><see cref="InterLinqMemberInfo"/>.</param>
        /// <returns>Returns true if the <see cref="InterLinqMemberInfo"/> was already constructed.</returns>
        public bool IsInterLinqMemberInfoRegistered(InterLinqMemberInfo memberInfo)
        {
            return memberInfo == null || _interLinqTypeMap.ContainsKey(memberInfo);
        }

        /// <summary>
        /// Returns the C# <see cref="MemberInfo"/> for a <see cref="InterLinqMemberInfo"/>.
        /// </summary>
        /// <typeparam name="T"><see cref="Type"/> of the requested <see cref="MemberInfo"/></typeparam>
        /// <param name="memberInfo"><see cref="InterLinqMemberInfo"/>.</param>
        /// <returns>Returns the C# <see cref="MemberInfo"/> for a <see cref="InterLinqMemberInfo"/>.</returns>
        public T GetClrVersion<T>(InterLinqMemberInfo memberInfo) where T : MemberInfo
        {
            if (memberInfo == null)
            {
                return null;
            }
            return (T)_interLinqTypeMap[memberInfo];
        }

        /// <summary>
        /// Stores the <see cref="InterLinqMemberInfo"/>-<see cref="MemberInfo"/>
        /// mapping locally.
        /// </summary>
        /// <param name="memberInfo"><see cref="InterLinqMemberInfo"/>.</param>
        /// <param name="clrMeberInfo"><see cref="MemberInfo"/>.</param>
        public void SetClrVersion(InterLinqMemberInfo memberInfo, MemberInfo clrMeberInfo)
        {
            _interLinqTypeMap.Add(memberInfo, clrMeberInfo);
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Gets the generic argument of an <see cref="IEnumerable"/>.
        /// </summary>
        /// <param name="seqType"><see cref="Type"/> to search in.</param>
        /// <returns>Returns the generic argument of an <see cref="IEnumerable"/>.</returns>
        internal static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            if (ienum == null)
            {
                return seqType;
            }
            return ienum.GetGenericArguments()[0];
        }

        /// <summary>
        /// Finds an <see cref="IEnumerable"/> in <paramref name="seqType"/>.
        /// </summary>
        /// <param name="seqType"><see cref="Type"/> to search in.</param>
        /// <returns>Returns an <see cref="IEnumerable"/>.</returns>
        public static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null)
                        return ienum;
                }
            }
            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }

        #endregion

    }
}
