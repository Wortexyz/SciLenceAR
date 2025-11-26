using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Space]
    [Header("Login")]
    public InputField emailLoginField;
    public InputField passwordLoginField;

    [Space]
    [Header("Registration")]
    public InputField nameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField confirmPasswordRegisterField;

    [Space]
    [Header("Error Texts")]
    public TMP_Text loginErrorText;
    public TMP_Text registerErrorText;

    [Tooltip("If true the manager will LoadScene(\"GameScene\") after sign in. If false, it will just open the subject panel UI.")]
    public bool loadGameSceneAfterSignIn = false;
    [Tooltip("Name of scene to load when using scene loading.")]
    public string gameSceneName = "GameScene";

    private void Start()
    {
        if (loginErrorText) loginErrorText.gameObject.SetActive(false);
        if (registerErrorText) registerErrorText.gameObject.SetActive(false);

        StartCoroutine(CheckAndFixDependenciesAsync());
    }

    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        dependencyStatus = dependencyTask.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializeFirebase();
            yield return new WaitForEndOfFrame();
            StartCoroutine(CheckForAutoEntry());
        }
        else
        {
            Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
        }
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;

        // avoid duplicate subscriptions
        auth.StateChanged -= AuthStateChanged;
        auth.StateChanged += AuthStateChanged;

        user = auth.CurrentUser;
        AuthStateChanged(this, null);
    }

    // Decide where the user should land:
    private IEnumerator CheckForAutoEntry()
    {
        // If auth not ready just fallback to Home
        if (auth == null)
        {
            if (UIManager.Instance != null) UIManager.Instance.OpenHomePanel();
            yield break;
        }

        if (auth.CurrentUser != null)
        {
            // reload to ensure token and profile are fresh
            user = auth.CurrentUser;
            var reloadTask = user.ReloadAsync();
            yield return new WaitUntil(() => reloadTask.IsCompleted);

            if (auth.CurrentUser != null)
            {
                // already signed in => go straight to subject
                GoToSubject();
                yield break;
            }
        }

        // not signed in => show Home where player picks login/register/google
        if (UIManager.Instance != null) UIManager.Instance.OpenHomePanel();
    }

    private void GoToSubject()
    {
        // Save display name for later if you use it
        if (auth.CurrentUser != null) References.userName = auth.CurrentUser.DisplayName;

        if (loadGameSceneAfterSignIn)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            if (UIManager.Instance != null) UIManager.Instance.OpenSubjectPanel();
        }
    }

    // called when auth status changes (sign-in/out)
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth == null) return;

        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out: " + user.UserId);
                // show Home so user can choose login/register again
                if (UIManager.Instance != null) UIManager.Instance.OpenHomePanel();
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in: " + user.UserId);
                // on sign-in, go to subject
                GoToSubject();
            }
        }
    }

    // LOGIN
    public void Login()
    {
        if (auth == null)
        {
            Debug.LogError("Auth not initialized yet.");
            StartCoroutine(ShowError(loginErrorText, "Auth not ready. Try again."));
            return;
        }

        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Login Failed! ";

            switch (authError)
            {
                case AuthError.InvalidEmail: failedMessage += "Email is invalid"; break;
                case AuthError.WrongPassword: failedMessage += "Wrong Password"; break;
                case AuthError.MissingEmail: failedMessage += "Email is missing"; break;
                case AuthError.MissingPassword: failedMessage += "Password is missing"; break;
                default: failedMessage = "Login Failed"; break;
            }

            StartCoroutine(ShowError(loginErrorText, failedMessage));
        }
        else
        {
            user = loginTask.Result.User;
            References.userName = user.DisplayName;
            // on success, go to subject (or load scene)
            GoToSubject();
        }
    }

    // REGISTRATION
    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(name))
        {
            StartCoroutine(ShowError(registerErrorText, "User Name is empty"));
            yield break;
        }
        else if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowError(registerErrorText, "Email field is empty"));
            yield break;
        }
        else if (password != confirmPassword)
        {
            StartCoroutine(ShowError(registerErrorText, "Passwords do not match"));
            yield break;
        }

        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Registration Failed! ";
            switch (authError)
            {
                case AuthError.InvalidEmail: failedMessage += "Email is invalid"; break;
                case AuthError.WeakPassword: failedMessage += "Password is too weak"; break;
                case AuthError.EmailAlreadyInUse: failedMessage += "Email already in use"; break;
                default: failedMessage = "Registration Failed"; break;
            }

            StartCoroutine(ShowError(registerErrorText, failedMessage));
            yield break;
        }

        // Create user succeeded — user is automatically signed in by Firebase.
        user = registerTask.Result.User;

        UserProfile profile = new UserProfile { DisplayName = name };
        var updateTask = user.UpdateUserProfileAsync(profile);
        yield return new WaitUntil(() => updateTask.IsCompleted);

        if (updateTask.Exception != null)
        {
            // cleanup and inform
            user.DeleteAsync();
            StartCoroutine(ShowError(registerErrorText, "Profile update failed"));
            yield break;
        }

        // After registration Firebase signs the user in. AuthStateChanged will call GoToSubject().
        // But in case you want immediate behavior here, call GoToSubject() as well:
        GoToSubject();
    }

    public void LogOut()
    {
        if (auth != null)
        {
            auth.SignOut();
            // AuthStateChanged will open HomePanel
        }
    }

    private IEnumerator ShowError(TMP_Text errorText, string message, float duration = 3f)
    {
        if (errorText == null)
        {
            Debug.LogWarning("ShowError called but errorText is null. Message: " + message);
            yield break;
        }

        errorText.text = message;
        errorText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }
}