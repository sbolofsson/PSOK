using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace InterLinq.Types
{
    /// <summary>
    /// InterLINQ representation of <see cref="MethodBase"/>.
    /// </summary>
    /// <seealso cref="InterLinqMemberInfo"/>
    /// <seealso cref="MethodBase"/>
    [Serializable]
    [DataContract]
    public abstract class InterLinqMethodBase : InterLinqMemberInfo
    {

        #region Properties

        /// <summary>
        /// Gets or sets the ParameterTypes.
        /// </summary>
        [DataMember]
        public List<InterLinqType> ParameterTypes { get; set; }

        #endregion

        #region Constructors / Initialization

        /// <summary>
        /// Empty constructor.
        /// </summary>
        protected InterLinqMethodBase()
        {
            ParameterTypes = new List<InterLinqType>();
        }

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="methodBase">Represented CLR <see cref="MethodBase"/>.</param>
        protected InterLinqMethodBase(MethodBase methodBase)
        {
            ParameterTypes = new List<InterLinqType>();
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            Initialize(methodBase);
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="memberInfo">Represented <see cref="MemberInfo"/></param>
        /// <seealso cref="InterLinqMemberInfo.Initialize"/>
        public override void Initialize(MemberInfo memberInfo)
        {
            base.Initialize(memberInfo);
            MethodBase methodBase = memberInfo as MethodBase;
            // ReSharper disable PossibleNullReferenceException
            foreach (ParameterInfo parameter in methodBase.GetParameters())
                // ReSharper restore PossibleNullReferenceException
            {
                ParameterTypes.Add(InterLinqTypeSystem.Instance.GetInterLinqVersionOf<InterLinqType>(parameter.ParameterType));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compares <paramref name="obj"/> to this instance.
        /// </summary>
        /// <param name="obj"><see langword="object"/> to compare.</param>
        /// <returns>True if equal, false if not.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            InterLinqMethodBase other = (InterLinqMethodBase)obj;
            if (ParameterTypes.Count != other.ParameterTypes.Count)
            {
                return false;
            }

            return !ParameterTypes.Where((t, i) => !t.Equals(other.ParameterTypes[i])).Any() && base.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see langword="object"/>.</returns>
        public override int GetHashCode()
        {
            int num = -103514268;
            ParameterTypes.ForEach(o => num ^= EqualityComparer<InterLinqType>.Default.GetHashCode(o));
            return num ^ base.GetHashCode();
        }

        #endregion

    }
}
