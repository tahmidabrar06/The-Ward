using System.Reflection;
using UnityEngine;

namespace MirzaBeig.CinematicExplosionsFree
{
    /// <summary>
    /// Generates camera shake when a Cinemachine impulse source is installed,
    /// while allowing this package to compile when Cinemachine is absent.
    /// </summary>
    public class CustomImpulse : MonoBehaviour
    {
        private void OnEnable()
        {
            foreach (MonoBehaviour component in GetComponents<MonoBehaviour>())
            {
                if (component == null || component == this ||
                    !component.GetType().Name.Contains("CinemachineImpulseSource"))
                    continue;

                MethodInfo generateImpulse = component.GetType().GetMethod(
                    "GenerateImpulse", BindingFlags.Instance | BindingFlags.Public,
                    null, System.Type.EmptyTypes, null);
                generateImpulse?.Invoke(component, null);
                return;
            }
        }
    }
}
