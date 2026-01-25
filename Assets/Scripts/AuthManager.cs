using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    [Header("Firebase Setup")]
    public FirebaseAuth auth;

    [Header("Screen Containers")]
    public GameObject loginContainer;
    public GameObject signUpContainer;

    [Header("Login UI")]
    public TMP_InputField loginEmailField;
    public TMP_InputField loginPasswordField;

    [Header("Sign Up UI")]
    public TMP_InputField signUpEmailField;
    public TMP_InputField signUpPasswordField;

    [Header("Error UI")]
    public GameObject errorUI;
    public TMP_Text errorText;

    [Header("Settings")]
    public string nextSceneName = "MainGameScene";

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        loginContainer.SetActive(false);
        signUpContainer.SetActive(false);
        
        if (errorUI != null) errorUI.SetActive(false);
    }

    public void ShowLoginScreen()
    {
        if (errorUI != null) errorUI.SetActive(false); 
        loginContainer.SetActive(true);
        signUpContainer.SetActive(false);
    }

    public void ShowSignUpScreen()
    {
        if (errorUI != null) errorUI.SetActive(false); 
        loginContainer.SetActive(false);
        signUpContainer.SetActive(true);
    }

    public void TryLogin()
    {
        string email = loginEmailField.text;
        string password = loginPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowErrorMessage("Please fill in all fields.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                HandleError(task.Exception);
                return;
            }

            // SUCCESS
            Debug.Log("Login Successful! User: " + task.Result.User.Email);
            
            SceneManager.LoadScene(nextSceneName); 
        });
    }

    public void TrySignUp()
    {
        string email = signUpEmailField.text;
        string password = signUpPasswordField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowErrorMessage("Please fill in all fields.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                HandleError(task.Exception);
                return;
            }

            // SUCCESS
            Debug.Log("Sign Up Success! Account Created.");
            ShowLoginScreen(); 
        });
    }

    void HandleError(System.Exception exception)
    {
        Debug.LogError("Auth Error: " + exception);
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;
        if (firebaseEx != null)
        {
            ShowErrorMessage(((AuthError)firebaseEx.ErrorCode).ToString());
        }
        else
        {
            ShowErrorMessage("An unknown error occurred.");
        }
    }

    void ShowErrorMessage(string message)
    {
        if (errorUI != null)
        {
            errorUI.SetActive(true);
            if (errorText != null) errorText.text = message;
            StopAllCoroutines(); 
            StartCoroutine(HideErrorAfterDelay());
        }
    }

    System.Collections.IEnumerator HideErrorAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        if (errorUI != null) errorUI.SetActive(false);
    }
}