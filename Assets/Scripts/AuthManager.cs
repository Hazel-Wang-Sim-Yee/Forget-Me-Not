using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AuthManager : MonoBehaviour
{
    [Header("Firebase Setup")]
    public FirebaseAuth auth;
    private DatabaseReference dbRef;

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
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

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

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                HandleError(task.Exception);
                return;
            }

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

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                HandleError(task.Exception);
                return;
            }

            string userId = task.Result.User.UserId;
            Debug.Log("Sign Up Success! User ID: " + userId);

            InitializeUserData(userId);
        });
    }

    private void InitializeUserData(string userId)
    {
        Dictionary<string, object> userData = new Dictionary<string, object>();

        userData["current_day"] = 1;
        userData["twist_unlocked"] = false;

        Dictionary<string, object> npcs = new Dictionary<string, object>();
        npcs["ah_boon"] = false;
        npcs["mdm_wei_ting"] = false;
        npcs["mrs_raj"] = false;
        npcs["siti"] = false;
        npcs["uncle_tan"] = false;
        userData["npcs"] = npcs;

        Dictionary<string, object> recalledNpcs = new Dictionary<string, object>();
        recalledNpcs["ah_boon"] = false;
        recalledNpcs["siti"] = false;
        recalledNpcs["uncle_tan"] = false;
        recalledNpcs["mrs_raj"] = false;
        recalledNpcs["mdm_wei_ting"] = false;
        userData["recalled_npcs"] = recalledNpcs;

        dbRef.Child("users").Child(userId).UpdateChildrenAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Database Initialized for user: " + userId);
                ShowLoginScreen();
            }
            else
            {
                Debug.LogError("Failed to initialize database: " + task.Exception);
            }
        });
    }

    void HandleError(System.Exception exception)
    {
        FirebaseException firebaseEx = exception.GetBaseException() as FirebaseException;

        string message = "An error occurred.";

        if (firebaseEx != null)
        {
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            switch (errorCode)
            {
                case AuthError.WrongPassword:
                    message = "Wrong password.";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist.";
                    break;
                case AuthError.EmailAlreadyInUse:
                    message = "Email is already taken.";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid email format.";
                    break;
                case AuthError.WeakPassword:
                    message = "Password must be at least 6 characters.";
                    break;
                default:
                    message = errorCode.ToString();
                    break;
            }
        }

        ShowErrorMessage(message);
    }

    void ShowErrorMessage(string message)
    {
        if (errorUI != null && errorText != null)
        {
            errorText.text = message;

            errorUI.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(HideErrorAfterDelay());
        }
        else
        {
            Debug.LogError("Error UI or Text slots are empty in the Inspector!");
        }
    }

    System.Collections.IEnumerator HideErrorAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        if (errorUI != null) errorUI.SetActive(false);
    }


}