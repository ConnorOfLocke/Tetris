using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCell : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private bool isAwaitingOffset = false;
    private Vector3 delayedOffset = Vector3.zero;

    public void DestroyAfterDelay(float _delay)
    {
        StartCoroutine(_DestroyAfterDelay(_delay));
    }

    private IEnumerator _DestroyAfterDelay(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        Destroy(this.gameObject);
    }

    public void FireDestroyAnim()
    {
        animator.SetTrigger("Explode");
    }

    public void AddPositionAfterDelay(Vector3 _offset, float _delay)
    {
        if (isAwaitingOffset)
        {
            delayedOffset += _offset;
            //Debug.Log($"DelayedOffset {delayedOffset}");
        }
        else
        {
            isAwaitingOffset = true;
            delayedOffset = _offset;
            StartCoroutine(_AddPositionAfterDelay(_delay));
        }
    }

    private  IEnumerator _AddPositionAfterDelay(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        transform.position += delayedOffset;

        delayedOffset = Vector3.zero;
        isAwaitingOffset = false;
    }

}
