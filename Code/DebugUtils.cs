using Game.Prefabs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace NetworkAdjusterCS2.Code
{
#if DEBUG
    /// <summary>
    /// List of helper methods to help me discover what can be done and what is available to me during the development process. This is not used in the final game and is 
    /// only part of the debug environment.
    /// </summary>
    internal static class DebugUtils
    {
        /// <summary>
        /// Scans the assembly of type T and finds all other types which inherit type T using reflection
        /// </summary>
        private static Type[] FindAllSubtypes<T>() where T : class
        {
            var typeList = Assembly.GetAssembly(typeof(T)).GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(T))).ToArray();
            return typeList;
        }

        /// <summary>
        /// Finds what components are attached to the provided prefab instance
        /// </summary>
        internal static List<string> GetAllComponentNamesOfPrefab(PrefabBase _prefab)
        {
            List<string> components = new List<string>();
            var typesList = FindAllSubtypes<ComponentBase>();
            foreach (Type type in typesList)
            {
                if (_prefab.TryGet(type, out _))
                {
                    components.Add(type.Name);
                }
            }
            return components;
        }
    }
#endif
}
