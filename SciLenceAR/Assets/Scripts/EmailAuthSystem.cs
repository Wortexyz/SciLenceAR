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
    public TMP_Text loginErrorText;
    public TMP_Text registerErrorText;
    public TMP_Text loginStatusText;
    public TMP_Text registerStatusText;

    [Header("Reset Password Settings")]
    public int resetCooldownSeconds = 60;

    [Tooltip("If true loads a scene after sign-in. If false opens subject panel.")]
    public bool loadGameSceneAfterSignIn = false;
    public string gameSceneName = "GameScene";

    public int minPasswordLength = 6;

    void Start()
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
            Debug.LogError("Firebase dependency error: " + dependencyStatus);
            if (registerStatusText)
                StartCoroutine(ShowError(registerStatusText, "Firebase unavailable.", 5f));
        }
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged -= AuthStateChanged;
        auth.StateChanged += AuthStateChanged;

        user = auth.CurrentUser;
        AuthStateChanged(this, null);
    }

    private IEnumerator CheckForAutoEntry()
    {
        if (auth == null)
        {
            UIManager.Instance?.OpenHomePanel();
            yield break;
        }

        if (auth.CurrentUser != null)
        {
            user = auth.CurrentUser;
            var reloadTask = user.ReloadAsync();
            yield return new WaitUntil(() => reloadTask.IsCompleted);

            user = auth.CurrentUser;
            if (user != null)
            {
                GoToSubject();
                yield break;
            }
        }

        UIManager.Instance?.OpenHomePanel();
    }

    // Navigate to subject panel after login/register
    private void GoToSubject()
    {
        if (auth.CurrentUser != null)
            References.userName = auth.CurrentUser.DisplayName;

        if (loadGameSceneAfterSignIn)
            SceneManager.LoadScene(gameSceneName);
        else
            UIManager.Instance?.OpenSubjectPanel();

        var pm = FindObjectOfType<ProfileManager>();
        pm?.RefreshProfile();
    }

    private void AuthStateChanged(object sender, System.EventArgs e)
    {
        if (auth == null) return;

        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out: " + user.UserId);
                UIManager.Instance?.OpenHomePanel();
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

    // ------------------- LOGIN -------------------
    public void Login()
    {
        if (UIManager.Instance != null &&
            UIManager.Instance.loginPanel != null &&
            !UIManager.Instance.loginPanel.activeSelf)
        {
            UIManager.Instance.homePanel?.SetActive(false);
            UIManager.Instance.loginPanel.SetActive(true);

            loginErrorText?.gameObject.SetActive(false);
            loginStatusText?.gameObject.SetActive(false);

            return;
        }

        string email = emailLoginField?.text.Trim();
        string password = passwordLoginField?.text;

        if (string.IsNullOrEmpty(email))
        {
            StartCoroutine(ShowError(loginErrorText, "Email is empty"));
            return;
        }
        if (!IsValidEmail(email))
        {
            StartCoroutine(ShowError(loginErrorText, "Invalid email format"));
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
        loginStatusText.text = "Signing in...";
        loginStatusText.gameObject.SetActive(true);

        var task = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        SetButtonsInteractable(true);
        loginStatusText.gameObject.SetActive(false);

        if (task.Exception != null)
        {
            FirebaseException fe = task.Exception.GetBaseException() as FirebaseException;
            string msg = "Login failed: ";

            if (fe != null)
            {
                switch ((AuthError)fe.ErrorCode)
                {
                    case AuthError.InvalidEmail: msg += "Invalid Email"; break;
                    case AuthError.WrongPassword: msg += "Wrong Password"; break;
                    case AuthError.UserNotFound: msg += "User not found"; break;
                    default: msg += fe.Message; break;
                }
            }

            StartCoroutine(ShowError(loginErrorText, msg));
        }
        else
        {
            user = task.Result.User;
            References.userName = user.DisplayName;
            GoToSubject();
        }
    }

    // ------------------- REGISTER -------------------
    public void Register()
    {
        if (UIManager.Instance != null &&
            UIManager.Instance.registerPanel != null &&
            !UIManager.Instance.registerPanel.activeSelf)
        {
            UIManager.Instance.homePanel?.SetActive(false);
            UIManager.Instance.registerPanel.SetActive(true);

            registerErrorText?.gameObject.SetActive(false);
            registerStatusText?.gameObject.SetActive(false);
            return;
        }

        StartCoroutine(RegisterAsync(
            nameRegisterField.text.Trim(),
            emailRegisterField.text.Trim(),
            passwordRegisterField.text,
            confirmPasswordRegisterField.text
        ));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(name))
        {
            StartCoroutine(ShowError(registerErrorText, "Name is empty"));
            yield break;
        }
        if (!IsValidEmail(email))
        {
            StartCoroutine(ShowError(registerErrorText, "Invalid email format"));
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
        registerStatusText.text = "Registering...";
        registerStatusText.gameObject.SetActive(true);

        var task = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        SetButtonsInteractable(true);
        registerStatusText.gameObject.SetActive(false);

        if (task.Exception != null)
        {
            FirebaseException fe = task.Exception.GetBaseException() as FirebaseException;
            string msg = "Registration failed: ";
            if (fe != null)
            {
                switch ((AuthError)fe.ErrorCode)
                {
                    case AuthError.EmailAlreadyInUse: msg += "Email already in use"; break;
                    case AuthError.WeakPassword: msg += "Password too weak"; break;
                    default: msg += fe.Message; break;
                }
            }
            StartCoroutine(ShowError(registerErrorText, msg));
            yield break;
        }

        user = task.Result.User;

        // Set display name
        var profileTask = user.UpdateUserProfileAsync(
            new UserProfile { DisplayName = name }
        );
        yield return new WaitUntil(() => profileTask.IsCompleted);

        GoToSubject();
    }

    public void LogOut()
    {
        auth?.SignOut();

        var pm = FindObjectOfType<ProfileManager>();
        if (pm?.panelRoot != null) pm.panelRoot.SetActive(false);

        UIManager.Instance?.OpenHomePanel();
    }

    // ------------------------------------------------------
    // ------------------- RESET PASSWORD -------------------
    // ------------------------------------------------------

    public void SendPasswordReset()
    {
        // Always use best option: OpenResetPasswordPanelFromLogin()
        if (UIManager.Instance != null &&
            UIManager.Instance.resetPasswordPanel != null)
        {
            UIManager.Instance.OpenResetPasswordPanelFromLogin();

            // Prefill with login email
            if (UIManager.Instance.resetEmailField != null &&
                emailLoginField != null)
            {
                UIManager.Instance.resetEmailField.text =
                    emailLoginField.text.Trim();
            }

            return;
        }
    }

    // Called by Reset Panel Submit Button
    public void SubmitPasswordResetFromPanel()
    {
        string email = UIManager.Instance.resetEmailField.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            UIManager.Instance.resetStatusText.text = "Enter your email.";
            return;
        }

        if (!IsValidEmail(email))
        {
            UIManager.Instance.resetStatusText.text = "Invalid email format.";
            return;
        }

        StartCoroutine(ResetAsync(email));
    }

    private IEnumerator ResetAsync(string email)
    {
        UIManager.Instance.resetStatusText.text = "Sending reset link...";
        UIManager.Instance.resetSubmitButton.interactable = false;

        var task = auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            UIManager.Instance.resetStatusText.text = "Failed to send reset email.";
            UIManager.Instance.resetSubmitButton.interactable = true;
            yield break;
        }

        UIManager.Instance.resetStatusText.text =
            "If an account exists, a reset link was sent.";

        yield return new WaitForSeconds(2f);

        UIManager.Instance.CloseResetPasswordPanel();

        StartCoroutine(ResetCooldown());
    }

    private IEnumerator ResetCooldown()
    {
        Button btn = UIManager.Instance.resetSubmitButton;
        btn.interactable = false;

        float t = 0;
        while (t < resetCooldownSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }

        btn.interactable = true;
    }

    // ------------------------------------------------------
    // Utility
    // ------------------------------------------------------

    private IEnumerator ShowError(TMP_Text t, string msg, float dur = 3f)
    {
        if (t == null) yield break;
        t.text = msg;
        t.gameObject.SetActive(true);
        yield return new WaitForSeconds(dur);
        t.gameObject.SetActive(false);
    }

    private void SetButtonsInteractable(bool state)
    {
        if (loginButton) loginButton.interactable = state;
        if (registerButton) registerButton.interactable = state;
    }

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
