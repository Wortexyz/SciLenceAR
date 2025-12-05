using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Space]
    [Header("Login")]
    // If you're using TextMeshPro input fields, replace InputField with TMP_InputField in inspector and code.
    public InputField emailLoginField;
    public InputField passwordLoginField;
    public Button loginButton;

    [Space]
    [Header("Registration")]
    public InputField nameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField confirmPasswordRegisterField;
    public Button registerButton;

    [Space]
    [Header("Error & Status Texts")]
    // Errors:
    public TMP_Text loginErrorText;
    public TMP_Text registerErrorText;
    // Status messages for each panel:
    public TMP_Text loginStatusText;
    public TMP_Text registerStatusText;

    [Tooltip("If true the manager will LoadScene(\"GameScene\") after sign in. If false, it will just open the subject panel UI.")]
    public bool loadGameSceneAfterSignIn = false;
    [Tooltip("Name of scene to load when using scene loading.")]
    public string gameSceneName = "GameScene";

    [Tooltip("Minimum password length to validate locally before sending to Firebase.")]
    public int minPasswordLength = 6;

    private void Start()
    {
        if (loginErrorText) loginErrorText.gameObject.SetActive(false);
        if (registerErrorText) registerErrorText.gameObject.SetActive(false);

        if (loginStatusText) loginStatusText.gameObject.SetActive(false);
        if (registerStatusText) registerStatusText.gameObject.SetActive(false);

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
            // Use registerStatusText as fallback to show the dependency error if present, otherwise log only.
            if (registerStatusText)
                StartCoroutine(ShowError(registerStatusText, "Firebase dependencies unavailable: " + dependencyStatus, 5f));
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

    private IEnumerator CheckForAutoEntry()
    {
        if (auth == null)
        {
            if (UIManager.Instance != null) UIManager.Instance.OpenHomePanel();
            yield break;
        }

        if (auth.CurrentUser != null)
        {
            user = auth.CurrentUser;
            var reloadTask = user.ReloadAsync();
            yield return new WaitUntil(() => reloadTask.IsCompleted);

            // refresh user ref
            user = auth.CurrentUser;
            if (user != null)
            {
                GoToSubject();
                yield break;
            }
        }

        if (UIManager.Instance != null) UIManager.Instance.OpenHomePanel();
    }

    private void GoToSubject()
    {
        if (auth.CurrentUser != null) References.userName = auth.CurrentUser.DisplayName;

        if (loadGameSceneAfterSignIn)
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            if (UIManager.Instance != null) UIManager.Instance.OpenSubjectPanel();
        }
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth == null) return;

        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out: " + user.UserId);
                if (UIManager.Instance != null) UIManager.Instance.OpenHomePanel();
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in: " + user.UserId);
                GoToSubject();
            }
        }
    }

    private void OnDestroy()
    {
        if (auth != null)
            auth.StateChanged -= AuthStateChanged;
    }

    // ---------- LOGIN ----------
    public void Login()
    {
        // If UIManager has a dedicated login panel and it is hidden, open it first
        if (UIManager.Instance != null && UIManager.Instance.loginPanel != null && !UIManager.Instance.loginPanel.activeSelf)
        {
            if (UIManager.Instance.homePanel != null) UIManager.Instance.homePanel.SetActive(false);
            UIManager.Instance.loginPanel.SetActive(true);

            if (loginErrorText) loginErrorText.gameObject.SetActive(false);
            if (loginStatusText) loginStatusText.gameObject.SetActive(false);

            // Stop here — user now sees login UI
            return;
        }

        if (auth == null)
        {
            Debug.LogError("Auth not initialized yet.");
            StartCoroutine(ShowError(loginErrorText, "Auth not ready. Try again."));
            return;
        }

        string email = emailLoginField != null ? emailLoginField.text.Trim() : string.Empty;
        string password = passwordLoginField != null ? passwordLoginField.text : string.Empty;

        // local validation
        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowError(loginErrorText, "Email is empty"));
            return;
        }
        if (!IsValidEmail(email))
        {
            StartCoroutine(ShowError(loginErrorText, "Email format invalid"));
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            StartCoroutine(ShowError(loginErrorText, "Password is empty"));
            return;
        }

        StartCoroutine(LoginAsync(email, password));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        SetButtonsInteractable(false);
        if (loginStatusText) { loginStatusText.text = "Signing in..."; loginStatusText.gameObject.SetActive(true); }

        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        SetButtonsInteractable(true);
        if (loginStatusText) loginStatusText.gameObject.SetActive(false);

        if (loginTask.Exception != null)
        {
            var baseEx = loginTask.Exception.GetBaseException();
            FirebaseException firebaseException = baseEx as FirebaseException;
            string failedMessage = "Login Failed! ";

            if (firebaseException != null)
            {
                // safe parse of error code
                int errorCode = firebaseException.ErrorCode;
                AuthError authError = (AuthError)errorCode;
                switch (authError)
                {
                    case AuthError.InvalidEmail: failedMessage += "Email is invalid"; break;
                    case AuthError.WrongPassword: failedMessage += "Wrong password"; break;
                    case AuthError.MissingEmail: failedMessage += "Email is missing"; break;
                    case AuthError.MissingPassword: failedMessage += "Password is missing"; break;
                    case AuthError.UserNotFound: failedMessage += "No user found with this email"; break;
                    case AuthError.UserDisabled: failedMessage += "User account disabled"; break;
                    default: failedMessage += firebaseException.Message; break;
                }
            }
            else
            {
                failedMessage += baseEx != null ? baseEx.Message : "Unexpected error";
            }

            StartCoroutine(ShowError(loginErrorText, failedMessage));
        }
        else
        {
            user = loginTask.Result.User;
            References.userName = user.DisplayName;
            GoToSubject();
        }
    }

    // ---------- REGISTRATION ----------
    public void Register()
    {
        // If UIManager has a dedicated register panel and it is hidden, open it first
        if (UIManager.Instance != null && UIManager.Instance.registerPanel != null && !UIManager.Instance.registerPanel.activeSelf)
        {
            if (UIManager.Instance.homePanel != null) UIManager.Instance.homePanel.SetActive(false);
            UIManager.Instance.registerPanel.SetActive(true);

            if (registerErrorText) registerErrorText.gameObject.SetActive(false);
            if (registerStatusText) registerStatusText.gameObject.SetActive(false);

            // Stop here — user now sees register UI
            return;
        }

        string name = nameRegisterField != null ? nameRegisterField.text.Trim() : string.Empty;
        string email = emailRegisterField != null ? emailRegisterField.text.Trim() : string.Empty;
        string password = passwordRegisterField != null ? passwordRegisterField.text : string.Empty;
        string confirmPassword = confirmPasswordRegisterField != null ? confirmPasswordRegisterField.text : string.Empty;

        StartCoroutine(RegisterAsync(name, email, password, confirmPassword));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        // local validation before network call
        if (string.IsNullOrEmpty(name))
        {
            StartCoroutine(ShowError(registerErrorText, "User name is empty"));
            yield break;
        }

        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowError(registerErrorText, "Email field is empty"));
            yield break;
        }

        if (!IsValidEmail(email))
        {
            StartCoroutine(ShowError(registerErrorText, "Email format invalid"));
            yield break;
        }

        if (string.IsNullOrEmpty(password))
        {
            StartCoroutine(ShowError(registerErrorText, "Password is empty"));
            yield break;
        }

        if (password.Length < minPasswordLength)
        {
            StartCoroutine(ShowError(registerErrorText, $"Password must be at least {minPasswordLength} characters"));
            yield break;
        }

        if (password != confirmPassword)
        {
            StartCoroutine(ShowError(registerErrorText, "Passwords do not match"));
            yield break;
        }

        SetButtonsInteractable(false);
        if (registerStatusText) { registerStatusText.text = "Registering..."; registerStatusText.gameObject.SetActive(true); }

        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        SetButtonsInteractable(true);
        if (registerStatusText) registerStatusText.gameObject.SetActive(false);

        if (registerTask.Exception != null)
        {
            var baseEx = registerTask.Exception.GetBaseException();
            FirebaseException firebaseException = baseEx as FirebaseException;
            string failedMessage = "Registration Failed! ";

            if (firebaseException != null)
            {
                int errorCode = firebaseException.ErrorCode;
                AuthError authError = (AuthError)errorCode;
                switch (authError)
                {
                    case AuthError.InvalidEmail: failedMessage += "Email is invalid"; break;
                    case AuthError.WeakPassword: failedMessage += "Password is too weak"; break;
                    case AuthError.EmailAlreadyInUse: failedMessage += "Email already in use"; break;
                    default: failedMessage += firebaseException.Message; break;
                }
            }
            else
            {
                failedMessage += baseEx != null ? baseEx.Message : "Unexpected error";
            }

            StartCoroutine(ShowError(registerErrorText, failedMessage));
            yield break;
        }

        user = registerTask.Result.User;

        // Update display name
        UserProfile profile = new UserProfile { DisplayName = name };
        var updateTask = user.UpdateUserProfileAsync(profile);
        yield return new WaitUntil(() => updateTask.IsCompleted);

        if (updateTask.Exception != null)
        {
            // cleanup and inform - await DeleteAsync to avoid compiler warning
            var delTask = user.DeleteAsync();
            yield return new WaitUntil(() => delTask.IsCompleted);

            StartCoroutine(ShowError(registerErrorText, "Profile update failed"));
            yield break;
        }

        // Optionally send verification email
        var sendEmailTask = user.SendEmailVerificationAsync();
        yield return new WaitUntil(() => sendEmailTask.IsCompleted);
        // ignore failure here but log it
        if (sendEmailTask.Exception != null)
            Debug.LogWarning("Verification email send failed: " + sendEmailTask.Exception.GetBaseException().Message);

        // After registration Firebase signs the user in.
        GoToSubject();
    }

    public void LogOut()
    {
        if (auth != null)
        {
            auth.SignOut();

            // CLOSE PROFILE PANEL if it is open
            ProfileManager pm = FindObjectOfType<ProfileManager>();
            if (pm != null && pm.panelRoot != null)
            {
                pm.panelRoot.SetActive(false);
            }

            // RETURN TO HOME SCREEN
            if (UIManager.Instance != null)
                UIManager.Instance.OpenHomePanel();
        }
    }


    // Optional: reset password
    public void SendPasswordReset()
    {
        string email = emailLoginField != null ? emailLoginField.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowError(loginErrorText, "Enter email to reset password"));
            return;
        }

        if (!IsValidEmail(email))
        {
            StartCoroutine(ShowError(loginErrorText, "Email format invalid"));
            return;
        }

        StartCoroutine(SendPasswordResetAsync(email));
    }

    private IEnumerator SendPasswordResetAsync(string email)
    {
        SetButtonsInteractable(false);
        if (loginStatusText) { loginStatusText.text = "Sending reset link..."; loginStatusText.gameObject.SetActive(true); }

        var resetTask = auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => resetTask.IsCompleted);

        SetButtonsInteractable(true);
        if (loginStatusText) loginStatusText.gameObject.SetActive(false);

        if (resetTask.Exception != null)
        {
            StartCoroutine(ShowError(loginErrorText, "Failed to send reset email"));
        }
        else
        {
            StartCoroutine(ShowError(loginErrorText, "Reset email sent (check inbox).", 4f));
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

    private void SetButtonsInteractable(bool interactable)
    {
        if (loginButton) loginButton.interactable = interactable;
        if (registerButton) registerButton.interactable = interactable;
    }

    // Basic email regex validation (reasonable, not exhaustive)
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        // Simple RFC-ish check (good enough for local validation)
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
}
