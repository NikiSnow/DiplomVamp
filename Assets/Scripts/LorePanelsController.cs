using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LorePanelsController : MonoBehaviour
{

    [Serializable]
    public class LorePanelBlock
    {
        [Header("Panel Index")]
        public int PanelIndex = 0;

        [Header("Open Button")]
        public Button OpenPanelButton;

        [Header("Panel Root")]
        public GameObject Panel;

        [Header("Close Button")]
        public Button ClosePanelButton;

        [Header("Start State")]
        public bool HideOnStart = true;
    }

    [Header("Behaviour")]
    [SerializeField] private bool CloseOtherPanelsOnOpen = true;
    [SerializeField] private bool LogDebug = false;

    [Header("Lore Panels")]
    [SerializeField] private LorePanelBlock[] Panels = Array.Empty<LorePanelBlock>();

    private UnityAction[] openActions = Array.Empty<UnityAction>();
    private UnityAction[] closeActions = Array.Empty<UnityAction>();


    [SerializeField] float TimeForPanel1 = 60f;
    [SerializeField] float TimeForPanel2 = 120f;
    [SerializeField] float TimeForPanel3 = 360f;
    float RecordTime = 0;


    private void Awake()
    {
        CreateButtonActions();
        BindButtons();
    }

    private void Start()
    {
        RecordTime = PlayerPrefs.GetFloat("Gametime", 0f);
        ApplyStartState();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void CreateButtonActions()
    {
        openActions = new UnityAction[Panels.Length];
        closeActions = new UnityAction[Panels.Length];

        for (int i = 0; i < Panels.Length; i++)
        {
            int capturedIndex = i;

            openActions[i] = () =>
            {
                OpenPanelByArrayIndex(capturedIndex);
            };

            closeActions[i] = () =>
            {
                ClosePanelByArrayIndex(capturedIndex);
            };
        }
    }

    private void BindButtons()
    {
        for (int i = 0; i < Panels.Length; i++)
        {
            LorePanelBlock block = Panels[i];

            if (block == null)
                continue;

            if (block.OpenPanelButton != null && i < openActions.Length)
                block.OpenPanelButton.onClick.AddListener(openActions[i]);

            if (block.ClosePanelButton != null && i < closeActions.Length)
                block.ClosePanelButton.onClick.AddListener(closeActions[i]);
        }
    }

    private void UnbindButtons()
    {
        for (int i = 0; i < Panels.Length; i++)
        {
            LorePanelBlock block = Panels[i];

            if (block == null)
                continue;

            if (block.OpenPanelButton != null && i < openActions.Length)
                block.OpenPanelButton.onClick.RemoveListener(openActions[i]);

            if (block.ClosePanelButton != null && i < closeActions.Length)
                block.ClosePanelButton.onClick.RemoveListener(closeActions[i]);
        }
    }

    private void ApplyStartState()
    {
        for (int i = 0; i < Panels.Length; i++)
        {
            LorePanelBlock block = Panels[i];

            if (block == null)
                continue;

            if (block.Panel == null)
                continue;

            if (block.HideOnStart)
                block.Panel.SetActive(false);
        }
    }

    public void OpenPanelByIndex(int panelIndex)
    {
        int arrayIndex = FindArrayIndexByPanelIndex(panelIndex);

        if (arrayIndex < 0)
        {
            DebugLog($"Panel with index {panelIndex} was not found.");
            return;
        }

        OpenPanelByArrayIndex(arrayIndex);
    }

    public void ClosePanelByIndex(int panelIndex)
    {
        int arrayIndex = FindArrayIndexByPanelIndex(panelIndex);

        if (arrayIndex < 0)
        {
            DebugLog($"Panel with index {panelIndex} was not found.");
            return;
        }

        ClosePanelByArrayIndex(arrayIndex);
    }

    public void OpenPanelByArrayIndex(int arrayIndex)
    {
        if (!IsValidArrayIndex(arrayIndex))
            return;

        LorePanelBlock block = Panels[arrayIndex];

        if (block == null)
            return;

        if (block.Panel == null)
            return;

        if (!CanOpenPanel(block))
        {
            OnPanelLocked(block);
            return;
        }

        if (CloseOtherPanelsOnOpen)
            CloseAllPanels();

        block.Panel.SetActive(true);

        OnPanelOpened(block);
    }

    public void ClosePanelByArrayIndex(int arrayIndex)
    {
        if (!IsValidArrayIndex(arrayIndex))
            return;

        LorePanelBlock block = Panels[arrayIndex];

        if (block == null)
            return;

        if (block.Panel == null)
            return;

        block.Panel.SetActive(false);

        OnPanelClosed(block);
    }

    public void CloseAllPanels()
    {
        for (int i = 0; i < Panels.Length; i++)
        {
            LorePanelBlock block = Panels[i];

            if (block == null)
                continue;

            if (block.Panel == null)
                continue;

            block.Panel.SetActive(false);
        }
    }

    private bool CanOpenPanel(LorePanelBlock block)
    {
        /*
         * ЗАГЛУШКА ПОД БУДУЩУЮ ПРОВЕРКУ РАЗБЛОКИРОВКИ ЛОРА.
         *
         * Здесь потом будет код проверки времени игровой сессии.
         *
         * Например логика может быть такая:
         *
         * if (block.PanelIndex == 0)
         *     return bestGameplaySessionSeconds >= 60f;
         *
         * if (block.PanelIndex == 1)
         *     return bestGameplaySessionSeconds >= 180f;
         *
         * if (block.PanelIndex == 2)
         *     return bestGameplaySessionSeconds >= 300f;
         *
         * return false;
         *
         * Сейчас возвращаем true, чтобы все панели открывались сразу.
         */
        if (block.PanelIndex == 0)
            return RecordTime >= TimeForPanel1;

        if (block.PanelIndex == 1)
            return RecordTime >= TimeForPanel2;

        if (block.PanelIndex == 2)
            return RecordTime >= TimeForPanel3;

        return false;

        //return true;
    }

    private void OnPanelLocked(LorePanelBlock block)
    {
        /*
         * ЗАГЛУШКА ДЛЯ СЛУЧАЯ, ЕСЛИ ЛОР ЕЩЕ ЗАКРЫТ.
         *
         * Здесь потом можно сделать:
         * - звук закрытой кнопки;
         * - popup "Survive longer to unlock";
         * - тряску кнопки;
         * - текст с прогрессом разблокировки.
         */

        DebugLog($"Panel is locked. Index: {block.PanelIndex}");
    }

    private void OnPanelOpened(LorePanelBlock block)
    {
        DebugLog($"Panel opened. Index: {block.PanelIndex}");
    }

    private void OnPanelClosed(LorePanelBlock block)
    {
        DebugLog($"Panel closed. Index: {block.PanelIndex}");
    }

    private int FindArrayIndexByPanelIndex(int panelIndex)
    {
        for (int i = 0; i < Panels.Length; i++)
        {
            LorePanelBlock block = Panels[i];

            if (block == null)
                continue;

            if (block.PanelIndex == panelIndex)
                return i;
        }

        return -1;
    }

    private bool IsValidArrayIndex(int arrayIndex)
    {
        return arrayIndex >= 0 && arrayIndex < Panels.Length;
    }

    private void DebugLog(string message)
    {
        if (!LogDebug)
            return;

        Debug.Log($"[LorePanelsController] {message}");
    }
}