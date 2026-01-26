using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Auth;

public class LogOutManager : MonoBehaviour
{
    [Header("Navigation")]
    public string loginSceneName = "LoginScene";

    public void Logout()
    {
        // 1. Sign out of Firebase
        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("User logged out.");

        // 2. Load the Login Scene
        SceneManager.LoadScene(loginSceneName);
    }
}