using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform logoTransform;
    [SerializeField] private Button logoButton;
    [SerializeField] private RectTransform startButton;
    [SerializeField] private RectTransform loadButton;
    [SerializeField] private RectTransform characterImage;

    [Header("Character Position Settings")]
    [SerializeField] private Vector2 characterStartPosition = new Vector2(-1000f, 0f);
    [SerializeField] private Vector2 characterFinalPosition = new Vector2(50f, 0f);
    [SerializeField] private bool useScreenWidthForStart = true;  // ȭ�� �ʺ� ���� ���� ��ġ ��� ����
    [SerializeField][Range(-2f, 0f)] private float screenWidthMultiplier = -1.2f;  // ȭ�� �ʺ� ���� ����

    [Header("Animation Settings")]
    [SerializeField] private float logoScaleDuration = 0.5f;
    [SerializeField] private float buttonsFadeInDuration = 0.3f;
    [SerializeField] private float characterSlideDuration = 0.4f;
    [SerializeField] private Ease characterSlideEase = Ease.OutBack;

    // �����Ϳ��� ��ġ �̸����⸦ ���� ����
    [SerializeField] private bool showPositionGizmos = true;
    [SerializeField] private Color gizmoColorStart = Color.red;
    [SerializeField] private Color gizmoColorEnd = Color.green;

    private void Start()
    {
        if (characterImage == null)
        {
            Debug.LogError("Character Image is not assigned!");
            return;
        }

        InitializeUI();
        logoButton.onClick.AddListener(OnLogoClick);
    }

    private void InitializeUI()
    {
        if (startButton != null) startButton.gameObject.SetActive(false);
        if (loadButton != null) loadButton.gameObject.SetActive(false);

        if (characterImage != null)
        {
            characterImage.gameObject.SetActive(true);

            // ���� ��ġ ����
            Vector2 startPos;
            if (useScreenWidthForStart)
            {
                startPos = new Vector2(Screen.width * screenWidthMultiplier, characterStartPosition.y);
            }
            else
            {
                startPos = characterStartPosition;
            }

            characterImage.anchoredPosition = startPos;
            Debug.Log($"Character initial position: {characterImage.anchoredPosition}");
        }
    }

    private void OnLogoClick()
    {
        Debug.Log("Logo clicked!");
        logoButton.interactable = false;

        logoTransform.DOScale(Vector3.zero, logoScaleDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                logoTransform.gameObject.SetActive(false);
                ShowMenuButtons();
                ShowCharacter();
            });
    }

    private void ShowMenuButtons()
    {
        if (startButton == null || loadButton == null)
        {
            Debug.LogError("Buttons are not assigned!");
            return;
        }

        startButton.gameObject.SetActive(true);
        loadButton.gameObject.SetActive(true);

        CanvasGroup startCanvasGroup = startButton.GetComponent<CanvasGroup>();
        CanvasGroup loadCanvasGroup = loadButton.GetComponent<CanvasGroup>();

        if (startCanvasGroup != null && loadCanvasGroup != null)
        {
            startCanvasGroup.alpha = 0f;
            loadCanvasGroup.alpha = 0f;

            startCanvasGroup.DOFade(1f, buttonsFadeInDuration).SetEase(Ease.OutQuad);
            loadCanvasGroup.DOFade(1f, buttonsFadeInDuration).SetEase(Ease.OutQuad);
        }
    }

    private void ShowCharacter()
    {
        if (characterImage == null) return;

        Debug.Log("Starting character animation");
        characterImage.gameObject.SetActive(true);

        characterImage.DOAnchorPosX(characterFinalPosition.x, characterSlideDuration)
            .SetEase(characterSlideEase)
            .OnComplete(() => {
                Debug.Log($"Character animation completed. Final position: {characterImage.anchoredPosition}");
            });

        // Y ��ġ�� �ִϸ��̼����� �̵�
        if (characterImage.anchoredPosition.y != characterFinalPosition.y)
        {
            characterImage.DOAnchorPosY(characterFinalPosition.y, characterSlideDuration)
                .SetEase(characterSlideEase);
        }
    }

    // Unity �����Ϳ��� ��ġ�� �ð������� Ȯ���ϱ� ���� �����
    private void OnDrawGizmos()
    {
        if (!showPositionGizmos || characterImage == null) return;

        // ���� ��ġ �����
        Vector3 startWorldPos = transform.TransformPoint(
            useScreenWidthForStart
            ? new Vector3(Screen.width * screenWidthMultiplier, characterStartPosition.y, 0)
            : new Vector3(characterStartPosition.x, characterStartPosition.y, 0));

        Gizmos.color = gizmoColorStart;
        Gizmos.DrawWireSphere(startWorldPos, 20f);
        Gizmos.DrawWireCube(startWorldPos, new Vector3(100f, 100f, 1f));

        // ���� ��ġ �����
        Vector3 endWorldPos = transform.TransformPoint(
            new Vector3(characterFinalPosition.x, characterFinalPosition.y, 0));

        Gizmos.color = gizmoColorEnd;
        Gizmos.DrawWireSphere(endWorldPos, 20f);
        Gizmos.DrawWireCube(endWorldPos, new Vector3(100f, 100f, 1f));

        // ��� ǥ��
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startWorldPos, endWorldPos);
    }
}