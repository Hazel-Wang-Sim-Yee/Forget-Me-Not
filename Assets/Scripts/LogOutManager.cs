/*
* Author: Jeffrey
* Date: 2026-02-11
* Description: This script manages the logout functionality for the game. It handles signing out the current user from Firebase and loading the login scene.
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;

public class LogOutManager : MonoBehaviour
{
    [Header("Navigation")]
    public string loginSceneName = "LoginScene"; // The name of the login scene to load after logout, set in the Unity Editor

    // This method is called to log out the current user. It first signs out of Firebase using the SignOut method, then logs a message to the console indicating that the user has been logged out. Finally, it loads the login scene specified by the loginSceneName variable to return the player to the login screen.
    public void Logout()
    {
        // 1. Sign out of Firebase
        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("User logged out.");

        // 2. Load the Login Scene
        SceneManager.LoadScene(loginSceneName);
    }
}