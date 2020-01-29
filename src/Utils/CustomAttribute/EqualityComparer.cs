using System;
using System.Collections;
using JetBrains.Annotations;

namespace ProjectCeleste.Misc.Container.Utils.CustomAttribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ContainerEqualityComparer : Attribute
    {
        [NotNull] public IEqualityComparer EqualityComparer { get; }

        public ContainerEqualityComparer([NotNull] IEqualityComparer equalityComparer)
        {
            EqualityComparer = equalityComparer;
        }

        public ContainerEqualityComparer(StringComparison stringComparison)
        {
            EqualityComparer = stringComparison switch
            {
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.InvariantCulture => StringComparer.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                StringComparison.Ordinal => StringComparer.Ordinal,
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                _ => throw new ArgumentOutOfRangeException(nameof(stringComparison), stringComparison,
                    $"Invalid {nameof(stringComparison)} value.")
            };
        }
    }
}