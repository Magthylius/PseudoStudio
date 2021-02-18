using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Legacy;
using TMPro;
using UnityEngine.UI;
using Magthylius.LerpFunctions;

//! C: Jon
public class MainMenuManager : MenuManager
{
    enum MenuPhase
    {
        START = 0,
        MAIN
    }

    MenuPhase menuPhase = MenuPhase.START;
    [SerializeField] Launcher launcher;

    [Header("Start settings")]
    [SerializeField] Image startFiller;
    [Range(0f, 1f)] public float startFillerSpeed = 0.5f;

    [Space(10f)]
    [SerializeField] TextMeshProUGUI versionTMP;

    [Space(10f)]
    [SerializeField] Menu startMenu;
    [SerializeField] Menu nicknameMenu;
    [SerializeField] Menu lobbyMenu;

    ImageFiller startIF;

    [Header("Nickname settings")]
    [SerializeField] TMP_InputField nicknameTMPInput;
    [SerializeField] TextMeshProUGUI lobbyNicknameTMP;

    [Header("Room settings")]
    [SerializeField] RectTransform createRoomPanel;
    [SerializeField] RectTransform findRoomPanel;

    FlexibleRect createRoomFR;
    FlexibleRect findRoomFR;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        versionTMP.text = "V " + Application.version;

        startIF = new ImageFiller(startFiller, startFillerSpeed, 1f);
        startIF.OnFillComplete += EndStartPhase;

        createRoomFR = new FlexibleRect(createRoomPanel);
        findRoomFR = new FlexibleRect(findRoomPanel);
    }

    void Update()
    {
        switch (menuPhase)
        {
            case MenuPhase.START:
                startIF.Step(Time.unscaledDeltaTime);
                break;
            case MenuPhase.MAIN:
                createRoomFR.Step(Time.unscaledDeltaTime);
                findRoomFR.Step(Time.unscaledDeltaTime);
                break;
        }
    }

    #region Main Menu 
    void ChangePhase(MenuPhase phase) => menuPhase = phase;
    

    #region Start phase
    public void PNTR_ChargeStartFiller() => startIF.StartCharge();
    public void PNTR_DischargeStartFiller() => startIF.StopCharge();
    void EndStartPhase()
    {
        ChangePhase(MenuPhase.MAIN);
        CloseMenu(startMenu);

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            UpdateLobbyNickname();
            OpenMenu(lobbyMenu);
        }
        else
        {
            OpenMenu(nicknameMenu);
        }
    }
    #endregion

    #region Nickname phase
    public void BTN_ApplyName()
    {
        PlayerPrefs.SetString("PlayerName", nicknameTMPInput.text);
        UpdateLobbyNickname();

        CloseMenu(nicknameMenu);
        OpenMenu(lobbyMenu);
    }

    void UpdateLobbyNickname()
    {
        lobbyNicknameTMP.text = PlayerPrefs.GetString("PlayerName").ToUpper();
        launcher.ChangeNickname(PlayerPrefs.GetString("PlayerName"));
    }
    #endregion

    #endregion
}
