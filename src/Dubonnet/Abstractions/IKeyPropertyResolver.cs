using System;
using System.Reflection;

namespace Dubonnet.Abstractions
{
    /// <summary>
    /// Defines methods for resolving the key property of entities.
    /// </summary>
    public interface IKeyPropertyResolver
        {
            /// <summary>
            /// Resolves the key property for the specified type.
            /// </summary>
            /// <param name="type">The type to resolve the key property for.</param>
            /// <returns>A <see /> instance of the key property of <paramref name="type"/>.</returns>
            DubonProperty Resolve(Type type);

            /// <summary>
            /// Resolves the key property for the specified type.
            /// </summary>
            /// <param name="type">The type to resolve the key property for.</param>
            /// <param name="isIdentity">Indicates whether the key property is an identity property.</param>
            /// <returns>A <see cref="PropertyInfo"/> instance of the key property of <paramref name="type"/>.</returns>
            DubonProperty Resolve(Type type, out bool isIdentity);
       }
    
}
