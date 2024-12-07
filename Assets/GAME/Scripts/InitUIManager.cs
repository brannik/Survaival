using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Core;
using System;
using Unity.Services.Authentication;

public class InitUIManager : MonoBehaviour
{
    [SerializeField] public GameObject LoginUI;
    [SerializeField] public GameObject LobbyUI;
    [SerializeField] public GameObject MainMenu;
    [SerializeField] public GameObject AccountMenu;
    [SerializeField] public TextMeshProUGUI LoggedInText;
    [Header("Login")]
    [SerializeField] public TMP_InputField Username;
    [SerializeField] public TMP_InputField Password;
    [SerializeField] public Toggle RememberLogin;
    [SerializeField] public Button BTN_Login;
    [SerializeField] public Button BTN_Register;
    [Header("Register")]
    [SerializeField] public GameObject RegisterUI;
    [SerializeField] public TMP_InputField RegUsername;
    [SerializeField] public TMP_InputField RegPassword;
    [SerializeField] public Button BTN_FinishRegister;
    [SerializeField] public Button BTN_CancelRegister;
    [Header("Main Menu")]
    [SerializeField] public Button BTN_Play;
    [SerializeField] public Button BTN_Character;
    [SerializeField] public Button BTN_Settings;
    [SerializeField] public Button BTN_LogOUT;
    [SerializeField] public Button BTN_Exit;
    [Header("Error frame")]
    [SerializeField] public ErrorWindow ErrorFrame;
    [Header("Settings UI")]
    [SerializeField] public GameObject SettingsUI;


    private static InitUIManager instance;

    // Static singleton property
    public static InitUIManager Instance
    {
        // Here we use the ?? operator, to return 'instance' if 'instance' does not equal null
        // otherwise we assign instance to a new component and return that
        get { return instance ; }
    }   
    async void Awake()
	{
		try
		{
			await UnityServices.InitializeAsync();
		}
		catch (Exception e)
		{
            ErrorFrame.ShowError("Authentication error",e.Message);
			Debug.LogException(e);
		}

        // Ensure there's only one instance
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
	}
    void Start()
    { 
        BTN_Login.onClick.AddListener(delegate{
            if(Username.text.Length == 0) return;
            if(Password.text.Length == 0) return;
            LoginUsernamePassword(Username.text,Password.text);
        });
        BTN_FinishRegister.onClick.AddListener(delegate{
            RegisterUsernamePassword(RegUsername.text,RegPassword.text);
            Debug.Log($"Register with name: {RegUsername.text}");
        });
        BTN_Settings.onClick.AddListener(delegate{
            ShowSettingsUI();
        });
        BTN_Register.onClick.AddListener(delegate{
            ShowRegisterUI();
        });
        BTN_CancelRegister.onClick.AddListener(delegate{
            ShowLoginUI();
        });
        BTN_Play.onClick.AddListener(async delegate
        {
            ShowMainGame();
            LobbyManager lmgr = FindAnyObjectByType<LobbyManager>();
            await lmgr.UpdateLobbyList();
        });
        BTN_LogOUT.onClick.AddListener(Logout);
        BTN_Exit.onClick.AddListener(delegate{
            Application.Quit();
        });
        BTN_Character.onClick.AddListener(delegate{
            ShowAccountPanel();
        });
        

        if(AuthenticationService.Instance.IsSignedIn){
            ShowMainGame();
        }else{
            if(CheckForSavedLogin()){
                List<string> userData = new List<string>();
                userData = GetSavedLoginData();
                RememberLogin.isOn = true;
                LoginUsernamePassword(userData[0],userData[1]);
            }else{
                ShowLoginUI();
            }
        }

        SetCursors(gameObject);
    }

    public void SetCursors(GameObject parent){
        Button[] buttons = parent.GetComponentsInChildren<Button>();

        // Loop through all found Buttons and add hover listeners
        foreach (Button button in buttons)
        {
            // Check if the button already has the ButtonPointerEventHandler attached
            if (button.gameObject.GetComponent<ButtonPointerEventHandler>() == null)
            {
                // Attach the custom PointerEventHandler for hover detection
                var pointerHandler = button.gameObject.AddComponent<ButtonPointerEventHandler>();
                pointerHandler.SetButton(button);
            }
        }

        TMP_InputField[] inputFields = parent.GetComponentsInChildren<TMP_InputField>();

        // Iterate over all input fields and add the handler
        foreach (TMP_InputField inputField in inputFields)
        {
            // Check if the TMP_InputField already has a TMP_InputFieldHandler
            TMP_InputFieldHandler existingHandler = inputField.GetComponent<TMP_InputFieldHandler>();
            if (existingHandler != null)
            {
                //Debug.LogWarning($"TMP_InputFieldHandler already attached to {inputField.name}");
                return; // Prevent adding multiple handlers to the same input field
            }

            // Attach the TMP_InputFieldHandler to the input field
            TMP_InputFieldHandler newHandler = inputField.gameObject.AddComponent<TMP_InputFieldHandler>();
        
            // Set the input field to the handler
            newHandler.SetInputField(inputField);
        }

        Slider[] sliders = parent.GetComponentsInChildren<Slider>();

        // Loop through all sliders and add the handler
        foreach (Slider slider in sliders)
        {
            // Check if the handler is already attached
            if (slider.gameObject.GetComponent<SliderPointerEventHandler>() == null)
            {
                // Attach the SliderPointerEventHandler
                SliderPointerEventHandler handler = slider.gameObject.AddComponent<SliderPointerEventHandler>();
                handler.SetSlider(slider);
            }
        }
    }

    private void Logout(){
        AuthenticationService.Instance.SignOut();
        ShowLoginUI();
    }
    private void ShowSettingsUI(){
        SetCursors(SettingsUI);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.openPage);
        SettingsUI.SetActive(true);
        LobbyUI.gameObject.SetActive(false);
        AccountMenu.gameObject.SetActive(false);
    }
    private bool CheckForSavedLogin(){
        string sUser = PlayerPrefs.GetString("username");
        string sPass = PlayerPrefs.GetString("password");
        return sUser.Length > 0 && sPass.Length > 0;
    }
    private List<string> GetSavedLoginData(){
        List<string> data = new List<string>();
        data.Add(PlayerPrefs.GetString("username"));
        data.Add(PlayerPrefs.GetString("password"));
        return data;
    }
    private void ShowMainGame(){
        SetCursors(MainMenu);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.openPage);
        SettingsUI.SetActive(false);
        LobbyUI.gameObject.SetActive(true);
        MainMenu.gameObject.SetActive(true);
        LoginUI.gameObject.SetActive(false);
        LoggedInText.gameObject.SetActive(true);
        RegisterUI.SetActive(false);
        AccountMenu.gameObject.SetActive(false);
    }
    private void ShowLoginUI(){
        SetCursors(LoginUI);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.openPage);
        SettingsUI.SetActive(false);
        LobbyUI.gameObject.SetActive(false);
        MainMenu.gameObject.SetActive(false);
        LoginUI.gameObject.SetActive(true);
        LoggedInText.gameObject.SetActive(false);
        RegisterUI.SetActive(false);
        AccountMenu.gameObject.SetActive(false);
    }
    private void ShowRegisterUI(){
        SetCursors(RegisterUI);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.openPage);
        SettingsUI.SetActive(false);
        RegisterUI.SetActive(true);
        LobbyUI.gameObject.SetActive(false);
        MainMenu.gameObject.SetActive(false);
        LoginUI.gameObject.SetActive(false);
        LoggedInText.gameObject.SetActive(false);
        AccountMenu.gameObject.SetActive(false);
    }
    private void ShowAccountPanel(){
        SetCursors(AccountMenu);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.openPage);
        SettingsUI.SetActive(false);
        LobbyUI.gameObject.SetActive(false);
        LoginUI.gameObject.SetActive(false);
        AccountMenu.gameObject.SetActive(true);
    }
    private async void RegisterUsernamePassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
            ShowMainGame();
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorFrame.ShowError("Registration error",ex.Message);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorFrame.ShowError("Registration error",ex.Message);
        }
    }

    private async void LoginUsernamePassword(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            if(RememberLogin.isOn){
                PlayerPrefs.SetString("username",username);
                PlayerPrefs.SetString("password",password);
                PlayerPrefs.Save();
            }else{
                PlayerPrefs.SetString("username","");
                PlayerPrefs.SetString("password","");
                PlayerPrefs.Save();
            }
            LoggedInText.text = username;
            PlayerPrefs.SetString("PlayerName",username);
            PlayerPrefs.Save();
            ShowMainGame();
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorFrame.ShowError("Login error",ex.Message);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            ErrorFrame.ShowError("Login error",ex.Message);
        }
    }
    private async void UpdatePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
            Debug.Log("Password updated.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public void HideUI(){
        gameObject.SetActive(false);
    }
    public void ShowUI(){
        gameObject.SetActive(true);
    }
    public void ToggleSettingsIngame(){
        foreach (Transform t in gameObject.transform) {
            t.gameObject.SetActive(false); // if you want to disable all
        }
        gameObject.SetActive(!gameObject.activeSelf);
        SettingsUI.SetActive(true);
        SetCursors(SettingsUI);
    }
    public void ToggleOffSettingsIngame(){
        foreach (Transform t in gameObject.transform) {
            t.gameObject.SetActive(false); // if you want to disable all
        }
        gameObject.SetActive(false);
        SettingsUI.SetActive(false);
    }
}
