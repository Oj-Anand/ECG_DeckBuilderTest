using UnityEngine;
using System;
using DG.Tweening;
using UnityTest.View;
using UnityEngine.InputSystem;

namespace UnityTest.Gameplay
{
    /// <summary>
    /// Performs the multi stahe draw animation: Lift, flip, focus
    /// hold for input and settle intoa target hand slot
    /// </summary>
    public class CardAnimator : MonoBehaviour
    {
        [Header("Anchors")]
        [SerializeField] private Transform focusAnchor;

        [Header("Timing")]
        [SerializeField] private float liftDuration = 0.2f;
        [SerializeField] private float focusDuration = 0.5f;
        [SerializeField] private float settleDuration = 0.4f;

        [Header("Movement")]
        [SerializeField] private float liftHeight = 1.0f;
        [SerializeField] private float focusScale = 1.5f;

        private bool _waitingForFocusDismiss;

        public void PlayDrawSequence(
            CardView card,
            Vector3 handTargetPos, 
            Quaternion handTargetRot, 
            Vector3 handTargetScale, 
            Action onSequenceComplete)
        {
            Vector3 deckScale = card.transform.localScale;
            Vector3 focusScaleVec = deckScale * focusScale;

            Sequence seq = DOTween.Sequence();

            //Lift off the deck
            Vector3 liftPos = card.transform.position + Vector3.up * liftHeight;
            seq.Append(card.transform.DOMove(liftPos, liftDuration).SetEase(Ease.OutQuad));

            //Move to focus pos, scale up and flip faceup
            seq.Append(card.transform.DOMove(focusAnchor.position, focusDuration).SetEase(Ease.InOutCubic));
            seq.Join(card.transform.DORotateQuaternion(focusAnchor.rotation, focusDuration).SetEase(Ease.InOutCubic));
            seq.Join(card.transform.DOScale(focusScaleVec, focusDuration).SetEase(Ease.OutBack));

            //wait for player click
            seq.AppendCallback(() => _waitingForFocusDismiss = true);
            seq.AppendInterval(0f); //actual wait happens via Update()

            //Settle into hand position, scale and rotation 
            seq.Append(card.transform.DOMove(handTargetPos, settleDuration).SetEase(Ease.InOutCubic));
            seq.Join(card.transform.DORotateQuaternion(handTargetRot, settleDuration).SetEase(Ease.InOutCubic));
            seq.Join(card.transform.DOScale(handTargetScale, settleDuration).SetEase(Ease.InOutCubic));

            seq.OnComplete(() => onSequenceComplete?.Invoke());
            seq.Pause();

            StartCoroutine(RunWithFocusHold(seq)); 
        }

        private System.Collections.IEnumerator RunWithFocusHold(Sequence seq)
        {
            seq.Play();

            //wait untill _waitiingForFoucsDismiss true
            yield return new WaitUntil(() => _waitingForFocusDismiss);

            //Pause the seq at the card in focus 
            seq.Pause();

            //wait for players click
            yield return new WaitUntil(() => Mouse.current.leftButton.wasPressedThisFrame);

            _waitingForFocusDismiss = false; 
            seq.Play(); 
        }
    }
}


