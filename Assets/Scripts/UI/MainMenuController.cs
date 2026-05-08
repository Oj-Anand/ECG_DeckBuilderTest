using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityTest.Services;

namespace UnityTest.UI
{
    ///<summary>
    ///Controlls the Main menu scene
    ///Handles generating UUIDs and lookup through player prefs
    ///Routes to either deck builder or deck viewer
    ///</summary>
    public class MainMenuController : MonoBehaviour
    {
        private const string DeckBuilderScene = "DeckBuilder";
        private const string DeckViewerScene = "DeckViewer";

        [Header("Buttons")]
        [SerializeField] private Button newUserButton;
        [SerializeField] private Button continueButton;

        [Header("Animation Targets")]
        [SerializeField] private CanvasGroup titleGroup;
        [SerializeField] private CanvasGroup buttonGroup;
        [SerializeField] private RectTransform titleTransform;
        [SerializeField] private RectTransform buttonGroupTransform;

        private void Awake()
        {
            newUserButton.onClick.AddListener(OnNewUserClicked);
            continueButton.onClick.AddListener(OnContinueClicked);

        }

        private void Start()
        {
            ConfigureContinueButton();
            PlayEntryAnimation();
        }

        //Continue is only availble if a UUID already exists in PlayerPrefs
        //Otherwise it is disabled but still visible so users are aware of the option
        private void ConfigureContinueButton()
        {
            bool hasExistingUser = UserSession.HasUser();
            continueButton.interactable = hasExistingUser;

            CanvasGroup cg = continueButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = continueButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = hasExistingUser ? 1f : 0.4f;
        }

        private void OnNewUserClicked()
        {
            UserSession.CreateUser();

            TransitionToScene(DeckBuilderScene);
        }

        private void OnContinueClicked()
        {
            if (!UserSession.HasUser()) return;
            TransitionToScene(DeckViewerScene);
        }

        private void TransitionToScene(string sceneName)
        {
            //Disable buttons to avoid double or phantom clicks 
            newUserButton.interactable = false;
            continueButton.interactable = false;

            Sequence exit = DOTween.Sequence();
            exit.Append(buttonGroup.DOFade(0f, 0.3f));
            exit.Join(buttonGroupTransform.DOAnchorPosY(buttonGroupTransform.anchoredPosition.y - 50f, 0.3f));
            exit.Join(titleGroup.DOFade(0f, 0.3f));
            exit.OnComplete(() => SceneManager.LoadScene(sceneName));
        }

        private void PlayEntryAnimation()
        {
            //stat everything invisible and slightly offset 
            titleGroup.alpha = 0f;
            buttonGroup.alpha = 0f;
            Vector2 titleStart = titleTransform.anchoredPosition;
            Vector2 buttonStart = buttonGroupTransform.anchoredPosition;
            titleTransform.anchoredPosition = titleStart + new Vector2(0, 50f);
            buttonGroupTransform.anchoredPosition = buttonStart + new Vector2(0, -50f);

            Sequence entry = DOTween.Sequence();
            entry.Append(titleGroup.DOFade(1f, 0.6f).SetEase(Ease.OutQuad));
            entry.Join(titleTransform.DOAnchorPos(titleStart, 0.6f).SetEase(Ease.OutQuad));
            entry.Append(buttonGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));
            entry.Join(buttonGroupTransform.DOAnchorPos(buttonStart, 0.5f).SetEase(Ease.OutQuad));
        }
    }
}

