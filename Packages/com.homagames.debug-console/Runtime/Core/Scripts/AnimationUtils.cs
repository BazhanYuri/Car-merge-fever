using System.Collections;
using UnityEngine;

namespace HomaGames.HomaConsole
{
    public static class AnimationUtils
    {
        public static Coroutine Animate<T>(this MonoBehaviour comp, System.Action<T> animate,
            System.Func<T, T, float, T> interpolation, float duration, T start, T end)
        {
            return comp.StartCoroutine(AnimateRoutine<T>(animate, interpolation, duration, start, end));
        }

        static IEnumerator AnimateRoutine<T>(System.Action<T> animate, System.Func<T, T, float, T> interpolation,
            float duration, T start, T end, System.Action callback = null)
        {
            var remainingTime = duration;
            while (remainingTime > 0)
            {
                animate(interpolation(start, end, 1 - remainingTime / duration));
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            animate(end);
            callback?.Invoke();
        }
    }
}