using System.Collections;
using Match3.Grid;
using UnityEngine;

namespace Match3.Anim
{
    public class AnimationController : MonoBehaviour
    {
        [SerializeField] private float swapDuration = 0.18f;
        [SerializeField] private float fallDurationPerCell = 0.06f;
        [SerializeField] private AnimationCurve easeOut = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public IEnumerator MoveTile(Tile tile, Vector3 target, float duration)
        {
            Vector3 start = tile.transform.localPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = easeOut.Evaluate(Mathf.Clamp01(t / duration));
                tile.transform.localPosition = Vector3.LerpUnclamped(start, target, p);
                yield return null;
            }
            tile.transform.localPosition = target;

            // small landing bounce
            yield return ScalePulse(tile.transform, 1.06f, 0.06f);
        }

        public IEnumerator Swap(Tile a, Tile b)
        {
            var apos = a.transform.localPosition;
            var bpos = b.transform.localPosition;
            yield return StartCoroutine(MoveTwo(a, b, bpos, apos, swapDuration));
        }

        public IEnumerator RevertSwap(Tile a, Tile b)
        {
            yield return Swap(a, b);
        }

        public IEnumerator Fall(Tile tile, Vector3 target, int cells)
        {
            float duration = Mathf.Max(0.08f, cells * fallDurationPerCell);
            yield return MoveTile(tile, target, duration);
        }

        public IEnumerator Pop(Tile tile)
        {
            yield return ScalePulse(tile.transform, 1.15f, 0.07f);
            yield return ScaleTo(tile.transform, 0f, 0.09f);
        }

        private IEnumerator MoveTwo(Tile a, Tile b, Vector3 atarget, Vector3 btarget, float duration)
        {
            Vector3 astart = a.transform.localPosition;
            Vector3 bstart = b.transform.localPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = easeOut.Evaluate(Mathf.Clamp01(t / duration));
                a.transform.localPosition = Vector3.LerpUnclamped(astart, atarget, p);
                b.transform.localPosition = Vector3.LerpUnclamped(bstart, btarget, p);
                yield return null;
            }
            a.transform.localPosition = atarget;
            b.transform.localPosition = btarget;
        }

        private static IEnumerator ScalePulse(Transform tr, float scale, float duration)
        {
            yield return ScaleTo(tr, scale, duration);
            yield return ScaleTo(tr, 1f, duration);
        }

        private static IEnumerator ScaleTo(Transform tr, float scale, float duration)
        {
            var start = tr.localScale;
            var end = Vector3.one * scale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                tr.localScale = Vector3.Lerp(start, end, t / duration);
                yield return null;
            }
            tr.localScale = end;
        }
    }
}
