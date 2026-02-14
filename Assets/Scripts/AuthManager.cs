/*
* Author: Jeffrey
* Date: 2026-02-09
* Description: Manages user authentication using Firebase. It provides methods for showing the login and sign-up screens, handling user input for email and password, and communicating with Firebase to authenticate users. It also handles error messages and initializes user data in the Firebase Realtime Database upon successful sign-up.
*/
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
    public FirebaseAuth auth; // Firebase Authentication instance
    private DatabaseReference dbRef; // Reference to the Firebase Realtime Database

    [Header("Screen Containers")]
    public GameObject loginContainer; // Container for the login UI elements, set in the Unity Editor
    public GameObject signUpContainer; // Container for the sign-up UI elements, set in the Unity Editor

    [Header("Login UI")]
    public TMP_InputField loginEmailField; // Input field for the user's email in the login screen, set in the Unity Editor
    public TMP_InputField loginPasswordField; // Input field for the user's password in the login screen, set in the Unity Editor

    [Header("Sign Up UI")]
    public TMP_InputField signUpEmailField; // Input field for the user's email in the sign-up screen, set in the Unity Editor
    public TMP_InputField signUpPasswordField; // Input field for the user's password in the sign-up screen, set in the Unity Editor

    [Header("Error UI")]
    public GameObject errorUI; // Container for displaying error messages, set in the Unity Editor
    public TMP_Text errorText; // Text component for displaying error messages, set in the Unity Editor

    [Header("Settings")]
    public string nextSceneName = "MainGameScene"; // The name of the scene to load after successful login, set in the Unity Editor

    // This method is called when the script instance is being loaded. It initializes the Firebase Authentication and Database references, hides the login and sign-up containers, and ensures that the error UI is hidden at the start.
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        loginContainer.SetActive(false);
        signUpContainer.SetActive(false);

        if (errorUI != null) errorUI.SetActive(false);
    }

    // This method is called when the script instance is being enabled. It shows the login screen by default when the authentication manager is enabled.
    public void ShowLoginScreen()
    {
        if (errorUI != null) errorUI.SetActive(false);
        loginContainer.SetActive(true);
        signUpContainer.SetActive(false);
    }

    // This method is called when the user chooses to sign up. It hides the login screen and shows the sign-up screen, allowing the user to enter their email and password for account creation.
    public void ShowSignUpScreen()
    {
        if (errorUI != null) errorUI.SetActive(false);
        loginContainer.SetActive(false);
        signUpContainer.SetActive(true);
    }

    // This method is called when the user attempts to log in. It retrieves the email and password from the input fields, checks if they are not empty, and then calls Firebase Authentication to sign in the user. If the login is successful, it loads the next scene. If there is an error, it handles the error and displays an appropriate message.
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

    // This method is called when the user attempts to sign up. It retrieves the email and password from the input fields, checks if they are not empty, and then calls Firebase Authentication to create a new user account. If the sign-up is successful, it initializes the user's data in the Firebase Realtime Database and shows the login screen. If there is an error, it handles the error and displays an appropriate message.
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

    // This method initializes the user's data in the Firebase Realtime Database after a successful sign-up. It creates a dictionary with default values for the user's progress and interactions in the game, and then updates the database with this information. If the initialization is successful, it shows the login screen. If there is an error, it logs the error message.
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

    // This method handles errors that occur during login and sign-up processes. It checks the type of error returned by Firebase and maps it to a user-friendly message. The message is then displayed using the ShowErrorMessage method.
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

    // This method displays an error message on the UI. It sets the text of the error message, makes the error UI visible, and starts a coroutine to hide the error message after a delay. If the error UI or text components are not assigned, it logs an error message.
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

    // This coroutine waits for a specified delay (5 seconds) before hiding the error UI. It is called by the ShowErrorMessage method to automatically hide the error message after it has been displayed for a short period of time.
    System.Collections.IEnumerator HideErrorAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        if (errorUI != null) errorUI.SetActive(false);
    }


}